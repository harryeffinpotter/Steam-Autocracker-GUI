using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace APPID
{
    /// <summary>
    /// Builds the Steam-style clean release folder structure
    /// (GameName.Build.X.Win64.public/{depotcache,steamapps/common/InstallDir})
    /// used for clean (uncracked) releases. Shared by the single-game share flow
    /// and the batch processor.
    /// </summary>
    public static class CleanReleaseBuilder
    {
        /// <summary>
        /// Creates a temp folder containing the proper depotcache + steamapps
        /// structure for a clean release and returns the path to the named build
        /// folder (the folder that should be the root inside the archive).
        /// Returns null if the source ACF can't be located.
        /// </summary>
        public static string PrepareStructure(string appId, string gameName, string installPath, string buildId)
        {
            try
            {
                if (string.IsNullOrEmpty(buildId)) buildId = "Unknown";

                // Find the ACF file for this game across all Steam libraries
                string acfFilePath = null;
                string acfContent = null;

                foreach (var steamPath in GetSteamLibraryPaths())
                {
                    var potentialAcfPath = Path.Combine(steamPath, $"appmanifest_{appId}.acf");
                    if (File.Exists(potentialAcfPath))
                    {
                        acfFilePath = potentialAcfPath;
                        acfContent = File.ReadAllText(potentialAcfPath);
                        break;
                    }
                }

                if (string.IsNullOrEmpty(acfFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[CLEAN BUILD] No ACF found for AppID {appId}");
                    return null;
                }

                var depots = ParseInstalledDepots(acfContent);
                var manifestFiles = FindDepotManifests(depots);

                var acfData = ParseAcfFile(acfContent);
                string installDir = acfData.ContainsKey("installdir") ? acfData["installdir"] : Path.GetFileName(installPath);

                string tempBasePath = Path.Combine(Path.GetTempPath(), "SACGUI_Clean_" + Guid.NewGuid().ToString("N").Substring(0, 8));
                string cleanFolderName = $"{gameName.Replace(" ", ".")}.Build.{buildId}.Win64.public";
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    cleanFolderName = cleanFolderName.Replace(c.ToString(), "");
                }

                string cleanFolderPath = Path.Combine(tempBasePath, cleanFolderName);
                Directory.CreateDirectory(cleanFolderPath);

                // depotcache + manifests
                string depotcachePath = Path.Combine(cleanFolderPath, "depotcache");
                Directory.CreateDirectory(depotcachePath);
                foreach (var manifestFile in manifestFiles)
                {
                    File.Copy(manifestFile, Path.Combine(depotcachePath, Path.GetFileName(manifestFile)), true);
                }

                // steamapps + acf
                string steamappsPath = Path.Combine(cleanFolderPath, "steamapps");
                Directory.CreateDirectory(steamappsPath);
                File.Copy(acfFilePath, Path.Combine(steamappsPath, Path.GetFileName(acfFilePath)), true);

                // steamapps/common/InstallDir + game files
                string commonPath = Path.Combine(steamappsPath, "common");
                Directory.CreateDirectory(commonPath);
                CopyDirectory(installPath, Path.Combine(commonPath, installDir));

                System.Diagnostics.Debug.WriteLine($"[CLEAN BUILD] Structure prepared: {cleanFolderPath} ({manifestFiles.Count} manifests)");
                return cleanFolderPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CLEAN BUILD] Error: {ex.Message}");
                return null;
            }
        }

        private static Dictionary<string, string> ParseAcfFile(string content)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in Regex.Matches(content, @"""(\w+)""\s+""([^""]*)"""))
            {
                if (match.Groups.Count == 3 && !result.ContainsKey(match.Groups[1].Value))
                    result[match.Groups[1].Value] = match.Groups[2].Value;
            }
            return result;
        }

        private static List<(string depotId, string manifestId)> ParseInstalledDepots(string acfContent)
        {
            var depots = new List<(string, string)>();

            var depotMatch = Regex.Match(acfContent, @"""InstalledDepots"".*?\{(.*?)\n\t\}", RegexOptions.Singleline);
            if (depotMatch.Success)
            {
                var depotSection = depotMatch.Groups[1].Value;
                var depotIds = Regex.Matches(depotSection, @"""(\d+)""\s*\{");
                var manifestIds = Regex.Matches(depotSection, @"""manifest""\s+""(\d+)""");

                for (int i = 0; i < Math.Min(depotIds.Count, manifestIds.Count); i++)
                    depots.Add((depotIds[i].Groups[1].Value, manifestIds[i].Groups[1].Value));
            }

            return depots;
        }

        private static List<string> FindDepotManifests(List<(string depotId, string manifestId)> depots)
        {
            var manifestFiles = new List<string>();

            foreach (var steamPath in GetSteamLibraryPaths())
            {
                var depotcachePath = Path.Combine(Path.GetDirectoryName(steamPath), "depotcache");
                if (!Directory.Exists(depotcachePath)) continue;

                foreach (var (depotId, manifestId) in depots)
                {
                    var manifestFile = Path.Combine(depotcachePath, $"{depotId}_{manifestId}.manifest");
                    if (File.Exists(manifestFile) && !manifestFiles.Contains(manifestFile))
                        manifestFiles.Add(manifestFile);
                }
            }

            return manifestFiles;
        }

        private static List<string> GetSteamLibraryPaths()
        {
            var paths = new HashSet<string>();

            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => d.RootDirectory.FullName);

            foreach (var drive in drives)
            {
                var potentialPaths = new[]
                {
                    Path.Combine(drive, "Program Files (x86)", "Steam", "steamapps"),
                    Path.Combine(drive, "Program Files", "Steam", "steamapps"),
                    Path.Combine(drive, "Steam", "steamapps"),
                    Path.Combine(drive, "SteamLibrary", "steamapps"),
                    Path.Combine(drive, "Games", "Steam", "steamapps"),
                    Path.Combine(drive, "Games", "SteamLibrary", "steamapps")
                };

                foreach (var path in potentialPaths)
                {
                    if (!Directory.Exists(path)) continue;
                    paths.Add(path);

                    var vdfPath = Path.Combine(path, "libraryfolders.vdf");
                    if (File.Exists(vdfPath))
                    {
                        var vdfContent = File.ReadAllText(vdfPath);
                        foreach (Match match in Regex.Matches(vdfContent, @"""path""\s+""([^""]+)"""))
                        {
                            var libPath = Path.Combine(match.Groups[1].Value.Replace(@"\\", @"\"), "steamapps");
                            if (Directory.Exists(libPath))
                                paths.Add(libPath);
                        }
                    }
                }
            }

            return paths.ToList();
        }

        // Long-path (\\?\) aware recursive copy to survive >260 char game paths.
        private static void CopyDirectory(string sourceDir, string destDir)
        {
            string longSourceDir = ToLongPath(sourceDir);
            string longDestDir = ToLongPath(destDir);

            if (!Directory.Exists(longDestDir))
                Directory.CreateDirectory(longDestDir);

            foreach (string file in Directory.GetFiles(longSourceDir))
            {
                string destFile = Path.Combine(longDestDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(longSourceDir))
            {
                CopyDirectory(subDir, Path.Combine(longDestDir, Path.GetFileName(subDir)));
            }
        }

        private static string ToLongPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith(@"\\?\"))
                return path;
            if (path.StartsWith(@"\\"))
                return @"\\?\UNC\" + path.Substring(2);
            if (Path.IsPathRooted(path))
                return @"\\?\" + path;
            return @"\\?\" + Path.GetFullPath(path);
        }
    }
}
