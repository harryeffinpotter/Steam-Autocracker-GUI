using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SteamAppIdIdentifier
{
    /// <summary>
    /// Parses Steam ACF manifest files to automatically detect AppIDs
    /// </summary>
    public static class SteamManifestParser
    {
        // Cache for depot names fetched from Steam API
        private static readonly Dictionary<string, Dictionary<string, string>> DepotNameCache = new Dictionary<string, Dictionary<string, string>>();
        private static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        /// <summary>
        /// Attempts to find AppID from Steam manifest files when a folder is dropped
        /// </summary>
        /// <param name="droppedPath">The path that was dropped onto the application</param>
        /// <returns>Tuple of (AppID, GameName, SizeOnDisk) or null if not found</returns>
        public static (string appId, string gameName, long sizeOnDisk)? GetAppIdFromManifest(string droppedPath)
        {
            // Check if this path contains "steamapps" - indicating it's from a Steam library
            if (droppedPath.IndexOf("steamapps", StringComparison.OrdinalIgnoreCase) < 0)
            {
                System.Diagnostics.Debug.WriteLine("[MANIFEST] Path doesn't contain 'steamapps', skipping manifest check");
                return null;
            }

            // Get the game folder name (last part of the path)
            string gameFolderName = Path.GetFileName(droppedPath.TrimEnd('\\', '/'));
            System.Diagnostics.Debug.WriteLine($"[MANIFEST] Checking for game folder: {gameFolderName}");

            // Find the steamapps directory
            string steamappsPath = GetSteamappsPath(droppedPath);
            if (string.IsNullOrEmpty(steamappsPath))
            {
                System.Diagnostics.Debug.WriteLine("[MANIFEST] Could not find steamapps directory");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"[MANIFEST] Steamapps directory: {steamappsPath}");

            // Look for all appmanifest_*.acf files
            var acfFiles = Directory.GetFiles(steamappsPath, "appmanifest_*.acf");
            System.Diagnostics.Debug.WriteLine($"[MANIFEST] Found {acfFiles.Length} ACF files");

            foreach (var acfFile in acfFiles)
            {
                try
                {
                    string content = File.ReadAllText(acfFile);
                    var manifest = ParseAcfFile(content);

                    // Check if this manifest's installdir matches our game folder
                    if (manifest.ContainsKey("installdir") &&
                        string.Equals(manifest["installdir"], gameFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        string appId = manifest.ContainsKey("appid") ? manifest["appid"] : null;
                        string gameName = manifest.ContainsKey("name") ? manifest["name"] : gameFolderName;
                        long sizeOnDisk = 0;

                        if (manifest.ContainsKey("SizeOnDisk"))
                        {
                            long.TryParse(manifest["SizeOnDisk"], out sizeOnDisk);
                        }

                        System.Diagnostics.Debug.WriteLine($"[MANIFEST] ✅ Found match!");
                        System.Diagnostics.Debug.WriteLine($"[MANIFEST] AppID: {appId}");
                        System.Diagnostics.Debug.WriteLine($"[MANIFEST] Name: {gameName}");
                        System.Diagnostics.Debug.WriteLine($"[MANIFEST] Size: {sizeOnDisk / (1024 * 1024)} MB");

                        return (appId, gameName, sizeOnDisk);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MANIFEST] Error parsing {acfFile}: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine("[MANIFEST] No matching manifest found");
            return null;
        }

        /// <summary>
        /// Gets AppID directly from a known manifest file path
        /// </summary>
        public static string GetAppIdFromManifestFile(string acfFilePath)
        {
            try
            {
                string content = File.ReadAllText(acfFilePath);
                var manifest = ParseAcfFile(content);
                return manifest.ContainsKey("appid") ? manifest["appid"] : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Verifies if the actual folder size matches the manifest size (within tolerance)
        /// </summary>
        public static bool VerifyGameSize(string gamePath, long manifestSize, long toleranceBytes = 10485760) // 10MB tolerance
        {
            long actualSize = GetDirectorySize(gamePath);
            long difference = Math.Abs(actualSize - manifestSize);

            System.Diagnostics.Debug.WriteLine($"[MANIFEST] Size verification:");
            System.Diagnostics.Debug.WriteLine($"[MANIFEST] Manifest size: {manifestSize / (1024 * 1024)} MB");
            System.Diagnostics.Debug.WriteLine($"[MANIFEST] Actual size: {actualSize / (1024 * 1024)} MB");
            System.Diagnostics.Debug.WriteLine($"[MANIFEST] Difference: {difference / (1024 * 1024)} MB");

            return difference <= toleranceBytes;
        }

        /// <summary>
        /// Finds the steamapps directory from a given path
        /// </summary>
        private static string GetSteamappsPath(string path)
        {
            // Navigate up the directory tree to find steamapps
            DirectoryInfo dir = new DirectoryInfo(path);

            while (dir != null)
            {
                if (dir.Name.Equals("steamapps", StringComparison.OrdinalIgnoreCase))
                {
                    return dir.FullName;
                }

                // Check if current directory contains a steamapps folder
                var steamappsSubdir = dir.GetDirectories("steamapps", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (steamappsSubdir != null)
                {
                    return steamappsSubdir.FullName;
                }

                dir = dir.Parent;
            }

            return null;
        }

        /// <summary>
        /// Parses an ACF file into a dictionary of key-value pairs
        /// </summary>
        private static Dictionary<string, string> ParseAcfFile(string content)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Regular expressions for parsing VDF/ACF format
            var keyValuePattern = @"""(\w+)""\s+""([^""]*)""";
            var matches = Regex.Matches(content, keyValuePattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 3)
                {
                    string key = match.Groups[1].Value;
                    string value = match.Groups[2].Value;

                    // Only store the first occurrence of each key (ignores nested structures)
                    if (!result.ContainsKey(key))
                    {
                        result[key] = value;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches depot names for an app from Steam API and caches them
        /// </summary>
        public static async Task<Dictionary<string, string>> FetchDepotNamesAsync(string appId)
        {
            if (string.IsNullOrEmpty(appId)) return new Dictionary<string, string>();

            // Check cache first
            lock (DepotNameCache)
            {
                if (DepotNameCache.TryGetValue(appId, out var cached))
                    return cached;
            }

            var depotNames = new Dictionary<string, string>();

            try
            {
                // Try Steam Store API first
                string url = $"https://store.steampowered.com/api/appdetails?appids={appId}";
                var response = await httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                if (json[appId]?["success"]?.Value<bool>() == true)
                {
                    var data = json[appId]["data"];
                    string gameName = data["name"]?.Value<string>() ?? "";

                    // Check for depots in the response
                    var depots = data["depots"];
                    if (depots != null)
                    {
                        foreach (var depot in depots.Children<JProperty>())
                        {
                            string depotId = depot.Name;
                            if (long.TryParse(depotId, out _)) // Only numeric depot IDs
                            {
                                var depotInfo = depot.Value;
                                string name = depotInfo["name"]?.Value<string>();
                                if (!string.IsNullOrEmpty(name))
                                    depotNames[depotId] = name;
                            }
                        }
                    }

                    // If no depot names found, try SteamDB
                    if (depotNames.Count == 0)
                    {
                        depotNames = await FetchDepotNamesFromSteamDBAsync(appId, gameName);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEPOT] Error fetching depot names for {appId}: {ex.Message}");
            }

            // Cache the result
            lock (DepotNameCache)
            {
                DepotNameCache[appId] = depotNames;
            }

            return depotNames;
        }

        /// <summary>
        /// Fetches depot names from SteamDB as fallback
        /// </summary>
        private static async Task<Dictionary<string, string>> FetchDepotNamesFromSteamDBAsync(string appId, string gameName)
        {
            var depotNames = new Dictionary<string, string>();

            try
            {
                string url = $"https://steamdb.info/app/{appId}/depots/";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                var response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return depotNames;

                string html = await response.Content.ReadAsStringAsync();

                // Parse depot entries: <td><a href="/depot/XXXXX/">XXXXX</a></td><td>Depot Name</td>
                var depotPattern = @"<a href=""/depot/(\d+)/"">\d+</a></td>\s*<td[^>]*>([^<]+)</td>";
                var matches = Regex.Matches(html, depotPattern);

                foreach (Match match in matches)
                {
                    string depotId = match.Groups[1].Value;
                    string name = System.Net.WebUtility.HtmlDecode(match.Groups[2].Value.Trim());
                    if (!string.IsNullOrEmpty(name))
                        depotNames[depotId] = name;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEPOT] SteamDB fallback failed for {appId}: {ex.Message}");
            }

            return depotNames;
        }

        /// <summary>
        /// Gets depot names synchronously from cache
        /// Checks game-specific cache first, then shared redistributables (228980)
        /// </summary>
        public static string GetDepotName(string appId, string depotId)
        {
            lock (DepotNameCache)
            {
                // First check game-specific depot names
                if (!string.IsNullOrEmpty(appId) && DepotNameCache.TryGetValue(appId, out var gameDepots))
                {
                    if (gameDepots.TryGetValue(depotId, out string name))
                        return name;
                }

                // Then check shared redistributables (228980)
                if (DepotNameCache.TryGetValue("228980", out var sharedDepots))
                {
                    if (sharedDepots.TryGetValue(depotId, out string name))
                        return name;
                }
            }
            return null;
        }

        /// <summary>
        /// Parses InstallScripts section from ACF content and derives depot names from paths
        /// Returns dictionary of depotId -> derived name
        /// </summary>
        public static Dictionary<string, string> ParseInstallScripts(string content)
        {
            var depotNames = new Dictionary<string, string>();

            // Find InstallScripts section
            int scriptsStart = content.IndexOf("\"InstallScripts\"", StringComparison.OrdinalIgnoreCase);
            if (scriptsStart < 0) return depotNames;

            // Find the section content
            int braceStart = content.IndexOf('{', scriptsStart);
            if (braceStart < 0) return depotNames;

            int braceCount = 1;
            int pos = braceStart + 1;
            int braceEnd = -1;

            while (pos < content.Length && braceCount > 0)
            {
                if (content[pos] == '{') braceCount++;
                else if (content[pos] == '}') braceCount--;
                if (braceCount == 0) braceEnd = pos;
                pos++;
            }

            if (braceEnd < 0) return depotNames;

            string scriptsSection = content.Substring(braceStart + 1, braceEnd - braceStart - 1);

            // Parse each entry: "depotId" "path\to\installscript.vdf"
            var scriptPattern = @"""(\d+)""\s+""([^""]+)""";
            var matches = Regex.Matches(scriptsSection, scriptPattern);

            foreach (Match match in matches)
            {
                string depotId = match.Groups[1].Value;
                string scriptPath = match.Groups[2].Value;

                string derivedName = DeriveDepotNameFromPath(scriptPath);
                if (!string.IsNullOrEmpty(derivedName))
                    depotNames[depotId] = derivedName;
            }

            return depotNames;
        }

        /// <summary>
        /// Derives a human-readable depot name from an install script path
        /// e.g., "_CommonRedist\vcredist\2019\installscript.vdf" -> "VC 2019 Redist"
        /// </summary>
        private static string DeriveDepotNameFromPath(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath)) return null;

            scriptPath = scriptPath.Replace("\\\\", "\\").ToLowerInvariant();

            // Visual C++ Redistributables
            if (scriptPath.Contains("vcredist"))
            {
                var yearMatch = Regex.Match(scriptPath, @"vcredist[\\\/](\d{4})");
                if (yearMatch.Success)
                    return $"VC {yearMatch.Groups[1].Value} Redist";
            }

            // .NET Framework
            if (scriptPath.Contains("dotnet"))
            {
                var versionMatch = Regex.Match(scriptPath, @"dotnet[\\\/]([0-9.]+)");
                if (versionMatch.Success)
                    return $".NET {versionMatch.Groups[1].Value} Redist";
            }

            // XNA
            if (scriptPath.Contains("xna"))
            {
                var versionMatch = Regex.Match(scriptPath, @"xna[\\\/]([0-9.]+)");
                if (versionMatch.Success)
                    return $"XNA {versionMatch.Groups[1].Value} Redist";
            }

            // DirectX
            if (scriptPath.Contains("directx"))
            {
                if (scriptPath.Contains("jun2010"))
                    return "DirectX Jun2010 Redist";
                return "DirectX Redist";
            }

            // OpenAL
            if (scriptPath.Contains("openal"))
                return "OpenAL Redist";

            // PhysX
            if (scriptPath.Contains("physx"))
                return "PhysX Redist";

            return null;
        }

        /// <summary>
        /// Loads depot names from Steamworks Common Redistributables (228980) manifest
        /// These are shared across all games (VC++, .NET, XNA, DirectX, etc.)
        /// </summary>
        private static void LoadSharedDepotNames(string steamappsPath)
        {
            const string SHARED_DEPOTS_APPID = "228980";

            // Check if already loaded
            lock (DepotNameCache)
            {
                if (DepotNameCache.ContainsKey(SHARED_DEPOTS_APPID))
                    return;
            }

            try
            {
                string sharedManifestPath = Path.Combine(steamappsPath, $"appmanifest_{SHARED_DEPOTS_APPID}.acf");
                if (File.Exists(sharedManifestPath))
                {
                    string content = File.ReadAllText(sharedManifestPath);
                    var depotNames = ParseInstallScripts(content);

                    if (depotNames.Count > 0)
                    {
                        lock (DepotNameCache)
                        {
                            DepotNameCache[SHARED_DEPOTS_APPID] = depotNames;
                        }
                        System.Diagnostics.Debug.WriteLine($"[DEPOT] Loaded {depotNames.Count} shared depot names from 228980");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEPOT] Error loading shared depot names: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses InstalledDepots section from ACF content
        /// Returns dictionary of depotId -> (manifest, size)
        /// </summary>
        public static Dictionary<string, (string manifest, long size)> ParseInstalledDepots(string content)
        {
            var depots = new Dictionary<string, (string, long)>();

            // Find InstalledDepots section
            int depotsStart = content.IndexOf("\"InstalledDepots\"", StringComparison.OrdinalIgnoreCase);
            if (depotsStart < 0) return depots;

            // Find the opening brace after InstalledDepots
            int braceStart = content.IndexOf('{', depotsStart);
            if (braceStart < 0) return depots;

            // Find matching closing brace
            int braceCount = 1;
            int pos = braceStart + 1;
            int braceEnd = -1;

            while (pos < content.Length && braceCount > 0)
            {
                if (content[pos] == '{') braceCount++;
                else if (content[pos] == '}') braceCount--;
                if (braceCount == 0) braceEnd = pos;
                pos++;
            }

            if (braceEnd < 0) return depots;

            string depotsSection = content.Substring(braceStart + 1, braceEnd - braceStart - 1);

            // Parse each depot entry: "depotId" { "manifest" "value" "size" "value" }
            var depotPattern = @"""(\d+)""\s*\{([^}]*)\}";
            var depotMatches = Regex.Matches(depotsSection, depotPattern, RegexOptions.Singleline);

            foreach (Match match in depotMatches)
            {
                string depotId = match.Groups[1].Value;
                string depotContent = match.Groups[2].Value;

                string manifest = "";
                long size = 0;

                var manifestMatch = Regex.Match(depotContent, @"""manifest""\s+""(\d+)""");
                if (manifestMatch.Success)
                    manifest = manifestMatch.Groups[1].Value;

                var sizeMatch = Regex.Match(depotContent, @"""size""\s+""(\d+)""");
                if (sizeMatch.Success)
                    long.TryParse(sizeMatch.Groups[1].Value, out size);

                if (!string.IsNullOrEmpty(manifest))
                    depots[depotId] = (manifest, size);
            }

            return depots;
        }

        /// <summary>
        /// Gets full manifest info for a game including depots
        /// Also parses shared redistributables manifest for depot names
        /// </summary>
        public static (string appId, string gameName, string buildId, long lastUpdated, Dictionary<string, (string manifest, long size)> depots)? GetFullManifestInfo(string gamePath)
        {
            if (gamePath.IndexOf("steamapps", StringComparison.OrdinalIgnoreCase) < 0)
                return null;

            string gameFolderName = System.IO.Path.GetFileName(gamePath.TrimEnd('\\', '/'));
            string steamappsPath = GetSteamappsPath(gamePath);
            if (string.IsNullOrEmpty(steamappsPath)) return null;

            // First, try to load depot names from Steamworks Common Redistributables (228980)
            LoadSharedDepotNames(steamappsPath);

            var acfFiles = Directory.GetFiles(steamappsPath, "appmanifest_*.acf");

            foreach (var acfFile in acfFiles)
            {
                try
                {
                    string content = File.ReadAllText(acfFile);
                    var manifest = ParseAcfFile(content);

                    if (manifest.ContainsKey("installdir") &&
                        string.Equals(manifest["installdir"], gameFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        string appId = manifest.ContainsKey("appid") ? manifest["appid"] : null;
                        string gameName = manifest.ContainsKey("name") ? manifest["name"] : gameFolderName;
                        string buildId = manifest.ContainsKey("buildid") ? manifest["buildid"] : "";
                        long lastUpdated = 0;
                        if (manifest.ContainsKey("LastUpdated"))
                            long.TryParse(manifest["LastUpdated"], out lastUpdated);

                        var depots = ParseInstalledDepots(content);

                        // Parse game's own InstallScripts and cache depot names
                        var gameDepotNames = ParseInstallScripts(content);
                        if (gameDepotNames.Count > 0 && !string.IsNullOrEmpty(appId))
                        {
                            lock (DepotNameCache)
                            {
                                if (!DepotNameCache.ContainsKey(appId))
                                    DepotNameCache[appId] = new Dictionary<string, string>();
                                foreach (var kvp in gameDepotNames)
                                    DepotNameCache[appId][kvp.Key] = kvp.Value;
                            }
                        }

                        return (appId, gameName, buildId, lastUpdated, depots);
                    }
                }
                catch { }
            }

            return null;
        }

        /// <summary>
        /// Gets the total size of a directory in bytes
        /// </summary>
        private static long GetDirectorySize(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                return dir.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Finds all installed Steam games by scanning manifest files
        /// </summary>
        public static List<(string appId, string gameName, string installPath)> FindAllInstalledGames()
        {
            var games = new List<(string, string, string)>();
            var steamPaths = GetAllSteamLibraryPaths();

            foreach (var steamPath in steamPaths)
            {
                var steamappsPath = Path.Combine(steamPath, "steamapps");
                if (!Directory.Exists(steamappsPath)) continue;

                var acfFiles = Directory.GetFiles(steamappsPath, "appmanifest_*.acf");

                foreach (var acfFile in acfFiles)
                {
                    try
                    {
                        string content = File.ReadAllText(acfFile);
                        var manifest = ParseAcfFile(content);

                        if (manifest.ContainsKey("appid") && manifest.ContainsKey("name") && manifest.ContainsKey("installdir"))
                        {
                            string installPath = Path.Combine(steamappsPath, "common", manifest["installdir"]);
                            if (Directory.Exists(installPath))
                            {
                                games.Add((manifest["appid"], manifest["name"], installPath));
                            }
                        }
                    }
                    catch { }
                }
            }

            return games;
        }

        /// <summary>
        /// Gets all Steam library paths from the system
        /// </summary>
        private static List<string> GetAllSteamLibraryPaths()
        {
            var paths = new List<string>();

            // Default Steam installation paths
            var defaultPaths = new[]
            {
                @"C:\Program Files (x86)\Steam",
                @"C:\Program Files\Steam",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam")
            };

            // Add default paths that exist
            foreach (var path in defaultPaths)
            {
                if (Directory.Exists(path) && !paths.Contains(path))
                {
                    paths.Add(path);
                }
            }

            // Try to find additional library folders from Steam's libraryfolders.vdf
            foreach (var steamPath in paths.ToList())
            {
                var vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                if (File.Exists(vdfPath))
                {
                    try
                    {
                        string vdfContent = File.ReadAllText(vdfPath);
                        // Look for path entries in the VDF file
                        var pathMatches = Regex.Matches(vdfContent, @"""path""\s+""([^""]*)""");

                        foreach (Match match in pathMatches)
                        {
                            if (match.Groups.Count > 1)
                            {
                                string libPath = match.Groups[1].Value.Replace(@"\\", @"\");
                                if (Directory.Exists(libPath) && !paths.Contains(libPath))
                                {
                                    paths.Add(libPath);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            return paths;
        }
    }
}