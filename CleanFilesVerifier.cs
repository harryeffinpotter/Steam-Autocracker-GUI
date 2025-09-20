using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    public static class CleanFilesVerifier
    {
        public class VerificationResult
        {
            public bool IsClean { get; set; }
            public List<string> ContaminationReasons { get; set; } = new List<string>();
            public List<string> FoundBackups { get; set; } = new List<string>();
            public List<string> FoundCrackArtifacts { get; set; } = new List<string>();
            public bool HasSteamSettings { get; set; }
            public bool HasLobbyShortcuts { get; set; }
            public bool HasBackupFiles { get; set; }
        }

        public static VerificationResult VerifyCleanInstallation(string gamePath)
        {
            var result = new VerificationResult { IsClean = true };

            try
            {
                // Check 1: Look for steam_settings folder
                var steamSettingsPath = Path.Combine(gamePath, "steam_settings");
                if (Directory.Exists(steamSettingsPath))
                {
                    result.IsClean = false;
                    result.HasSteamSettings = true;
                    result.ContaminationReasons.Add($"Found steam_settings folder (crack artifact)");
                    result.FoundCrackArtifacts.Add(steamSettingsPath);
                }

                // Check 2: Look for any .bak files (especially game executables and DLLs)
                var bakFiles = Directory.GetFiles(gamePath, "*.bak", SearchOption.AllDirectories);
                foreach (var bakFile in bakFiles)
                {
                    var fileName = Path.GetFileName(bakFile);
                    var originalName = Path.GetFileNameWithoutExtension(bakFile);

                    // Check if it's a backed up executable or steam API dll
                    if (originalName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                        originalName.Contains("steam_api", StringComparison.OrdinalIgnoreCase) ||
                        originalName.Contains("steamclient", StringComparison.OrdinalIgnoreCase))
                    {
                        result.IsClean = false;
                        result.HasBackupFiles = true;
                        result.FoundBackups.Add(bakFile);
                        result.ContaminationReasons.Add($"Found backup file: {fileName}");
                    }
                }

                // Check 3: Look for Goldberg/Ali213/SSE emulator files
                var crackIndicators = new[]
                {
                    "ali213.ini",
                    "valve.ini",
                    "hlm.ini",
                    "steam_interfaces.txt",
                    "ColdClientLoader.ini",
                    "SmartSteamEmu.ini",
                    "SmartSteamEmu64.dll",
                    "SmartSteamEmu.dll",
                    "ALI213.ini",
                    "steam_api.ini",
                    "steam_appid.txt",  // This one is tricky - could be legit or crack
                    "local_save.txt",
                    "achievements.json",
                    "controller.vdf",
                    "items.json",
                    "stats.txt"
                };

                foreach (var indicator in crackIndicators)
                {
                    var files = Directory.GetFiles(gamePath, indicator, SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        // Special case: steam_appid.txt might be legitimate
                        if (indicator == "steam_appid.txt")
                        {
                            // Check if there's also a steam_settings folder or other crack files
                            if (result.HasSteamSettings || result.HasBackupFiles)
                            {
                                result.IsClean = false;
                                result.FoundCrackArtifacts.Add(file);
                                result.ContaminationReasons.Add($"Found suspicious steam_appid.txt alongside crack artifacts");
                            }
                        }
                        else
                        {
                            result.IsClean = false;
                            result.FoundCrackArtifacts.Add(file);
                            result.ContaminationReasons.Add($"Found crack file: {Path.GetFileName(file)}");
                        }
                    }
                }

                // Check 4: Look for lobby/multiplayer shortcuts created by our tool
                var shortcutPatterns = new[]
                {
                    "*Lobby*.lnk",
                    "*Multiplayer*.lnk",
                    "*LAN*.lnk",
                    "*Online*.lnk",
                    "*_MP.lnk",
                    "*_Lobby.lnk"
                };

                foreach (var pattern in shortcutPatterns)
                {
                    var shortcuts = Directory.GetFiles(gamePath, pattern, SearchOption.TopDirectoryOnly);
                    foreach (var shortcut in shortcuts)
                    {
                        // Check if it's one of our created shortcuts (contains specific parameters)
                        if (IsOurShortcut(shortcut))
                        {
                            result.IsClean = false;
                            result.HasLobbyShortcuts = true;
                            result.FoundCrackArtifacts.Add(shortcut);
                            result.ContaminationReasons.Add($"Found auto-generated lobby shortcut: {Path.GetFileName(shortcut)}");
                        }
                    }
                }

                // Check 5: Look for modified steam_api DLLs by checking file sizes/dates
                var steamApiFiles = new[] { "steam_api.dll", "steam_api64.dll", "steamclient.dll", "steamclient64.dll" };
                foreach (var apiFile in steamApiFiles)
                {
                    var files = Directory.GetFiles(gamePath, apiFile, SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        // Check if there's a corresponding .bak file
                        if (File.Exists(file + ".bak"))
                        {
                            result.IsClean = false;
                            result.ContaminationReasons.Add($"Found backed up Steam API: {Path.GetFileName(file)}.bak");
                        }

                        // Check file size - Goldberg's steam_api.dll is typically different size
                        var fileInfo = new FileInfo(file);
                        if (IsKnownCrackSize(fileInfo.Length, Path.GetFileName(file)))
                        {
                            result.IsClean = false;
                            result.FoundCrackArtifacts.Add(file);
                            result.ContaminationReasons.Add($"Steam API file has suspicious size: {Path.GetFileName(file)}");
                        }
                    }
                }

                // Check 6: Look for debug/log files from cracks
                var debugFiles = new[]
                {
                    "debug_log.txt",
                    "steam_api.log",
                    "goldberg_steam.log",
                    "SteamEmu.log",
                    "SmartSteamEmu.log"
                };

                foreach (var debugFile in debugFiles)
                {
                    var files = Directory.GetFiles(gamePath, debugFile, SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        result.IsClean = false;
                        result.FoundCrackArtifacts.Add(file);
                        result.ContaminationReasons.Add($"Found crack log file: {Path.GetFileName(file)}");
                    }
                }

            }
            catch (Exception ex)
            {
                result.IsClean = false;
                result.ContaminationReasons.Add($"Error during verification: {ex.Message}");
            }

            return result;
        }

        private static bool IsOurShortcut(string shortcutPath)
        {
            try
            {
                // Read shortcut file to check if it contains our specific parameters
                var content = File.ReadAllText(shortcutPath);
                return content.Contains("-lobby") ||
                       content.Contains("-multiplayer") ||
                       content.Contains("connect_lobby");
            }
            catch
            {
                return false;
            }
        }

        private static bool IsKnownCrackSize(long fileSize, string fileName)
        {
            // Known Goldberg Steam API sizes (approximate)
            var knownCrackSizes = new Dictionary<string, long[]>
            {
                { "steam_api.dll", new long[] { 247808, 251904, 268288 } },
                { "steam_api64.dll", new long[] { 288256, 292352, 309760 } }
            };

            if (knownCrackSizes.ContainsKey(fileName.ToLower()))
            {
                var sizes = knownCrackSizes[fileName.ToLower()];
                return sizes.Any(size => Math.Abs(fileSize - size) < 1024); // Within 1KB tolerance
            }

            return false;
        }

        public static bool ShowContaminationDialog(VerificationResult result, string gameName)
        {
            var message = $"⚠️ WARNING: {gameName} is NOT clean!\n\n";
            message += "Found the following contamination:\n\n";

            foreach (var reason in result.ContaminationReasons.Take(5))
            {
                message += $"• {reason}\n";
            }

            if (result.ContaminationReasons.Count > 5)
            {
                message += $"• ... and {result.ContaminationReasons.Count - 5} more issues\n";
            }

            message += "\n📝 TO FIX THIS:\n";
            message += "1. Go to Steam Library\n";
            message += "2. Right-click on " + gameName + "\n";
            message += "3. Properties → Installed Files\n";
            message += "4. Click 'Verify integrity of game files'\n";
            message += "5. Wait for Steam to restore clean files\n";
            message += "6. Try sharing again\n\n";
            message += "Would you like to open Steam now?";

            var dialogResult = MessageBox.Show(
                message,
                "Cannot Share - Files Are Not Clean",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            return dialogResult == DialogResult.Yes;
        }

        public static void OpenSteamVerifyPage(string appId)
        {
            try
            {
                // Open Steam to the game's properties page
                System.Diagnostics.Process.Start($"steam://validate/{appId}");
            }
            catch
            {
                // Fallback to just opening Steam
                System.Diagnostics.Process.Start("steam://open/games");
            }
        }

        public static bool QuickVerifyClean(string gamePath)
        {
            // Quick check for most common contamination
            return !Directory.Exists(Path.Combine(gamePath, "steam_settings")) &&
                   !Directory.GetFiles(gamePath, "*.bak", SearchOption.AllDirectories).Any() &&
                   !File.Exists(Path.Combine(gamePath, "steam_api.ini"));
        }

        public static void CleanupCrackArtifacts(string gamePath, VerificationResult result)
        {
            // This method would clean up crack artifacts if user agrees
            // But for clean uploads, we DON'T want to do this automatically
            // User must verify through Steam to ensure truly clean files

            var message = "Found crack artifacts that need to be removed:\n\n";

            if (result.FoundBackups.Any())
            {
                message += $"• {result.FoundBackups.Count} backup files\n";
            }
            if (result.FoundCrackArtifacts.Any())
            {
                message += $"• {result.FoundCrackArtifacts.Count} crack files\n";
            }
            if (result.HasSteamSettings)
            {
                message += "• steam_settings folder\n";
            }
            if (result.HasLobbyShortcuts)
            {
                message += "• Lobby shortcuts\n";
            }

            message += "\n⚠️ IMPORTANT: Do NOT manually delete these!\n";
            message += "Use Steam's 'Verify integrity' feature instead.\n";
            message += "This ensures you have genuine clean files.";

            MessageBox.Show(
                message,
                "Crack Artifacts Detected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }

    // Extension for the share window
    public static class ShareWindowCleanVerification
    {
        public static async Task<bool> VerifyAndPrepareCleanShare(
            string gamePath,
            string gameName,
            string appId,
            Form parentForm)
        {
            parentForm.Cursor = Cursors.WaitCursor;

            try
            {
                // Run verification
                var result = CleanFilesVerifier.VerifyCleanInstallation(gamePath);

                if (!result.IsClean)
                {
                    parentForm.Cursor = Cursors.Default;

                    // Show contamination dialog
                    if (CleanFilesVerifier.ShowContaminationDialog(result, gameName))
                    {
                        // Open Steam verification page
                        CleanFilesVerifier.OpenSteamVerifyPage(appId);
                    }

                    return false;
                }

                // Files are clean, proceed with share
                return true;
            }
            finally
            {
                parentForm.Cursor = Cursors.Default;
            }
        }

        public static async Task<bool> HandleBothShareRequest(
            string gamePath,
            string gameName,
            string appId,
            Form parentForm,
            Action<string> statusCallback)
        {
            // For "Both" requests, we need to:
            // 1. First verify and upload clean files
            // 2. Then crack the game
            // 3. Upload cracked version

            statusCallback("Verifying clean installation...");

            // Step 1: Verify clean
            if (!await VerifyAndPrepareCleanShare(gamePath, gameName, appId, parentForm))
            {
                return false;
            }

            statusCallback("Uploading clean files...");

            // Step 2: Upload clean version
            // (Upload implementation here)

            var confirmCrack = MessageBox.Show(
                $"Clean files uploaded successfully!\n\n" +
                "Now we'll crack the game and upload the cracked version.\n" +
                "This will modify your game files.\n\n" +
                "Continue with cracking?",
                "Upload Both Versions",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmCrack != DialogResult.Yes)
            {
                return true; // Clean was uploaded at least
            }

            statusCallback("Cracking game...");

            // Step 3: Crack the game
            // (Cracking implementation here)

            statusCallback("Uploading cracked files...");

            // Step 4: Upload cracked version
            // (Upload implementation here)

            return true;
        }
    }
}