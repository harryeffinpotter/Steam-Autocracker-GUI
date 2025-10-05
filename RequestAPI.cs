using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Drawing;

namespace SteamAppIdIdentifier
{
    public static partial class RequestAPI
    {
        internal const string API_BASE = "https://pydrive.harryeffingpotter.com/sacgui/api";
        internal static readonly HttpClient client = new HttpClient();
        private static string _hwid;

        // Get unique HWID for this machine
        public static string GetHWID()
        {
            if (_hwid == null)
            {
                var cpuId = GetWMIProperty("Win32_Processor", "ProcessorId");
                var motherboardId = GetWMIProperty("Win32_BaseBoard", "SerialNumber");
                var diskId = GetWMIProperty("Win32_DiskDrive", "SerialNumber");

                var combined = $"{cpuId}_{motherboardId}_{diskId}";

                using (var sha256 = SHA256.Create())
                {
                    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    _hwid = Convert.ToBase64String(hash).Substring(0, 16);
                }
            }
            return _hwid;
        }

        private static string GetWMIProperty(string wmiClass, string property)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj[property]?.ToString() ?? "Unknown";
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        // Submit a new request with full parameters
        public static async Task<bool> SubmitRequest(string appId, string gameName, string userId, string hwid)
        {
            try
            {
                var request = new
                {
                    appId = appId,
                    gameName = gameName,
                    userId = userId,
                    hwid = hwid
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/request", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Overload for backward compatibility
        public static async Task<bool> SubmitRequest(string userId, string appId, string gameName)
        {
            return await SubmitRequest(appId, gameName, "anonymous", "anonymous");
        }

        // Overload for backward compatibility
        public static async Task<bool> SubmitRequest(string appId, string gameName)
        {
            return await SubmitRequest(appId, gameName, "anonymous", "anonymous");
        }

        // Submit a game request with type (Clean, Cracked, Both)
        public static async Task<bool> SubmitGameRequest(string appId, string gameName, string requestType)
        {
            try
            {
                var request = new
                {
                    appId = appId,
                    gameName = gameName,
                    requestType = requestType,
                    requesterId = GetHWID(),
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

        // Check if user has any requested games on startup
        public static async Task<UserStatus> CheckUserStatus(List<string> userAppIds)
        {
            try
            {
                var data = new
                {
                    hwid = GetHWID(),
                    appIds = userAppIds
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/check", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserStatus>(responseJson);
                }
            }
            catch { }

            return new UserStatus();
        }

        // Mark a request as honored (user uploaded files)
        public static async Task<bool> HonorRequest(string appId, string uploadType)
        {
            try
            {
                var data = new
                {
                    appId = appId,
                    uploadType = uploadType,  // Clean or Cracked
                    honorerId = GetHWID(),
                    timestamp = DateTime.UtcNow
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/honor", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Get top requested games from community
        public static async Task<List<TopRequestedGame>> GetTopRequestedGames(int limit = 20)
        {
            try
            {
                var response = await client.GetAsync($"{API_BASE}/top-requests?limit={limit}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<TopRequestedGame>>(json);
                }
            }
            catch { }

            return new List<TopRequestedGame>();
        }

        // Get active requests
        public static async Task<List<RequestedGame>> GetActiveRequests()
        {
            try
            {
                var response = await client.GetAsync($"{API_BASE}/requests/active");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<RequestedGame>>(json);
                }
            }
            catch { }
            return new List<RequestedGame>();
        }

        // Remove a request
        public static async Task RemoveRequest(string appId, string userId, string hwid)
        {
            try
            {
                var request = new
                {
                    appId = appId,
                    userId = userId,
                    hwid = hwid
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/remove-request", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to remove request: {ex.Message}");
                throw;
            }
        }

        // Overload for backward compatibility
        public static async Task RemoveRequest(string userId, string appId)
        {
            await RemoveRequest(appId, "anonymous", "anonymous");
        }

        // Get global stats
        public static async Task<GlobalStats> GetGlobalStats()
        {
            try
            {
                var response = await client.GetAsync($"{API_BASE}/stats");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<GlobalStats>(json);
                }
            }
            catch { }

            return new GlobalStats();
        }

        // Get detailed user statistics
        public static async Task<DetailedUserStats> GetDetailedUserStats(string userId)
        {
            try
            {
                var response = await client.GetAsync($"{API_BASE}/users/{userId}/stats");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<DetailedUserStats>(json);
                }
            }
            catch { }
            return new DetailedUserStats();
        }

        public class UserStatus
        {
            public List<RequestedGame> NeededGames { get; set; } = new List<RequestedGame>();
            public int TotalRequests { get; set; }
            public int UserHonored { get; set; }
            public int UserRequested { get; set; }
        }

        public class RequestedGame
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public string RequestType { get; set; }  // Clean, Cracked, Both
            public int RequestCount { get; set; }
            public int CleanRequests { get; set; }
            public int CrackedRequests { get; set; }
            public DateTime FirstRequested { get; set; }
            public DateTime? RequestDate { get; set; }
            public string Status { get; set; }
            public int VoteCount { get; set; }
            public int UncrackableVotes { get; set; }
        }

        public class TopRequestedGame
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public int RequestCount { get; set; }
            public DateTime? LastRequested { get; set; }
        }

        public class GlobalStats
        {
            public int TotalRequests { get; set; }
            public int TotalHonored { get; set; }
            public int TotalFilled { get; set; }
            public int ActiveUsers { get; set; }
        }

        public class DetailedUserStats
        {
            public int Fulfilled { get; set; }     // Games they provided that were requested
            public int Requested { get; set; }     // Games they asked for
            public int Filled { get; set; }        // Games they asked for and received
            public int HonorScore { get; set; }
            public DateTime LastActivity { get; set; }

            public double GetGiveToTakeRatio()
            {
                if (Requested == 0) return Fulfilled > 0 ? double.MaxValue : 0;
                return (double)Fulfilled / Requested;
            }

            public string GetUserType()
            {
                var ratio = GetGiveToTakeRatio();
                if (ratio >= 3.0) return "🏆 Legend";
                if (ratio >= 2.0) return "🌟 Hero";
                if (ratio >= 1.5) return "💚 Contributor";
                if (ratio >= 1.0) return "⚖️ Balanced";
                if (ratio >= 0.5) return "📥 Receiver";
                return "🆕 New User";
            }
        }

        // Get user activity history
        public static async Task<UserActivity> GetUserActivity(string userId)
        {
            try
            {
                var response = await client.GetAsync($"{API_BASE}/activity/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserActivity>(json);
                }
            }
            catch { }

            return new UserActivity
            {
                RecentRequests = new List<RequestedGame>(),
                RecentHonors = new List<HonorRecord>(),
                ActiveRequests = new List<RequestedGame>(),
                FulfilledRequests = new List<RequestedGame>(),
                Contributions = new List<HonorRecord>()
            };
        }

        public class UserActivity
        {
            public List<RequestedGame> RecentRequests { get; set; }
            public List<HonorRecord> RecentHonors { get; set; }
            public List<RequestedGame> ActiveRequests { get; set; }
            public List<RequestedGame> FulfilledRequests { get; set; }
            public List<HonorRecord> Contributions { get; set; }
        }

        public class HonorRecord
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public DateTime HonoredDate { get; set; }
            public string UploadType { get; set; }
        }
    }

    // Add to main form
    public class RequestStatusDisplay : Label
    {
        private System.Windows.Forms.Timer updateTimer;

        public RequestStatusDisplay()
        {
            this.Font = new System.Drawing.Font("Consolas", 9);
            this.ForeColor = System.Drawing.Color.FromArgb(100, 200, 100);
            this.BackColor = System.Drawing.Color.Transparent;
            this.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.AutoSize = false;
            this.Size = new System.Drawing.Size(300, 20);

            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 60000; // Update every minute
            updateTimer.Tick += async (s, e) => await UpdateStats();
            updateTimer.Start();

            // Initial update
            Task.Run(async () => await UpdateStats());
        }

        private async Task UpdateStats()
        {
            try
            {
                var stats = await RequestAPI.GetGlobalStats();

                this.Invoke(new Action(() =>
                {
                    this.Text = $"Requests: {stats.TotalRequests} | Honored: {stats.TotalHonored} | Filled: {stats.TotalFilled}";

                    // Subtle color changes based on activity
                    if (stats.TotalRequests > stats.TotalFilled + 10)
                    {
                        // Many pending requests - yellowish
                        this.ForeColor = System.Drawing.Color.FromArgb(200, 200, 100);
                    }
                    else
                    {
                        // Most requests filled - greenish
                        this.ForeColor = System.Drawing.Color.FromArgb(100, 200, 100);
                    }
                }));
            }
            catch
            {
                this.Text = "Requests: -- | Honored: -- | Filled: --";
            }
        }
    }

    // Check on app startup
    public static class StartupRequestChecker
    {
        private static readonly HttpClient client = RequestAPI.client;
        private const string API_BASE = RequestAPI.API_BASE;

        public static async Task CheckAndNotify(Form mainForm, List<string> userAppIds)
        {
            try
            {
                var status = await RequestAPI.CheckUserStatus(userAppIds);

                if (status.NeededGames.Any())
                {
                    // Show subtle notification
                    var message = new StringBuilder();
                    message.AppendLine("🔥 YOUR GAMES ARE NEEDED:");
                    message.AppendLine();

                    foreach (var game in status.NeededGames.Take(5))
                    {
                        message.AppendLine($"• {game.GameName} ({game.RequestCount} requests)");
                    }

                    if (status.NeededGames.Count > 5)
                    {
                        message.AppendLine($"... and {status.NeededGames.Count - 5} more");
                    }

                    message.AppendLine();
                    message.AppendLine($"Your contribution score: {status.UserHonored}");

                    // Show as a non-blocking notification
                    var notificationForm = new Form
                    {
                        Text = "Games Needed",
                        Size = new System.Drawing.Size(400, 300),
                        StartPosition = FormStartPosition.Manual,
                        Location = new System.Drawing.Point(
                            Screen.PrimaryScreen.WorkingArea.Width - 420,
                            Screen.PrimaryScreen.WorkingArea.Height - 320
                        ),
                        FormBorderStyle = FormBorderStyle.FixedToolWindow,
                        BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
                        TopMost = true
                    };

                    var label = new Label
                    {
                        Text = message.ToString(),
                        Dock = DockStyle.Fill,
                        ForeColor = System.Drawing.Color.White,
                        Font = new System.Drawing.Font("Segoe UI", 10),
                        Padding = new System.Windows.Forms.Padding(20)
                    };

                    notificationForm.Controls.Add(label);
                    notificationForm.Show();

                    // Auto-close after 10 seconds
                    var closeTimer = new System.Windows.Forms.Timer();
                    closeTimer.Interval = 10000;
                    closeTimer.Tick += (s, e) =>
                    {
                        notificationForm.Close();
                        closeTimer.Stop();
                    };
                    closeTimer.Start();
                }
            }
            catch { }
        }

        public static async Task VoteOnRequest(string userId, string appId, bool upvote)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(new
                {
                    userId = userId,
                    appId = appId,
                    voteType = upvote ? "upvote" : "downvote",
                    timestamp = DateTime.UtcNow
                }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/vote-request", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to vote on request: {ex.Message}");
                throw;
            }
        }

        public static async Task VoteUncrackable(string userId, string appId)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(new
                {
                    userId = userId,
                    appId = appId,
                    timestamp = DateTime.UtcNow
                }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/vote-uncrackable", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to vote uncrackable: {ex.Message}");
                throw;
            }
        }

        public static async Task<GameRequestStats> GetGameRequestStats(string appId)
        {
            try
            {
                var response = await client.GetAsync($"{API_BASE}/game-stats/{appId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<GameRequestStats>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get game stats: {ex.Message}");
                return new GameRequestStats
                {
                    AppId = appId,
                    ActiveRequests = 0,
                    UncrackableVotes = 0
                };
            }

            return new GameRequestStats
            {
                AppId = appId,
                ActiveRequests = 0,
                UncrackableVotes = 0
            };
        }

    }
}

public class GameRequestStats
{
    public string AppId { get; set; }
    public int ActiveRequests { get; set; }
    public int UncrackableVotes { get; set; }
}
