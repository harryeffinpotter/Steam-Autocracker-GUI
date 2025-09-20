using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SteamAppIdIdentifier
{
    public static class HWIDManager
    {
        private static readonly string HWID_FILE = "sacgui.uid";
        private static readonly string API_BASE = "https://pydrive.harryeffingpotter.com/sacgui/api";
        private static readonly HttpClient client = new HttpClient();

        private static string _currentHWID;
        private static string _userId;
        private static HWIDData _storedData;

        public class HWIDData
        {
            public string HWID { get; set; }
            public string UserId { get; set; }
            public DateTime LastVerified { get; set; }
            public string HashedHWID { get; set; }
            public int HonorScore { get; set; }
            public int TotalShares { get; set; }
            public int TotalRequests { get; set; }
        }

        public static async Task<string> InitializeHWID()
        {
            try
            {
                // Get current HWID
                _currentHWID = GenerateHWID();

                // Get stored HWID data
                _storedData = LoadStoredHWID();

                if (_storedData == null)
                {
                    // First time user
                    return await RegisterNewUser();
                }
                else if (_storedData.HWID != _currentHWID)
                {
                    // HWID changed - hardware upgrade or spoof
                    return await HandleHWIDChange();
                }
                else
                {
                    // HWID matches - verify with backend
                    return await VerifyExistingUser();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HWID] Error initializing: {ex.Message}");
                return _currentHWID; // Fallback to just HWID
            }
        }

        private static string GenerateHWID()
        {
            try
            {
                var sb = new StringBuilder();

                // Get CPU ID
                using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        sb.Append(obj["ProcessorId"]?.ToString() ?? "NOCPU");
                        break;
                    }
                }

                sb.Append("_");

                // Get Motherboard Serial
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        sb.Append(obj["SerialNumber"]?.ToString() ?? "NOMB");
                        break;
                    }
                }

                sb.Append("_");

                // Get primary disk serial
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var serial = obj["SerialNumber"]?.ToString();
                        if (!string.IsNullOrEmpty(serial))
                        {
                            sb.Append(serial);
                            break;
                        }
                    }
                }

                // Hash the combined hardware IDs
                using (var sha256 = SHA256.Create())
                {
                    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                    return Convert.ToBase64String(bytes).Substring(0, 24); // Shorter, cleaner ID
                }
            }
            catch
            {
                // Fallback to machine name + random
                var fallback = Environment.MachineName + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                using (var sha256 = SHA256.Create())
                {
                    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fallback));
                    return Convert.ToBase64String(bytes).Substring(0, 24);
                }
            }
        }

        private static HWIDData LoadStoredHWID()
        {
            try
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, HWID_FILE);
                if (!File.Exists(filePath))
                    return null;

                var encryptedData = File.ReadAllText(filePath);
                var decryptedJson = SimpleDecrypt(encryptedData);
                return JsonConvert.DeserializeObject<HWIDData>(decryptedJson);
            }
            catch
            {
                return null;
            }
        }

        private static void SaveHWIDData(HWIDData data)
        {
            try
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, HWID_FILE);
                var json = JsonConvert.SerializeObject(data);
                var encrypted = SimpleEncrypt(json);
                File.WriteAllText(filePath, encrypted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HWID] Failed to save: {ex.Message}");
            }
        }

        private static async Task<string> RegisterNewUser()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[HWID] Registering new user...");

                var registration = new
                {
                    hwid = _currentHWID,
                    timestamp = DateTime.UtcNow,
                    version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    machine = Environment.MachineName
                };

                var json = JsonConvert.SerializeObject(registration);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/register", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseJson);

                    _userId = result.userId;

                    // Save locally
                    _storedData = new HWIDData
                    {
                        HWID = _currentHWID,
                        UserId = _userId,
                        LastVerified = DateTime.UtcNow,
                        HashedHWID = HashHWID(_currentHWID),
                        HonorScore = 0,
                        TotalShares = 0,
                        TotalRequests = 0
                    };

                    SaveHWIDData(_storedData);

                    System.Diagnostics.Debug.WriteLine($"[HWID] Registered with UserId: {_userId}");
                    return _userId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HWID] Registration failed: {ex.Message}");
            }

            return _currentHWID;
        }

        private static async Task<string> HandleHWIDChange()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[HWID] Hardware change detected!");
                System.Diagnostics.Debug.WriteLine($"[HWID] Old: {_storedData.HWID}");
                System.Diagnostics.Debug.WriteLine($"[HWID] New: {_currentHWID}");

                var update = new
                {
                    oldHwid = _storedData.HWID,
                    newHwid = _currentHWID,
                    userId = _storedData.UserId,
                    timestamp = DateTime.UtcNow,
                    reason = "hardware_change"
                };

                var json = JsonConvert.SerializeObject(update);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/updatehwid", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseJson);

                    // Update stored data
                    _storedData.HWID = _currentHWID;
                    _storedData.HashedHWID = HashHWID(_currentHWID);
                    _storedData.LastVerified = DateTime.UtcNow;

                    // Backend might return updated stats
                    if (result.honorScore != null)
                        _storedData.HonorScore = result.honorScore;
                    if (result.totalShares != null)
                        _storedData.TotalShares = result.totalShares;
                    if (result.totalRequests != null)
                        _storedData.TotalRequests = result.totalRequests;

                    SaveHWIDData(_storedData);

                    System.Diagnostics.Debug.WriteLine("[HWID] Successfully updated HWID on server");
                    return _storedData.UserId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HWID] Update failed: {ex.Message}");
            }

            // If update fails, still use the stored UserId
            return _storedData.UserId ?? _currentHWID;
        }

        private static async Task<string> VerifyExistingUser()
        {
            try
            {
                // Only verify if last check was > 24 hours ago
                if ((DateTime.UtcNow - _storedData.LastVerified).TotalHours < 24)
                {
                    System.Diagnostics.Debug.WriteLine("[HWID] Using cached UserId (verified recently)");
                    return _storedData.UserId;
                }

                System.Diagnostics.Debug.WriteLine("[HWID] Verifying with backend...");

                var verify = new
                {
                    hwid = _currentHWID,
                    userId = _storedData.UserId
                };

                var json = JsonConvert.SerializeObject(verify);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/verify", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseJson);

                    // Update stats from server
                    _storedData.LastVerified = DateTime.UtcNow;
                    if (result.honorScore != null)
                        _storedData.HonorScore = result.honorScore;
                    if (result.totalShares != null)
                        _storedData.TotalShares = result.totalShares;
                    if (result.totalRequests != null)
                        _storedData.TotalRequests = result.totalRequests;

                    SaveHWIDData(_storedData);

                    System.Diagnostics.Debug.WriteLine($"[HWID] Verified - Honor: {_storedData.HonorScore}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HWID] Verification failed: {ex.Message}");
            }

            return _storedData.UserId;
        }

        private static string HashHWID(string hwid)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hwid + "SACGUI"));
                return Convert.ToBase64String(bytes);
            }
        }

        // Simple encryption for local storage (not for security, just obfuscation)
        private static string SimpleEncrypt(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ 0x42);
            }
            return Convert.ToBase64String(bytes);
        }

        private static string SimpleDecrypt(string encrypted)
        {
            var bytes = Convert.FromBase64String(encrypted);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ 0x42);
            }
            return Encoding.UTF8.GetString(bytes);
        }

        // Public methods for the app to use
        public static string GetUserId()
        {
            return _userId ?? _currentHWID;
        }

        public static string GetHWID()
        {
            return _currentHWID;
        }

        public static int GetHonorScore()
        {
            return _storedData?.HonorScore ?? 0;
        }

        public static async Task<bool> IncrementHonor(int points = 1)
        {
            if (_storedData != null)
            {
                _storedData.HonorScore += points;
                SaveHWIDData(_storedData);

                // Also update backend
                try
                {
                    var update = new
                    {
                        userId = _storedData.UserId,
                        honorIncrement = points
                    };

                    var json = JsonConvert.SerializeObject(update);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await client.PostAsync($"{API_BASE}/incrementhonor", content);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public static async Task<bool> RecordShare(string appId, string shareType)
        {
            try
            {
                if (_storedData != null)
                {
                    _storedData.TotalShares++;
                    SaveHWIDData(_storedData);
                }

                var record = new
                {
                    userId = GetUserId(),
                    appId = appId,
                    shareType = shareType,
                    timestamp = DateTime.UtcNow
                };

                var json = JsonConvert.SerializeObject(record);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/recordshare", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> RecordRequest(string appId, string requestType)
        {
            try
            {
                if (_storedData != null)
                {
                    _storedData.TotalRequests++;
                    SaveHWIDData(_storedData);
                }

                var record = new
                {
                    userId = GetUserId(),
                    appId = appId,
                    requestType = requestType,
                    timestamp = DateTime.UtcNow
                };

                var json = JsonConvert.SerializeObject(record);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/recordrequest", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    // Updated RequestAPI to use HWIDManager
    public static class RequestAPIEnhanced
    {
        private static readonly string API_BASE = "https://pydrive.harryeffingpotter.com/sacgui/api";
        private static readonly HttpClient client = new HttpClient();

        public static async Task<bool> SubmitGameRequest(string appId, string gameName, string requestType)
        {
            try
            {
                await HWIDManager.RecordRequest(appId, requestType);

                var request = new
                {
                    appId = appId,
                    gameName = gameName,
                    requestType = requestType,
                    userId = HWIDManager.GetUserId(),
                    hwid = HWIDManager.GetHWID(),
                    timestamp = DateTime.UtcNow
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/requests", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> CompleteUpload(string appId, string uploadType, string uploadUrl)
        {
            try
            {
                // Record the share
                await HWIDManager.RecordShare(appId, uploadType);

                var completion = new
                {
                    appId = appId,
                    uploadType = uploadType,
                    uploadUrl = uploadUrl,
                    userId = HWIDManager.GetUserId(),
                    hwid = HWIDManager.GetHWID(),
                    timestamp = DateTime.UtcNow
                };

                var json = JsonConvert.SerializeObject(completion);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Use the special endpoint that checks and removes from requests
                var response = await client.PostAsync(
                    $"https://pydrive.harryeffingpotter.com/sacgui/uploadcomplete?type={uploadType}&appid={appId}",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    // Increment honor if this fulfilled a request
                    var responseJson = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseJson);

                    if (result.fulfilledRequest == true)
                    {
                        await HWIDManager.IncrementHonor(result.honorPoints ?? 1);
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}