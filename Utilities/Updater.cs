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
        public static bool CheckForNet()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            int i = 0;
            if (!offline)
            {
                while (!hasinternet && i < hosts.Length)
                {
                    hasinternet = CheckForInternet(hosts[i]);
                    i++;
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

        public static bool CheckForInternet(string host)
        {
            bool success = false;
            Ping myPing = new Ping();
            byte[] buffer = new byte[32];
            int timeout = 60000;
            PingOptions pingOptions = new PingOptions();
            try
            {
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {
                    hasinternet = true;
                    offline = false;
                    success = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogNetwork($"Internet check failed for {host}: {ex.Message}");
                hasinternet = false;
                offline = true;
                return false;
            }
            if (success == true)
            {
                hasinternet = true;
                offline = false;
                return true;
            }
            offline = true;
            return false;

        }
    }
}