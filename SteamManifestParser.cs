using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SteamAppIdIdentifier
{
    /// <summary>
    /// Parses Steam ACF manifest files to automatically detect AppIDs
    /// </summary>
    public static class SteamManifestParser
    {
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