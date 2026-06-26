using Newtonsoft.Json;
using Octokit;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace APPID
{

    internal class Updater
    {
        public static bool hasinternet = false;
        private static string[] hosts = { "1.1.1.1", "8.8.8.8", "208.67.222.222" };
        private static readonly string BinPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory, "_bin");

        // GitHub repo for SACGUI releases
        private const string SACGUI_GITHUB_USER = "harryeffinpotter";
        private const string SACGUI_GITHUB_REPO = "SteamAPPIDFinder";

        /// <summary>
        /// Check if a newer version of SACGUI is available on GitHub
        /// </summary>
        public static async System.Threading.Tasks.Task CheckForSACGUIUpdateAsync()
        {
            try
            {
                if (!hasinternet && !await CheckForNetAsync())
                    return;

                // Check if user skipped this version or disabled updates
                string skippedVersion = AppSettings.Default.SkippedUpdateVersion ?? "";

                var client = new GitHubClient(new ProductHeaderValue("SACGUI"));
                var releases = await client.Repository.Release.GetAll(SACGUI_GITHUB_USER, SACGUI_GITHUB_REPO);

                if (releases == null || releases.Count == 0)
                    return;

                // Get latest release (not prerelease)
                Release latestRelease = null;
                foreach (var release in releases)
                {
                    if (!release.Prerelease)
                    {
                        latestRelease = release;
                        break;
                    }
                }

                if (latestRelease == null)
                    return;

                // Parse version from tag (e.g., "v2.4.1" or "2.4.1")
                string latestVersionStr = latestRelease.TagName.TrimStart('v', 'V');
                string currentVersionStr = System.Windows.Forms.Application.ProductVersion;

                // Check if user skipped this specific version
                if (skippedVersion == latestVersionStr)
                    return;

                // Compare versions
                if (Version.TryParse(latestVersionStr, out Version latestVersion) &&
                    Version.TryParse(currentVersionStr, out Version currentVersion))
                {
                    if (latestVersion > currentVersion)
                    {
                        // Find the exe asset in the release
                        string downloadUrl = null;
                        foreach (var asset in latestRelease.Assets)
                        {
                            if (asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
                                (asset.Name.Contains("SACGUI", StringComparison.OrdinalIgnoreCase) ||
                                 asset.Name.Contains("Steam", StringComparison.OrdinalIgnoreCase)))
                            {
                                downloadUrl = asset.BrowserDownloadUrl;
                                break;
                            }
                        }

                        // Show custom update dialog
                        var result = ShowUpdateDialog(currentVersionStr, latestVersionStr,
                            latestRelease.Body ?? "", downloadUrl != null);

                        switch (result)
                        {
                            case UpdateDialogResult.Install:
                                if (downloadUrl != null)
                                    await DownloadAndInstallUpdateAsync(downloadUrl, latestVersionStr);
                                else
                                    Process.Start(new ProcessStartInfo { FileName = latestRelease.HtmlUrl, UseShellExecute = true });
                                break;
                            case UpdateDialogResult.Skip:
                                // Skip this version - don't ask again until next version
                                AppSettings.Default.SkippedUpdateVersion = latestVersionStr;
                                AppSettings.Default.Save();
                                break;
                            case UpdateDialogResult.Later:
                                // Do nothing - will ask again next launch
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Silently ignore - not critical, probably rate limited
                LogHelper.LogNetwork($"SACGUI update check skipped: {ex.Message}");
            }
        }

        private enum UpdateDialogResult { Install, Skip, Later }

        private static UpdateDialogResult ShowUpdateDialog(string currentVersion, string newVersion, string releaseNotes, bool canAutoInstall)
        {
            var result = UpdateDialogResult.Later;

            using (var form = new Form())
            {
                form.Text = "Update Available";
                form.Size = new System.Drawing.Size(480, 380);
                form.StartPosition = FormStartPosition.CenterScreen;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.BackColor = System.Drawing.Color.FromArgb(30, 30, 35);

                var titleLabel = new System.Windows.Forms.Label
                {
                    Text = $"SACGUI v{newVersion} is available!",
                    Location = new System.Drawing.Point(20, 15),
                    Size = new System.Drawing.Size(420, 25),
                    Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(100, 200, 255)
                };

                var versionLabel = new System.Windows.Forms.Label
                {
                    Text = $"You have v{currentVersion}",
                    Location = new System.Drawing.Point(20, 42),
                    Size = new System.Drawing.Size(420, 20),
                    Font = new System.Drawing.Font("Segoe UI", 9),
                    ForeColor = System.Drawing.Color.FromArgb(150, 150, 155)
                };

                var notesLabel = new System.Windows.Forms.Label
                {
                    Text = "What's new:",
                    Location = new System.Drawing.Point(20, 70),
                    Size = new System.Drawing.Size(100, 20),
                    Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(200, 200, 205)
                };

                var notesBox = new TextBox
                {
                    Location = new System.Drawing.Point(20, 92),
                    Size = new System.Drawing.Size(420, 180),
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BackColor = System.Drawing.Color.FromArgb(20, 22, 28),
                    ForeColor = System.Drawing.Color.FromArgb(200, 200, 205),
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new System.Drawing.Font("Segoe UI", 9),
                    Text = string.IsNullOrEmpty(releaseNotes) ? "No release notes available." : releaseNotes
                };

                var installBtn = new Button
                {
                    Text = canAutoInstall ? "Install Update" : "Download",
                    Location = new System.Drawing.Point(20, 290),
                    Size = new System.Drawing.Size(130, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.FromArgb(40, 120, 200),
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
                };
                installBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 140, 220);
                installBtn.Click += (s, e) => { result = UpdateDialogResult.Install; form.Close(); };

                var laterBtn = new Button
                {
                    Text = "Later",
                    Location = new System.Drawing.Point(165, 290),
                    Size = new System.Drawing.Size(130, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.FromArgb(50, 50, 55),
                    ForeColor = System.Drawing.Color.FromArgb(200, 200, 205),
                    Font = new System.Drawing.Font("Segoe UI", 9)
                };
                laterBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(70, 70, 75);
                laterBtn.Click += (s, e) => { result = UpdateDialogResult.Later; form.Close(); };

                var skipBtn = new Button
                {
                    Text = "Skip Version",
                    Location = new System.Drawing.Point(310, 290),
                    Size = new System.Drawing.Size(130, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.FromArgb(50, 50, 55),
                    ForeColor = System.Drawing.Color.FromArgb(150, 150, 155),
                    Font = new System.Drawing.Font("Segoe UI", 9)
                };
                skipBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(70, 70, 75);
                skipBtn.Click += (s, e) => { result = UpdateDialogResult.Skip; form.Close(); };

                form.Controls.AddRange(new Control[] { titleLabel, versionLabel, notesLabel, notesBox, installBtn, laterBtn, skipBtn });
                form.ShowDialog();
            }

            return result;
        }

        /// <summary>
        /// Download and install a SACGUI update
        /// </summary>
        private static async System.Threading.Tasks.Task DownloadAndInstallUpdateAsync(string downloadUrl, string version)
        {
            try
            {
                string currentExePath = Environment.ProcessPath;
                string exeDir = Path.GetDirectoryName(currentExePath);
                string tempExePath = Path.Combine(exeDir, $"SACGUI_v{version}_new.exe");
                string updaterBatPath = Path.Combine(exeDir, "update_sacgui.bat");

                // Download new version
                using (var progressForm = new Form())
                {
                    progressForm.Text = "Downloading Update...";
                    progressForm.Size = new System.Drawing.Size(400, 120);
                    progressForm.StartPosition = FormStartPosition.CenterScreen;
                    progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    progressForm.MaximizeBox = false;
                    progressForm.MinimizeBox = false;

                    var progressBar = new ProgressBar
                    {
                        Location = new System.Drawing.Point(20, 20),
                        Size = new System.Drawing.Size(340, 25),
                        Style = ProgressBarStyle.Continuous
                    };

                    var label = new System.Windows.Forms.Label
                    {
                        Location = new System.Drawing.Point(20, 55),
                        Size = new System.Drawing.Size(340, 20),
                        Text = "Downloading..."
                    };

                    progressForm.Controls.Add(progressBar);
                    progressForm.Controls.Add(label);
                    progressForm.Show();

                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();

                        var totalBytes = response.Content.Headers.ContentLength ?? -1;
                        var downloadedBytes = 0L;

                        using (var fs = new FileStream(tempExePath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None))
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var buffer = new byte[8192];
                            int bytesRead;

                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fs.WriteAsync(buffer, 0, bytesRead);
                                downloadedBytes += bytesRead;

                                if (totalBytes > 0)
                                {
                                    int progress = (int)((downloadedBytes * 100) / totalBytes);
                                    progressBar.Value = Math.Min(progress, 100);
                                    label.Text = $"Downloading... {downloadedBytes / 1024 / 1024}MB / {totalBytes / 1024 / 1024}MB";
                                }

                                System.Windows.Forms.Application.DoEvents();
                            }
                        }
                    }

                    progressForm.Close();
                }

                // Create batch file to replace exe after we exit
                string batContent = $@"@echo off
timeout /t 2 /nobreak >nul
del ""{currentExePath}""
move ""{tempExePath}"" ""{currentExePath}""
start """" ""{currentExePath}""
del ""%~f0""
";
                File.WriteAllText(updaterBatPath, batContent);

                // Launch the updater and exit
                Process.Start(new ProcessStartInfo
                {
                    FileName = updaterBatPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                });

                // Exit the application
                System.Windows.Forms.Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download update:\n{ex.Message}", "Update Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public static dynamic getJson(string requestURL)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            try
            {

                string BaseURL = "https://api.github.com/repos/";
                var options = new RestClientOptions(BaseURL)
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                var client = new RestClient(options);
                Console.WriteLine($"Requesting: {BaseURL}{requestURL}");
                var request = new RestRequest(requestURL, Method.Get);
                var queryResult = client.Execute(request);
                var obj = JsonConvert.DeserializeObject<dynamic>(queryResult.Content);
                return obj;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"getJson failed for {requestURL}", ex);
                return null;
            }
        }
        public static bool offline = false;
        public static async System.Threading.Tasks.Task<bool> CheckForNetAsync()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            if (!offline)
            {
                foreach (var host in hosts)
                {
                    if (await CheckForInternetAsync(host))
                    {
                        hasinternet = true;
                        return true;
                    }
                }
                if (!hasinternet)
                {
                    offline = true;
                }
            }
            return hasinternet;
        }

        public static async System.Threading.Tasks.Task CheckGitHubNewerVersion(string User, string Repo, string APIBase)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                var obj = getJson($"{User}/{Repo}/releases");
                GitHubClient client = new GitHubClient(new ProductHeaderValue($"{Repo}"));
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(User, Repo);
                string balls = releases.ToString();
                string latestGitHubVersion = releases[0].TagName.ToString().Replace("v", "");
                string localVersion = File.ReadAllText(Path.Combine(BinPath, $"{Repo}.ver")).Trim();
                if (localVersion != latestGitHubVersion)
                {
                LogHelper.LogUpdate($"Steamless", $"{localVersion} -> {latestGitHubVersion}");

                if (Directory.Exists(Path.Combine(BinPath, "Steamless")))
                {
                    Directory.Delete(Path.Combine(BinPath, "Steamless"), true);
                }
                foreach (var o in obj[0])
                {
                    if (o.ToString().Contains("browser_download_url"))
                    {
                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        string ballss = StringTools.RemoveEverythingBeforeFirstRemoveString(o.Value.ToString(), "browser_download_url\": \"");
                        string ballsss = StringTools.RemoveEverythingAfterFirstRemoveString(ballss, "\"");
                        using (HttpClient client2 = new HttpClient())
                        {
                            var response = await client2.GetAsync(ballsss);
                            using (var fs = new FileStream("SLS.zip", System.IO.FileMode.CreateNew))
                            {
                                await response.Content.CopyToAsync(fs);
                            }
                        }
                        await ExtractFileAsync("SLS.zip", Path.Combine(BinPath, "Steamless"));
                        File.Delete("SLS.zip");
                        File.Delete(Path.Combine(BinPath, $"{Repo}.ver"));
                        File.WriteAllText(Path.Combine(BinPath, $"{Repo}.ver"), latestGitHubVersion);
                    }
                }
            }
            else
            {
                return;
            }
            }
            catch (Exception ex)
            {
                // Silently ignore rate limit and other API errors - not critical
                LogHelper.LogNetwork($"GitHub API check skipped: {ex.Message}");
            }
        }

        public static async System.Threading.Tasks.Task UpdateGoldBergAsync()
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                // Get latest release from the new fork
                var obj = getJson("Detanup01/gbe_fork/releases/latest");
                if (obj == null)
                {
                    MessageBox.Show("Unable to check for GoldBerg updates. Please try again later.");
                    return;
                }

                string latestVersion = obj.tag_name.ToString().Replace("release-", "");
                string versionFile = Path.Combine(BinPath, "Goldberg", "version.txt");

                string localVersion = "";
                if (File.Exists(versionFile))
                {
                    localVersion = File.ReadAllText(versionFile).Trim();
                }

                if (localVersion != latestVersion)
                {
                    LogHelper.LogUpdate($"GoldBerg (gbe_fork)", $"{localVersion} -> {latestVersion}");

                    // Find the Windows release asset
                    string downloadUrl = "";
                    foreach (var asset in obj.assets)
                    {
                        if (asset.name.ToString() == "emu-win-release.7z")
                        {
                            downloadUrl = asset.browser_download_url.ToString();
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(downloadUrl))
                    {
                        // Download the new version
                        using (HttpClient client = new HttpClient())
                        {
                            string tempFile = "gbe_fork.7z";
                            var response = await client.GetAsync(downloadUrl);
                            using (var fs = new FileStream(tempFile, System.IO.FileMode.CreateNew))
                            {
                                await response.Content.CopyToAsync(fs);
                            }
                        }

                        // Extract to temporary directory
                        string tempDir = Path.Combine(BinPath, "temp_goldberg");
                        if (Directory.Exists(tempDir))
                        {
                            Directory.Delete(tempDir, true);
                        }

                        await ExtractFileAsync("gbe_fork.7z", tempDir);

                        // Copy the DLL files to Goldberg directory
                        if (!Directory.Exists(Path.Combine(BinPath, "Goldberg")))
                        {
                            Directory.CreateDirectory(Path.Combine(BinPath, "Goldberg"));
                        }

                        // Copy the specific DLL files from known paths
                        // Note: The archive extracts with 'release' as the root folder
                        string steam64Path = Path.Combine(tempDir, "release", "regular", "x64", "steam_api64.dll");
                        string steam32Path = Path.Combine(tempDir, "release", "regular", "x32", "steam_api.dll");

                        if (File.Exists(steam64Path))
                        {
                            File.Copy(steam64Path, Path.Combine(BinPath, "Goldberg", "steam_api64.dll"), true);
                        }

                        if (File.Exists(steam32Path))
                        {
                            File.Copy(steam32Path, Path.Combine(BinPath, "Goldberg", "steam_api.dll"), true);
                        }

                        // Copy generate_interfaces tools if they exist
                        // Try multiple possible paths as the structure might vary
                        string[] possibleToolPaths = new string[] {
                            Path.Combine(tempDir, "release", "tools", "generate_interfaces", "generate_interfaces_x64.exe"),
                            Path.Combine(tempDir, "release", "tools", "generate_interfaces", "generate_interfaces_x32.exe"),
                            Path.Combine(tempDir, "tools", "generate_interfaces", "generate_interfaces_x64.exe"),
                            Path.Combine(tempDir, "tools", "generate_interfaces", "generate_interfaces_x32.exe"),
                            Path.Combine(tempDir, "generate_interfaces_x64.exe"),
                            Path.Combine(tempDir, "generate_interfaces_x32.exe")
                        };

                        foreach (string toolPath in possibleToolPaths)
                        {
                            if (File.Exists(toolPath))
                            {
                                string fileName = Path.GetFileName(toolPath);
                                string destPath = Path.Combine(BinPath, "Goldberg", fileName);
                                File.Copy(toolPath, destPath, true);
                                Console.WriteLine($"Copied tool: {fileName}");
                            }
                        }

                        // Copy lobby_connect tools if they exist (for LAN multiplayer)
                        string[] lobbyConnectPaths = new string[] {
                            Path.Combine(tempDir, "release", "tools", "lobby_connect", "lobby_connect_x64.exe"),
                            Path.Combine(tempDir, "release", "tools", "lobby_connect", "lobby_connect_x32.exe"),
                            Path.Combine(tempDir, "tools", "lobby_connect", "lobby_connect_x64.exe"),
                            Path.Combine(tempDir, "tools", "lobby_connect", "lobby_connect_x32.exe")
                        };

                        foreach (string lobbyPath in lobbyConnectPaths)
                        {
                            if (File.Exists(lobbyPath))
                            {
                                string fileName = Path.GetFileName(lobbyPath);
                                // Copy to _bin folder for LAN multiplayer feature
                                string destPath = Path.Combine(BinPath, fileName);
                                File.Copy(lobbyPath, destPath, true);
                                Console.WriteLine($"Copied lobby_connect tool to _bin: {fileName}");
                            }
                        }

                        // Clean up
                        File.Delete("gbe_fork.7z");
                        Directory.Delete(tempDir, true);

                        // Update version file
                        File.WriteAllText(versionFile, latestVersion);
                    }
                    else
                    {
                        MessageBox.Show("Windows release asset not found for GoldBerg fork.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Silently ignore - not critical, probably rate limited
                LogHelper.LogError("UpdateGoldBerg failed", ex);
            }
        }
        public static async System.Threading.Tasks.Task ExtractFileAsync(string sourceArchive, string destination)
        {
            try
            {
                string sevenZipPath = Path.Combine(BinPath, "7z", "7za.exe");
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
                pro.FileName = sevenZipPath;
                pro.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", sourceArchive, destination);
                Process x = Process.Start(pro);
                if (!x.HasExited)
                    await System.Threading.Tasks.Task.Run(() => x.WaitForExit());
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"ExtractFileAsync failed for {sourceArchive}", ex);
                MessageBox.Show("Unable to extract updated files. If you have WINRAR try uninstalling it then trying again! If you have not installed FFAIO since version 2.0.13 then ");
            }
        }

        public static async System.Threading.Tasks.Task<bool> CheckForInternetAsync(string host)
        {
            using (Ping myPing = new Ping())
            {
                byte[] buffer = new byte[32];
                int timeout = 20000; // 20 seconds - async so no UI blocking
                PingOptions pingOptions = new PingOptions();
                try
                {
                    PingReply reply = await myPing.SendPingAsync(host, timeout, buffer, pingOptions);
                    if (reply.Status == IPStatus.Success)
                    {
                        hasinternet = true;
                        offline = false;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogNetwork($"Internet check failed for {host}: {ex.Message}");
                }
                return false;
            }
        }
    }
}