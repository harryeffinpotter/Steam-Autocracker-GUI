using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SAC_GUI
{
    public class OneFichierUploader : IDisposable
    {
        private const string API_KEY = "PSd6297MACENE2VQD7eNxBWIKrrmTTZb";
        private const string API_BASE_URL = "https://api.1fichier.com/v1";
        private readonly HttpClient httpClient;
        private bool disposed = false;

        public OneFichierUploader()
        {
            // Disable auto-redirect to capture the redirect URL with upload ID
            // Enable cookies in case 1fichier needs them
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SACGUI/1.0");
        }

        public class UploadServerInfo
        {
            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public class UploadResult
        {
            public string DownloadUrl { get; set; }
            public string FileName { get; set; }
            public long FileSize { get; set; }
            public string UploadId { get; set; }
        }

        public async Task<UploadServerInfo> GetUploadServerAsync()
        {
            try
            {
                // Use WebRequest for more control over headers (1fichier needs Content-Type on GET)
                var request = (HttpWebRequest)WebRequest.Create($"{API_BASE_URL}/upload/get_upload_server.cgi");
                request.Method = "GET";
                request.Headers.Add("Authorization", $"Bearer {API_KEY}");
                request.ContentType = "application/json";

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var json = await reader.ReadToEndAsync();
                    System.Diagnostics.Debug.WriteLine($"[1FICHIER] Response Status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"[1FICHIER] Response: {json}");

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var serverInfo = JsonConvert.DeserializeObject<UploadServerInfo>(json);
                        System.Diagnostics.Debug.WriteLine($"[1FICHIER] Got upload server: {serverInfo.Url}, ID: {serverInfo.Id}");
                        return serverInfo;
                    }
                    else
                    {
                        throw new Exception($"Failed to get upload server: {response.StatusCode}. Response: {json}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Error getting upload server: {ex.Message}");
                throw;
            }
        }

        public async Task<UploadResult> UploadFileAsync(string filePath, IProgress<double> progressCallback = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var fileInfo = new FileInfo(filePath);
                var fileName = Path.GetFileName(filePath);
                var fileSize = fileInfo.Length;

                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Uploading file: {fileName}, Size: {fileSize / (1024 * 1024)}MB");

                // Step 1: Get upload server
                var serverInfo = await GetUploadServerAsync();

                // Step 2: Upload the file
                var uploadUrl = $"https://{serverInfo.Url}/upload.cgi?id={serverInfo.Id}";
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Upload URL: {uploadUrl}");

                // Let's debug what's actually happening
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Starting upload of: {filePath}");
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] File exists: {File.Exists(filePath)}");
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] File size: {new FileInfo(filePath).Length}");

                // Manual HTTP request to exactly match Python's requests library
                var request = (HttpWebRequest)WebRequest.Create(uploadUrl);
                request.Method = "POST";
                request.AllowAutoRedirect = false;
                request.Headers.Add("Authorization", $"Bearer {API_KEY}");

                // Create boundary
                string boundary = "----WebKitFormBoundary" + Guid.NewGuid().ToString("N");
                request.ContentType = $"multipart/form-data; boundary={boundary}";

                progressCallback?.Report(0.1);

                using (var requestStream = request.GetRequestStream())
                {
                    var encoding = System.Text.Encoding.UTF8;

                    // Write domain field
                    var domainHeader = encoding.GetBytes($"--{boundary}\r\nContent-Disposition: form-data; name=\"domain\"\r\n\r\n0\r\n");
                    requestStream.Write(domainHeader, 0, domainHeader.Length);

                    // Write file field header
                    var fileHeader = encoding.GetBytes($"--{boundary}\r\nContent-Disposition: form-data; name=\"file[]\"; filename=\"{Path.GetFileName(filePath)}\"\r\nContent-Type: application/octet-stream\r\n\r\n");
                    requestStream.Write(fileHeader, 0, fileHeader.Length);

                    // Write file content
                    var fileBytes = File.ReadAllBytes(filePath);
                    requestStream.Write(fileBytes, 0, fileBytes.Length);

                    System.Diagnostics.Debug.WriteLine($"[1FICHIER] Wrote {fileBytes.Length} bytes to request stream");

                    // Write closing boundary
                    var closingBoundary = encoding.GetBytes($"\r\n--{boundary}--\r\n");
                    requestStream.Write(closingBoundary, 0, closingBoundary.Length);

                    // Debug: show exactly what we're sending
                    var debugBody = encoding.GetString(domainHeader) + $"[{fileBytes.Length} file bytes]" + encoding.GetString(closingBoundary);
                    System.Diagnostics.Debug.WriteLine($"[1FICHIER] Multipart body structure:\n{debugBody}");
                }

                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Sending request to {uploadUrl}");

                progressCallback?.Report(0.5);

                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                progressCallback?.Report(0.9);

                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Upload response status: {response.StatusCode}");

                // Log all response headers
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Response headers:");
                foreach (string key in response.Headers.AllKeys)
                {
                    System.Diagnostics.Debug.WriteLine($"[1FICHIER]   {key}: {response.Headers[key]}");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                    response.StatusCode == System.Net.HttpStatusCode.Found ||
                    response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                {
                    // Get redirect location
                    var location = response.Headers["Location"];

                    System.Diagnostics.Debug.WriteLine($"[1FICHIER] Upload complete! Redirected to: {location}");

                    // Extract upload ID from location
                    string uploadId = null;
                    if (!string.IsNullOrEmpty(location) && location.Contains("xid="))
                    {
                        var xidStart = location.IndexOf("xid=") + 4;
                        var xidEnd = location.IndexOf("&", xidStart);
                        if (xidEnd == -1) xidEnd = location.Length;
                        uploadId = location.Substring(xidStart, xidEnd - xidStart);
                    }

                    if (!string.IsNullOrEmpty(uploadId))
                    {
                        // Step 3: Get download links
                        var downloadInfo = await GetDownloadLinksAsync(serverInfo.Url, uploadId);

                        progressCallback?.Report(1.0); // Complete

                        return new UploadResult
                        {
                            DownloadUrl = downloadInfo?.DownloadUrl,
                            FileName = fileName,
                            FileSize = fileSize,
                            UploadId = uploadId
                        };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Sometimes 1fichier returns 200 OK with the file URL in the response
                    string responseContent;
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        responseContent = reader.ReadToEnd();
                    }

                    System.Diagnostics.Debug.WriteLine($"[1FICHIER] Response body preview: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");

                    // Check if response contains a file URL (usually starts with https://1fichier.com/?)
                    var urlMatch = System.Text.RegularExpressions.Regex.Match(responseContent, @"https://1fichier\.com/\?[\w]+");
                    if (urlMatch.Success)
                    {
                        var downloadUrl = urlMatch.Value;
                        System.Diagnostics.Debug.WriteLine($"[1FICHIER] Found download URL in response: {downloadUrl}");

                        progressCallback?.Report(1.0); // Complete

                        return new UploadResult
                        {
                            DownloadUrl = downloadUrl,
                            FileName = fileName,
                            FileSize = fileSize,
                            UploadId = ""
                        };
                    }

                    // Try to extract file ID from HTML
                    var fileIdMatch = System.Text.RegularExpressions.Regex.Match(responseContent, @"https://1fichier\.com/\?(\w+)");
                    if (fileIdMatch.Success && fileIdMatch.Groups.Count > 1)
                    {
                        var fileId = fileIdMatch.Groups[1].Value;
                        var downloadUrl = $"https://1fichier.com/?{fileId}";
                        System.Diagnostics.Debug.WriteLine($"[1FICHIER] Extracted file ID from HTML: {fileId}");

                        progressCallback?.Report(1.0); // Complete

                        return new UploadResult
                        {
                            DownloadUrl = downloadUrl,
                            FileName = fileName,
                            FileSize = fileSize,
                            UploadId = fileId
                        };
                    }

                    // If we get here, the upload likely succeeded but returned an error page
                    // Check for the French error message
                    if (responseContent.Contains("Pas de fichier"))
                    {
                        throw new Exception("Upload failed: No file was received by server");
                    }

                    throw new Exception($"Upload succeeded but couldn't extract download URL from response");
                }

                string errorContent;
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    errorContent = reader.ReadToEnd();
                }
                throw new Exception($"Unexpected response: {response.StatusCode}, Content: {errorContent}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Upload error: {ex.Message}");
                throw;
            }
        }

        private async Task<DownloadInfo> GetDownloadLinksAsync(string serverUrl, string uploadId)
        {
            try
            {
                var endUrl = $"https://{serverUrl}/end.pl?xid={uploadId}";
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Getting download links from: {endUrl}");

                var request = new HttpRequestMessage(HttpMethod.Get, endUrl);
                request.Headers.Add("JSON", "1");

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (contentType != null && contentType.Contains("application/json"))
                    {
                        var data = JObject.Parse(responseContent);
                        var links = data["links"]?.FirstOrDefault();

                        if (links != null)
                        {
                            var downloadUrl = links["download"]?.ToString();
                            System.Diagnostics.Debug.WriteLine($"[1FICHIER] Download URL: {downloadUrl}");

                            return new DownloadInfo
                            {
                                DownloadUrl = downloadUrl,
                                FileName = links["filename"]?.ToString(),
                                Size = links["size"]?.ToObject<long>() ?? 0
                            };
                        }
                    }
                    else
                    {
                        // Try to parse HTML response for download link
                        System.Diagnostics.Debug.WriteLine($"[1FICHIER] Non-JSON response, trying to parse HTML");
                        // For now, construct a basic download URL
                        var downloadUrl = $"https://1fichier.com/?{uploadId}";
                        return new DownloadInfo { DownloadUrl = downloadUrl };
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Failed to get download links: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Error getting download links: {ex.Message}");
                return null;
            }
        }

        private class DownloadInfo
        {
            public string DownloadUrl { get; set; }
            public string FileName { get; set; }
            public long Size { get; set; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    httpClient?.Dispose();
                }
                disposed = true;
            }
        }
    }
}