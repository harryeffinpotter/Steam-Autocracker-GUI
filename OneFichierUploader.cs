using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SAC_GUI
{
    /// <summary>
    /// Custom HttpContent that streams large files without buffering.
    /// This is critical for files > 2GB which would otherwise cause "Stream was too long" errors.
    /// </summary>
    public class LargeFileMultipartContent : HttpContent
    {
        private readonly string _filePath;
        private readonly string _boundary;
        private readonly byte[] _headerBytes;
        private readonly byte[] _footerBytes;
        private readonly long _fileSize;
        private readonly IProgress<double> _progressCallback;
        private readonly IProgress<string> _statusCallback;

        public LargeFileMultipartContent(string filePath, string fileName, string boundary,
            IProgress<double> progressCallback = null, IProgress<string> statusCallback = null)
        {
            _filePath = filePath;
            _boundary = boundary;
            _fileSize = new FileInfo(filePath).Length;
            _progressCallback = progressCallback;
            _statusCallback = statusCallback;

            // Build header bytes (domain field + file header)
            var sb = new StringBuilder();
            sb.Append($"--{boundary}\r\n");
            sb.Append("Content-Disposition: form-data; name=\"domain\"\r\n\r\n");
            sb.Append("0\r\n");
            sb.Append($"--{boundary}\r\n");
            sb.Append($"Content-Disposition: form-data; name=\"file[]\"; filename=\"{fileName}\"\r\n");
            sb.Append("Content-Type: application/octet-stream\r\n\r\n");
            _headerBytes = Encoding.UTF8.GetBytes(sb.ToString());

            // Build footer bytes
            _footerBytes = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");

            // Set content type header
            Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));
        }

        /// <summary>
        /// Returns true with pre-calculated length to prevent HttpClient from buffering.
        /// This is the KEY to supporting large files - when this returns true, HttpClient
        /// sets Content-Length header and streams directly without buffering.
        /// </summary>
        protected override bool TryComputeLength(out long length)
        {
            length = _headerBytes.Length + _fileSize + _footerBytes.Length;
            return true;
        }

        /// <summary>
        /// Streams content directly to the network without buffering.
        /// </summary>
        protected override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context)
        {
            // Write header
            await stream.WriteAsync(_headerBytes, 0, _headerBytes.Length);

            // Stream file content with progress reporting
            using (var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920))
            {
                var buffer = new byte[81920]; // 80KB buffer
                long totalWritten = 0;
                int bytesRead;

                var startTime = DateTime.Now;
                var lastUpdateTime = DateTime.Now;
                long lastBytesWritten = 0;

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead);
                    totalWritten += bytesRead;

                    // Calculate and report progress/speed every 0.5 seconds
                    var now = DateTime.Now;
                    var timeSinceLastUpdate = (now - lastUpdateTime).TotalSeconds;

                    if (timeSinceLastUpdate >= 0.5)
                    {
                        var bytesThisInterval = totalWritten - lastBytesWritten;
                        var speedMBps = (bytesThisInterval / (1024.0 * 1024.0)) / timeSinceLastUpdate;
                        var totalElapsed = (now - startTime).TotalSeconds;
                        var avgSpeedMBps = totalElapsed > 0 ? (totalWritten / (1024.0 * 1024.0)) / totalElapsed : 0;
                        var bytesRemaining = _fileSize - totalWritten;
                        var etaSeconds = avgSpeedMBps > 0 ? (bytesRemaining / (1024.0 * 1024.0)) / avgSpeedMBps : 0;

                        var statusText = $"Uploading: {speedMBps:F2} MB/s (Avg: {avgSpeedMBps:F2} MB/s) - ETA: {TimeSpan.FromSeconds(etaSeconds):hh\\:mm\\:ss}";
                        System.Diagnostics.Debug.WriteLine($"[1FICHIER] {statusText}");
                        _statusCallback?.Report(statusText);

                        lastUpdateTime = now;
                        lastBytesWritten = totalWritten;
                    }

                    // Report progress (0.0 to 1.0)
                    _progressCallback?.Report((double)totalWritten / _fileSize);
                }

                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Streamed {totalWritten} bytes to network");
            }

            // Write footer
            await stream.WriteAsync(_footerBytes, 0, _footerBytes.Length);
        }
    }

    public class OneFichierUploader : IDisposable
    {
        private const string API_KEY = "PSd6297MACENE2VQD7eNxBWIKrrmTTZb";
        private const string API_BASE_URL = "https://api.1fichier.com/v1";
        private readonly HttpClient httpClient;
        private readonly HttpClient uploadClient; // Separate client for uploads with longer timeout
        private bool disposed = false;

        public OneFichierUploader()
        {
            // Client for API calls
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SACGUI/1.0");

            // Separate client for uploads - no timeout for large files
            var uploadHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };
            uploadClient = new HttpClient(uploadHandler);
            uploadClient.Timeout = Timeout.InfiniteTimeSpan; // Critical for large files
            uploadClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
            uploadClient.DefaultRequestHeaders.Add("User-Agent", "SACGUI/1.0");
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

        public async Task<UploadResult> UploadFileAsync(string filePath, IProgress<double> progressCallback = null, IProgress<string> statusCallback = null)
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

                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Uploading file: {fileName}, Size: {fileSize / (1024 * 1024)}MB ({fileSize / (1024.0 * 1024 * 1024):F2} GB)");

                // Step 1: Get upload server
                var serverInfo = await GetUploadServerAsync();

                // Step 2: Upload the file using HttpClient with custom streaming content
                var uploadUrl = $"https://{serverInfo.Url}/upload.cgi?id={serverInfo.Id}";
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Upload URL: {uploadUrl}");
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Using HttpClient with LargeFileMultipartContent for streaming (supports files > 2GB)");

                // Create boundary for multipart
                string boundary = "----WebKitFormBoundary" + Guid.NewGuid().ToString("N");

                // Use custom HttpContent that streams without buffering
                using (var content = new LargeFileMultipartContent(filePath, fileName, boundary, progressCallback, statusCallback))
                {
                    // Send request using HttpClient (not HttpWebRequest)
                    // HttpClient with our custom content will NOT buffer because TryComputeLength returns true
                    var response = await uploadClient.PostAsync(uploadUrl, content);

                    statusCallback?.Report("Upload complete, waiting for server response...");

                    System.Diagnostics.Debug.WriteLine($"[1FICHIER] Upload response status: {response.StatusCode}");

                    // Log response headers
                    System.Diagnostics.Debug.WriteLine($"[1FICHIER] Response headers:");
                    foreach (var header in response.Headers)
                    {
                        System.Diagnostics.Debug.WriteLine($"[1FICHIER]   {header.Key}: {string.Join(", ", header.Value)}");
                    }

                    // Handle redirect (302/301)
                    if (response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                        response.StatusCode == System.Net.HttpStatusCode.Found ||
                        response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                    {
                        var location = response.Headers.Location?.ToString();
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
                            var downloadInfo = await GetDownloadLinksAsync(serverInfo.Url, uploadId, statusCallback);

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
                        var responseContent = await response.Content.ReadAsStringAsync();

                        System.Diagnostics.Debug.WriteLine($"[1FICHIER] Response body preview: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");

                        // Check if response contains a file URL
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

                        // Check for French error message
                        if (responseContent.Contains("Pas de fichier"))
                        {
                            throw new Exception("Upload failed: No file was received by server");
                        }

                        throw new Exception($"Upload succeeded but couldn't extract download URL from response");
                    }

                    // Handle error response
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Unexpected response: {response.StatusCode}, Content: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Upload error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task<DownloadInfo> GetDownloadLinksAsync(string serverUrl, string uploadId, IProgress<string> statusCallback = null)
        {
            // Retry for up to 5 minutes to wait for antivirus scan
            int maxRetries = 10; // 10 retries x 30 seconds = 5 minutes
            int retryDelay = 30000; // 30 seconds

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var endUrl = $"https://{serverUrl}/end.pl?xid={uploadId}";
                    System.Diagnostics.Debug.WriteLine($"[1FICHIER] Getting download links from: {endUrl} (attempt {attempt}/{maxRetries})");

                    var request = new HttpRequestMessage(HttpMethod.Get, endUrl);
                    request.Headers.Add("JSON", "1");

                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var contentType = response.Content.Headers.ContentType?.MediaType;
                        var responseContent = await response.Content.ReadAsStringAsync();

                        // Check if still waiting for antivirus scan
                        if (responseContent.Contains("Veuillez patienter") || responseContent.Contains("Please wait"))
                        {
                            statusCallback?.Report($"1fichier is still processing upload... retrying ({attempt}/{maxRetries})");
                            System.Diagnostics.Debug.WriteLine($"[1FICHIER] File still being scanned by antivirus, waiting {retryDelay/1000}s before retry...");
                            if (attempt < maxRetries)
                            {
                                await Task.Delay(retryDelay);
                                continue;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[1FICHIER] Antivirus scan timeout after {maxRetries} attempts");
                                statusCallback?.Report("1fichier processing timed out");
                                return null;
                            }
                        }

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

                    // Retry on failure
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(retryDelay);
                        continue;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[1FICHIER] Error getting download links (attempt {attempt}/{maxRetries}): {ex.Message}");

                    // Retry on exception
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(retryDelay);
                        continue;
                    }

                    return null;
                }
            }

            return null;
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
                    uploadClient?.Dispose();
                }
                disposed = true;
            }
        }
    }
}