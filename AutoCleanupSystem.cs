using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    public static class AutoCleanupSystem
    {
        // Files and folders we create during cracking
        private static readonly string[] OurCreatedFiles = new[]
        {
            "steam_settings",  // Directory
            "*.bak",          // Backup files
            "*Lobby*.lnk",    // Lobby shortcuts
            "*Multiplayer*.lnk",
            "*LAN*.lnk",
            "*Online*.lnk",
            "launch_*.bat",   // Batch files we might create
            "connect_lobby_*.bat",
            "steam_appid.txt", // Only if we created it (check alongside steam_settings)
            "ALI213.ini",
            "valve.ini",
            "steam_interfaces.txt",
            "local_save.txt",
            "achievements.json",
            "items.json",
            "stats.txt"
        };

        public class CleanupResult
        {
            public bool Success { get; set; }
            public List<string> RestoredFiles { get; set; } = new List<string>();
            public List<string> DeletedFiles { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
            public string Summary { get; set; }
        }

        public static async Task<bool> HandleContaminatedGame(string gamePath, string gameName, Form parentForm)
        {
            // First verify what contamination exists
            var verification = CleanFilesVerifier.VerifyCleanInstallation(gamePath);

            if (verification.IsClean)
            {
                return true; // Already clean, proceed
            }

            // Show detailed contamination dialog with restoration option
            var message = $"⚠️ {gameName} is not clean!\n\n";
            message += "Found the following issues:\n";

            int issueCount = 0;
            if (verification.HasSteamSettings)
            {
                message += "• steam_settings folder (crack files)\n";
                issueCount++;
            }
            if (verification.HasBackupFiles)
            {
                message += $"• {verification.FoundBackups.Count} backup files (.bak)\n";
                issueCount++;
            }
            if (verification.HasLobbyShortcuts)
            {
                message += "• Lobby/multiplayer shortcuts\n";
                issueCount++;
            }
            if (verification.FoundCrackArtifacts.Count > 0)
            {
                message += $"• {verification.FoundCrackArtifacts.Count} crack-related files\n";
                issueCount++;
            }

            message += "\n🔧 WOULD YOU LIKE US TO CLEAN THIS UP?\n\n";
            message += "We can automatically:\n";
            message += "✓ Restore original game files from backups\n";
            message += "✓ Remove steam_settings folder\n";
            message += "✓ Delete shortcuts and batch files\n";
            message += "✓ Clean all crack artifacts\n\n";
            message += "After cleanup, you'll have clean files ready to share.\n\n";
            message += "Clean up now?";

            var result = MessageBox.Show(
                message,
                "Clean Install Not Found - Auto Cleanup Available",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Cancel)
            {
                return false; // User cancelled
            }

            if (result == DialogResult.No)
            {
                MessageBox.Show(
                    "Cannot upload cracked files as clean.\n\n" +
                    "To share clean files, you must either:\n" +
                    "• Let us clean it up automatically, or\n" +
                    "• Verify integrity through Steam\n\n" +
                    "Share cancelled.",
                    "Cannot Proceed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );
                return false;
            }

            // User said YES - perform cleanup
            var cleanupResult = await PerformAutomaticCleanup(gamePath, gameName, parentForm);

            if (!cleanupResult.Success)
            {
                ShowCleanupErrorDialog(cleanupResult);
                return false;
            }

            ShowCleanupSuccessDialog(cleanupResult, gameName);
            return true;
        }

        private static async Task<CleanupResult> PerformAutomaticCleanup(string gamePath, string gameName, Form parentForm)
        {
            var result = new CleanupResult();

            try
            {
                parentForm.Cursor = Cursors.WaitCursor;

                // Step 1: Restore .bak files
                var bakFiles = Directory.GetFiles(gamePath, "*.bak", SearchOption.AllDirectories);
                foreach (var bakFile in bakFiles)
                {
                    try
                    {
                        var originalFile = bakFile.Substring(0, bakFile.Length - 4); // Remove .bak

                        // Delete the cracked version if it exists
                        if (File.Exists(originalFile))
                        {
                            File.Delete(originalFile);
                            result.DeletedFiles.Add(Path.GetFileName(originalFile) + " (cracked)");
                        }

                        // Restore the backup
                        File.Move(bakFile, originalFile);
                        result.RestoredFiles.Add(Path.GetFileName(originalFile));
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Failed to restore {Path.GetFileName(bakFile)}: {ex.Message}");
                    }
                }

                // Step 2: Delete steam_settings folder
                var steamSettingsPath = Path.Combine(gamePath, "steam_settings");
                if (Directory.Exists(steamSettingsPath))
                {
                    try
                    {
                        Directory.Delete(steamSettingsPath, true);
                        result.DeletedFiles.Add("steam_settings folder");
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Failed to delete steam_settings: {ex.Message}");
                    }
                }

                // Step 3: Delete shortcuts
                var shortcuts = new List<string>();
                shortcuts.AddRange(Directory.GetFiles(gamePath, "*Lobby*.lnk", SearchOption.TopDirectoryOnly));
                shortcuts.AddRange(Directory.GetFiles(gamePath, "*Multiplayer*.lnk", SearchOption.TopDirectoryOnly));
                shortcuts.AddRange(Directory.GetFiles(gamePath, "*LAN*.lnk", SearchOption.TopDirectoryOnly));
                shortcuts.AddRange(Directory.GetFiles(gamePath, "*Online*.lnk", SearchOption.TopDirectoryOnly));
                shortcuts.AddRange(Directory.GetFiles(gamePath, "*_MP.lnk", SearchOption.TopDirectoryOnly));

                foreach (var shortcut in shortcuts)
                {
                    try
                    {
                        File.Delete(shortcut);
                        result.DeletedFiles.Add(Path.GetFileName(shortcut));
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Failed to delete {Path.GetFileName(shortcut)}: {ex.Message}");
                    }
                }

                // Step 4: Delete batch files we created
                var batchFiles = new List<string>();
                batchFiles.AddRange(Directory.GetFiles(gamePath, "launch_*.bat", SearchOption.TopDirectoryOnly));
                batchFiles.AddRange(Directory.GetFiles(gamePath, "connect_*.bat", SearchOption.TopDirectoryOnly));
                batchFiles.AddRange(Directory.GetFiles(gamePath, "start_*.bat", SearchOption.TopDirectoryOnly));

                foreach (var batchFile in batchFiles)
                {
                    try
                    {
                        File.Delete(batchFile);
                        result.DeletedFiles.Add(Path.GetFileName(batchFile));
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Failed to delete {Path.GetFileName(batchFile)}: {ex.Message}");
                    }
                }

                // Step 5: Delete crack config files
                var crackFiles = new[]
                {
                    "ALI213.ini",
                    "valve.ini",
                    "steam_api.ini",
                    "steam_interfaces.txt",
                    "local_save.txt",
                    "achievements.json",
                    "items.json",
                    "stats.txt",
                    "controller.vdf",
                    "SmartSteamEmu.ini",
                    "ColdClientLoader.ini"
                };

                foreach (var crackFile in crackFiles)
                {
                    var files = Directory.GetFiles(gamePath, crackFile, SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            result.DeletedFiles.Add(Path.GetFileName(file));
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Failed to delete {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
                }

                // Step 6: Special handling for steam_appid.txt
                // Only delete if steam_settings existed (indicates we created it)
                if (Directory.Exists(steamSettingsPath))
                {
                    var steamAppIdFile = Path.Combine(gamePath, "steam_appid.txt");
                    if (File.Exists(steamAppIdFile))
                    {
                        try
                        {
                            File.Delete(steamAppIdFile);
                            result.DeletedFiles.Add("steam_appid.txt");
                        }
                        catch { }
                    }
                }

                // Step 7: Clean up any debug/log files from cracks
                var logFiles = new[]
                {
                    "debug_log.txt",
                    "steam_api.log",
                    "goldberg_steam.log",
                    "SteamEmu.log",
                    "SmartSteamEmu.log"
                };

                foreach (var logFile in logFiles)
                {
                    var files = Directory.GetFiles(gamePath, logFile, SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            result.DeletedFiles.Add(Path.GetFileName(file));
                        }
                        catch { }
                    }
                }

                // Verify the cleanup was successful
                await Task.Delay(500); // Small delay to ensure file system updates

                var finalVerification = CleanFilesVerifier.VerifyCleanInstallation(gamePath);

                if (finalVerification.IsClean)
                {
                    result.Success = true;
                    result.Summary = $"Successfully cleaned {gameName}!\n" +
                                   $"• Restored {result.RestoredFiles.Count} original files\n" +
                                   $"• Deleted {result.DeletedFiles.Count} crack artifacts";
                }
                else
                {
                    result.Success = false;
                    result.Summary = "Cleanup completed but some issues remain.\n" +
                                   "Please verify integrity through Steam.";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Critical error during cleanup: {ex.Message}");
                result.Summary = "Cleanup failed due to errors.";
            }
            finally
            {
                parentForm.Cursor = Cursors.Default;
            }

            return result;
        }

        private static void ShowCleanupSuccessDialog(CleanupResult result, string gameName)
        {
            var successForm = new Form
            {
                Text = "Cleanup Complete!",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblTitle = new Label
            {
                Text = $"✅ {gameName} Cleaned Successfully!",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Lime,
                Location = new Point(20, 20),
                Size = new Size(450, 30)
            };

            var lblSummary = new Label
            {
                Text = result.Summary,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 60),
                Size = new Size(450, 60)
            };

            // Details box
            var txtDetails = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9),
                Location = new Point(20, 130),
                Size = new Size(450, 200)
            };

            var details = "=== RESTORED FILES ===\n";
            foreach (var file in result.RestoredFiles)
            {
                details += $"✓ {file}\n";
            }

            details += "\n=== DELETED ARTIFACTS ===\n";
            foreach (var file in result.DeletedFiles)
            {
                details += $"✗ {file}\n";
            }

            if (result.Errors.Count > 0)
            {
                details += "\n=== WARNINGS ===\n";
                foreach (var error in result.Errors)
                {
                    details += $"⚠ {error}\n";
                }
            }

            txtDetails.Text = details;

            var btnOK = new Button
            {
                Text = "Proceed with Upload",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(200, 35),
                Location = new Point(150, 340),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            successForm.Controls.AddRange(new Control[] { lblTitle, lblSummary, txtDetails, btnOK });
            successForm.ShowDialog();
        }

        private static void ShowCleanupErrorDialog(CleanupResult result)
        {
            var errorForm = new Form
            {
                Text = "Cleanup Failed",
                Size = new Size(500, 350),
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblTitle = new Label
            {
                Text = "❌ Cleanup Failed",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Red,
                Location = new Point(20, 20),
                Size = new Size(450, 30)
            };

            var lblMessage = new Label
            {
                Text = "The automatic cleanup process encountered errors.\n" +
                       "Please verify game integrity through Steam instead.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 60),
                Size = new Size(450, 50)
            };

            var txtErrors = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Consolas", 9),
                Location = new Point(20, 120),
                Size = new Size(450, 150),
                Text = string.Join("\n", result.Errors)
            };

            var btnOK = new Button
            {
                Text = "OK",
                Size = new Size(100, 30),
                Location = new Point(200, 280),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            errorForm.Controls.AddRange(new Control[] { lblTitle, lblMessage, txtErrors, btnOK });
            errorForm.ShowDialog();
        }

        // Quick check if we can auto-clean
        public static bool CanAutoClean(string gamePath)
        {
            // We can auto-clean if we find our backup files
            var bakFiles = Directory.GetFiles(gamePath, "*.bak", SearchOption.AllDirectories);

            foreach (var bakFile in bakFiles)
            {
                var originalName = Path.GetFileNameWithoutExtension(bakFile);

                // Check if it's likely one of our backups (game exe or steam_api dll)
                if (originalName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                    originalName.IndexOf("steam_api", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    originalName.IndexOf("steamclient", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true; // We have backups we can restore
                }
            }

            return false;
        }
    }

    // Integration with share window
    public static class ShareCleanIntegration
    {
        public static async Task<bool> PrepareCleanShare(
            string gamePath,
            string gameName,
            string appId,
            Form parentForm)
        {
            // First check if it's clean
            var verification = CleanFilesVerifier.VerifyCleanInstallation(gamePath);

            if (verification.IsClean)
            {
                return true; // Already clean, good to go
            }

            // Not clean - offer automatic cleanup
            return await AutoCleanupSystem.HandleContaminatedGame(gamePath, gameName, parentForm);
        }
    }
}