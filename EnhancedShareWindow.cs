using APPID;
using SAC_GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    public partial class EnhancedShareWindow : Form
    {

        private Form parentForm;
        private bool gameSizeColumnSortedOnce = false;
        private Button btnCustomPath;

        public EnhancedShareWindow(Form parent)
        {
            parentForm = parent;
            InitializeComponent();  // Use the Designer.cs file instead!

            // Setup modern progress bar style
            SetupModernProgressBar();

            // Add custom sort for GameSize column
            gamesGrid.SortCompare += GamesGrid_SortCompare;
            gamesGrid.ColumnHeaderMouseClick += GamesGrid_ColumnHeaderMouseClick;

            // Make main panel draggable (empty space drags window)
            mainPanel.MouseDown += TitleBar_MouseDown;

            // Make data grid draggable but exclude column resizing
            gamesGrid.MouseDown += DataGrid_MouseDown;

            // Apply acrylic blur and rounded corners
            this.Load += (s, e) =>
            {
                ApplyAcrylicEffect();
                // Center over parent when loaded
                CenterOverParent();
            };
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Ctrl+S opens compression settings for selected game
            if (keyData == (Keys.Control | Keys.S))
            {
                if (gamesGrid.SelectedRows.Count > 0)
                {
                    var row = gamesGrid.SelectedRows[0];
                    var gameName = row.Cells["GameName"].Value?.ToString();
                    var appId = row.Cells["AppID"].Value?.ToString();
                    var installPath = row.Cells["InstallPath"].Value?.ToString();

                    // Trigger share clean by default
                    _ = ShareGame(gameName, installPath, appId, false, row);
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void GamesGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // On first click of GameSize column, sort descending (biggest to smallest)
            if (gamesGrid.Columns[e.ColumnIndex].Name == "GameSize" && !gameSizeColumnSortedOnce)
            {
                gameSizeColumnSortedOnce = true;
                gamesGrid.Sort(gamesGrid.Columns[e.ColumnIndex], System.ComponentModel.ListSortDirection.Descending);
            }
        }

        private void GamesGrid_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            // Sort GameSize column by actual byte value stored in Tag
            if (e.Column.Name == "GameSize")
            {
                long size1 = gamesGrid.Rows[e.RowIndex1].Cells["GameSize"].Tag as long? ?? 0;
                long size2 = gamesGrid.Rows[e.RowIndex2].Cells["GameSize"].Tag as long? ?? 0;

                e.SortResult = size1.CompareTo(size2);
                e.Handled = true;
            }
            // Sort LastUpdated column by actual timestamp stored in Tag
            else if (e.Column.Name == "LastUpdated")
            {
                long time1 = gamesGrid.Rows[e.RowIndex1].Cells["LastUpdated"].Tag as long? ?? 0;
                long time2 = gamesGrid.Rows[e.RowIndex2].Cells["LastUpdated"].Tag as long? ?? 0;

                e.SortResult = time1.CompareTo(time2);
                e.Handled = true;
            }
        }

        private void CenterOverParent()
        {
            if (parentForm != null && parentForm.IsHandleCreated)
            {
                int x = parentForm.Left + (parentForm.Width - this.Width) / 2;
                int y = parentForm.Top + (parentForm.Height - this.Height) / 2;
                this.Location = new Point(x, y);
            }
        }

        private void ApplyAcrylicEffect()
        {
            APPID.AcrylicHelper.ApplyAcrylic(this, roundedCorners: true);
        }

        private void SetupModernProgressBar()
        {
            progressBar.Paint += (s, e) =>
            {
                Rectangle rec = e.ClipRectangle;
                rec.Width = (int)(rec.Width * ((double)progressBar.Value / progressBar.Maximum)) - 4;
                if (ProgressBarRenderer.IsSupported)
                    rec.Height = rec.Height - 4;

                // Draw transparent/dark background
                e.Graphics.Clear(Color.Transparent);

                // Draw blue gradient progress bar
                rec.Height = rec.Height - 4;
                if (rec.Width > 0)
                {
                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        new Point(0, 0),
                        new Point(rec.Width, 0),
                        Color.FromArgb(0, 120, 215),   // Light blue
                        Color.FromArgb(0, 90, 180)))   // Darker blue
                    {
                        e.Graphics.FillRectangle(brush, 2, 2, rec.Width, rec.Height);
                    }
                }
            };
        }

        private async Task LoadGames()
        {
            // Update header
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    var lblHeader = this.Controls.Find("lblHeader", true).FirstOrDefault() as Label;
                    if (lblHeader != null)
                    {
                        lblHeader.Text = "Your Steam Library";
                        lblHeader.ForeColor = Color.FromArgb(100, 200, 255);
                    }
                }));
            }

            // Scan Steam libraries on background thread to avoid blocking UI
            var games = await Task.Run(() => ScanSteamLibraries());

            // Load shared games data
            var sharedGames = LoadSharedGamesData();

            // Add to grid with placeholders, calculate sizes async
            foreach (var game in games)
            {
                var row = gamesGrid.Rows[gamesGrid.Rows.Add()];
                row.Cells["GameName"].Value = game.Name;
                row.Cells["BuildID"].Value = game.BuildId;
                row.Cells["AppID"].Value = game.AppId;
                row.Cells["InstallPath"].Value = game.InstallDir;
                row.Cells["GameSize"].Value = "...";

                // Format and display last updated date
                if (game.LastUpdated > 0)
                {
                    DateTime lastUpdatedDate = DateTimeOffset.FromUnixTimeSeconds(game.LastUpdated).LocalDateTime;
                    row.Cells["LastUpdated"].Value = lastUpdatedDate.ToString("yyyy-MM-dd HH:mm");
                    row.Cells["LastUpdated"].Tag = game.LastUpdated; // Store timestamp for sorting
                }
                else
                {
                    row.Cells["LastUpdated"].Value = "Unknown";
                }

                // Check if we've shared this game before
                string key = $"{game.AppId}_{game.BuildId}";
                if (sharedGames.ContainsKey(key))
                {
                    var sharedData = sharedGames[key];
                    row.Cells["CrackOnly"].Value = sharedData.Contains("cracked_only") ? "✅ Cracked!" : "⚡ Crack";
                    row.Cells["ShareClean"].Value = sharedData.Contains("clean") ? "✅ Shared!" : "📦 Clean";
                    row.Cells["ShareCracked"].Value = sharedData.Contains("cracked") ? "✅ Shared!" : "🎮 Cracked";
                    if (sharedData.Contains("cracked_only")) row.Cells["CrackOnly"].Style.BackColor = Color.FromArgb(60, 0, 60);
                    if (sharedData.Contains("clean")) row.Cells["ShareClean"].Style.BackColor = Color.FromArgb(0, 60, 0);
                    if (sharedData.Contains("cracked")) row.Cells["ShareCracked"].Style.BackColor = Color.FromArgb(0, 60, 0);
                }
                else
                {
                    // Default button texts
                    row.Cells["CrackOnly"].Value = "⚡ Crack";
                    row.Cells["ShareClean"].Value = "📦 Clean";
                    row.Cells["ShareCracked"].Value = "🎮 Cracked";
                }

                // Calculate size asynchronously for each game
                int rowIndex = row.Index;
                _ = Task.Run(() =>
                {
                    long dirSize = 0;
                    try
                    {
                        if (Directory.Exists(game.InstallDir))
                        {
                            dirSize = new DirectoryInfo(game.InstallDir)
                                .EnumerateFiles("*", SearchOption.AllDirectories)
                                .Sum(file => file.Length);
                        }
                    }
                    catch { }

                    // Update UI on main thread
                    this.Invoke(new Action(() =>
                    {
                        if (rowIndex < gamesGrid.Rows.Count)
                        {
                            gamesGrid.Rows[rowIndex].Cells["GameSize"].Value = FormatFileSize(dirSize);
                            gamesGrid.Rows[rowIndex].Cells["GameSize"].Tag = dirSize; // Store actual bytes for sorting
                        }
                    }));
                });
            }
        }


        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private async void GamesGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = gamesGrid.Rows[e.RowIndex];
            var gameName = row.Cells["GameName"].Value?.ToString();
            var appId = row.Cells["AppID"].Value?.ToString();
            var installPath = row.Cells["InstallPath"].Value?.ToString();

            // No request button anymore - this window is for sharing YOUR games

            // Handle Crack Only button
            if (e.ColumnIndex == gamesGrid.Columns["CrackOnly"].Index)
            {
                await CrackOnlyGame(gameName, installPath, appId, row);
                return;
            }

            // Handle Share Clean button
            if (e.ColumnIndex == gamesGrid.Columns["ShareClean"].Index)
            {
                await ShareGame(gameName, installPath, appId, false, row);
                return;
            }

            // Handle Share Cracked button
            if (e.ColumnIndex == gamesGrid.Columns["ShareCracked"].Index)
            {
                await ShareGame(gameName, installPath, appId, true, row);
                return;
            }
        }

        private async Task CrackOnlyGame(string gameName, string installPath, string appId, DataGridViewRow row)
        {
            try
            {
                // Check if game path is valid
                if (string.IsNullOrEmpty(installPath) || !Directory.Exists(installPath))
                {
                    MessageBox.Show("Game installation path not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Verify this is the main form
                var mainForm = parentForm as SteamAppId;
                if (mainForm == null)
                {
                    MessageBox.Show("Cannot access cracking functionality.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Update UI to show cracking in progress
                row.Cells["CrackOnly"].Value = "⚙️ Cracking...";
                row.Cells["CrackOnly"].Style.BackColor = Color.FromArgb(30, 30, 0); // Subtle yellow tint

                // Create translucent overlay
                var overlay = new Form
                {
                    StartPosition = FormStartPosition.Manual,
                    FormBorderStyle = FormBorderStyle.None,
                    BackColor = Color.Black,
                    Opacity = 0.7,
                    ShowInTaskbar = false,
                    TopMost = true,
                    Location = this.Location,
                    Size = this.Size
                };

                var statusLabel = new Label
                {
                    Text = $"Cracking {gameName}...",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.Cyan,
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                statusLabel.Location = new Point(
                    (overlay.Width - statusLabel.Width) / 2,
                    (overlay.Height - statusLabel.Height) / 2
                );
                overlay.Controls.Add(statusLabel);
                overlay.Show();

                try
                {
                    // Set game directory and APPID for cracking
                    mainForm.GameDirectory = installPath;
                    SteamAppId.APPID = appId;

                    // Perform the crack
                    bool success = await mainForm.CrackAsync();

                    overlay.Close();

                    if (success)
                    {
                        row.Cells["CrackOnly"].Value = "✅ Cracked!";
                        row.Cells["CrackOnly"].Style.BackColor = Color.FromArgb(60, 0, 60); // Purple tint for cracked

                        // Save that we cracked this game
                        SaveSharedGame(appId, row.Cells["BuildID"].Value?.ToString(), "cracked_only");

                        MessageBox.Show($"{gameName} has been cracked successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        row.Cells["CrackOnly"].Value = "⚡ Crack";
                        row.Cells["CrackOnly"].Style.BackColor = Color.FromArgb(8, 8, 12); // Reset to default
                        MessageBox.Show($"Failed to crack {gameName}. Check the main window for details.",
                            "Crack Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    overlay.Close();
                    row.Cells["CrackOnly"].Value = "⚡ Crack";
                    row.Cells["CrackOnly"].Style.BackColor = Color.FromArgb(8, 8, 12); // Reset to default

                    System.Diagnostics.Debug.WriteLine($"[CRACK-ONLY] Error: {ex}");
                    MessageBox.Show($"Error cracking {gameName}:\n{ex.Message}",
                        "Crack Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                row.Cells["CrackOnly"].Value = "⚡ Crack";
            }
        }

        private async Task ShareGame(string gameName, string installPath, string appId, bool cracked, DataGridViewRow row)
        {
            if (string.IsNullOrEmpty(installPath) || !Directory.Exists(installPath))
            {
                MessageBox.Show("Game installation path not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // If sharing clean files, restore all .bak files and clean up crack artifacts
            if (!cracked)
            {
                try
                {
                    var parentFormTyped = parentForm as SteamAppId;
                    if (parentFormTyped != null)
                    {
                        // Restore .bak files
                        try
                        {
                            var bakFiles = Directory.GetFiles(installPath, "*.bak", SearchOption.AllDirectories);
                            if (bakFiles.Length > 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Found {bakFiles.Length} .bak files, restoring clean files...");
                                foreach (var bakFile in bakFiles)
                                {
                                    try
                                    {
                                        var originalFile = bakFile.Substring(0, bakFile.Length - 4); // Remove .bak
                                        if (File.Exists(originalFile))
                                            File.Delete(originalFile);
                                        File.Move(bakFile, originalFile);
                                        System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Restored {Path.GetFileName(bakFile)}");
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to restore {bakFile}: {ex.Message}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to search for .bak files: {ex.Message}");
                        }

                        // Delete steam_settings directories (recursively search for all instances)
                        try
                        {
                            var steamSettingsDirs = Directory.GetDirectories(installPath, "steam_settings", SearchOption.AllDirectories);
                            foreach (var steamSettingsDir in steamSettingsDirs)
                            {
                                try
                                {
                                    Directory.Delete(steamSettingsDir, true);
                                    System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Deleted steam_settings directory: {steamSettingsDir}");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to delete steam_settings at {steamSettingsDir}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to search for steam_settings directories: {ex.Message}");
                        }

                        // Delete lobby_connect files (all variations: _lobby_connect* and lobby_connect*)
                        try
                        {
                            // Search for _lobby_connect* pattern
                            var lobbyConnectFiles = Directory.GetFiles(installPath, "_lobby_connect*", SearchOption.AllDirectories);
                            foreach (var lobbyConnectFile in lobbyConnectFiles)
                            {
                                try
                                {
                                    File.Delete(lobbyConnectFile);
                                    System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Deleted lobby_connect file: {lobbyConnectFile}");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to delete lobby_connect file at {lobbyConnectFile}: {ex.Message}");
                                }
                            }

                            // Also search for lobby_connect* pattern (without underscore prefix)
                            lobbyConnectFiles = Directory.GetFiles(installPath, "lobby_connect*", SearchOption.AllDirectories);
                            foreach (var lobbyConnectFile in lobbyConnectFiles)
                            {
                                try
                                {
                                    File.Delete(lobbyConnectFile);
                                    System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Deleted lobby_connect file: {lobbyConnectFile}");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to delete lobby_connect file at {lobbyConnectFile}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to search for lobby_connect files: {ex.Message}");
                        }

                        // Delete shortcuts (*.lnk files in the game directory)
                        try
                        {
                            var shortcuts = Directory.GetFiles(installPath, "*.lnk", SearchOption.TopDirectoryOnly);
                            if (shortcuts.Length > 0)
                            {
                                foreach (var shortcut in shortcuts)
                                {
                                    try
                                    {
                                        File.Delete(shortcut);
                                        System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Deleted shortcut: {Path.GetFileName(shortcut)}");
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to delete shortcut {Path.GetFileName(shortcut)}: {ex.Message}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to search for shortcuts: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Error during clean file preparation: {ex.Message}");
                }
            }

            // If sharing cracked version, first crack the game
            if (cracked)
            {
                var result = MessageBox.Show(
                    $"This will crack {gameName} using Goldberg/ALI213 emulators.\n\n" +
                    "The game files will be modified. A backup (.bak) will be created.\n\n" +
                    "Continue?",
                    "Crack Game First?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Call the main form's cracking method
                var parentFormTyped = parentForm as SteamAppId;
                if (parentFormTyped != null)
                {
                    // Suppress status updates on main window while cracking from share window
                    parentFormTyped.SetSuppressStatusUpdates(true);

                    // Set the game directory and app ID in the main form
                    parentFormTyped.GameDirectory = installPath;
                    SteamAppId.APPID = appId;

                    // Show status with visual indicator
                    row.Cells["ShareCracked"].Value = "⚙️ Cracking...";
                    row.Cells["ShareCracked"].Style.BackColor = Color.FromArgb(30, 30, 0); // Subtle yellow tint

                    // Create translucent overlay
                    var overlay = new Panel
                    {
                        Size = this.ClientSize,
                        Location = new Point(0, 0),
                        BackColor = Color.FromArgb(180, 5, 8, 20), // Semi-transparent dark
                        Visible = true
                    };
                    // Enable double buffering to prevent flickering
                    typeof(Panel).InvokeMember("DoubleBuffered",
                        System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                        null, overlay, new object[] { true });
                    // Make overlay draggable
                    overlay.MouseDown += TitleBar_MouseDown;

                    var lblCracking = new Label
                    {
                        Text = $"⚙️ Cracking {gameName}...\n\nInitializing...",
                        Font = new Font("Segoe UI", 14, FontStyle.Bold),
                        ForeColor = Color.FromArgb(100, 200, 255),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill
                    };
                    // Make label draggable too
                    lblCracking.MouseDown += TitleBar_MouseDown;

                    overlay.Controls.Add(lblCracking);
                    this.Controls.Add(overlay);
                    overlay.BringToFront();
                    Application.DoEvents(); // Force UI update

                    // Helper to update crack progress
                    Action<string> updateProgress = (status) =>
                    {
                        if (lblCracking.IsHandleCreated)
                        {
                            lblCracking.Invoke(new Action(() =>
                            {
                                lblCracking.Text = $"⚙️ Cracking {gameName}...\n\n{status}";
                            }));
                        }
                    };

                    // Subscribe to main form's status updates
                    EventHandler<string> statusHandler = (s, status) => updateProgress(status);
                    parentFormTyped.CrackStatusChanged += statusHandler;

                    try
                    {
                        // Crack the game
                        bool crackSuccess = await parentFormTyped.CrackAsync();

                        // Unsubscribe
                        parentFormTyped.CrackStatusChanged -= statusHandler;

                        // Re-enable status updates
                        parentFormTyped.SetSuppressStatusUpdates(false);

                        // Remove overlay
                        this.Controls.Remove(overlay);
                        overlay.Dispose();

                        if (!crackSuccess)
                        {
                            MessageBox.Show(
                                "Failed to crack the game. This could be because:\n\n" +
                                "• The game doesn't have steam_api.dll or steam_api64.dll\n" +
                                "• The game is already cracked\n" +
                                "• Missing required files in _bin folder\n\n" +
                                "You can still share the game as Clean instead.",
                                "Crack Failed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            row.Cells["ShareCracked"].Value = "🎮 Cracked";
                            row.Cells["ShareCracked"].Style.BackColor = Color.FromArgb(8, 8, 12); // Reset to default
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Re-enable status updates
                        parentFormTyped.SetSuppressStatusUpdates(false);

                        // Remove overlay
                        this.Controls.Remove(overlay);
                        overlay.Dispose();

                        // Write full error to file for debugging
                        string errorLog = $"_ShareCrackError_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                        File.WriteAllText(errorLog,
                            $"=== Share Cracked Error ===\n" +
                            $"Game: {gameName}\n" +
                            $"AppID: {appId}\n" +
                            $"Path: {installPath}\n" +
                            $"Time: {DateTime.Now}\n\n" +
                            $"Exception Type: {ex.GetType().Name}\n" +
                            $"Message: {ex.Message}\n\n" +
                            $"Stack Trace:\n{ex.StackTrace}\n\n" +
                            $"Full Exception:\n{ex.ToString()}");

                        // Show user-friendly error
                        MessageBox.Show(
                            $"Error during cracking process:\n\n{ex.Message}\n\n" +
                            $"Full error details saved to:\n{errorLog}\n\n" +
                            "You can still share the game as Clean instead.",
                            "Crack Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        row.Cells["ShareCracked"].Value = "🎮 Cracked";
                        row.Cells["ShareCracked"].Style.BackColor = Color.FromArgb(8, 8, 12); // Reset to default
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Cannot access cracking functionality. Please use the main window to crack games.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                // Show the compression settings dialog
                using (var compressionForm = new SteamAutocrackGUI.CompressionSettingsForm())
                {
                    compressionForm.StartPosition = FormStartPosition.CenterParent;
                    compressionForm.ShowInTaskbar = false;  // Don't show in taskbar

                    if (compressionForm.ShowDialog(this) != DialogResult.OK)
                    {
                        // User cancelled
                        return;
                    }

                    // Update button to show processing
                    var btnColumn = cracked ? "ShareCracked" : "ShareClean";
                    row.Cells[btnColumn].Value = "⏳ Compressing...";

                    // Get selected settings
                    string format = compressionForm.SelectedFormat;
                    string level = compressionForm.SelectedLevel;
                    bool usePassword = compressionForm.UseRinPassword;

                    // Build output path
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string prefix = cracked ? "[SACGUI] CRACKED" : "[SACGUI] CLEAN";
                    // Sanitize filename - remove invalid characters but keep spaces
                    string safeGameName = gameName;
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        safeGameName = safeGameName.Replace(c.ToString(), "");
                    }
                    string zipName = $"{prefix} {safeGameName}.{format.ToLower()}";
                    string outputPath = Path.Combine(desktopPath, zipName);

                    // For clean shares, prepare proper Steam folder structure
                    string pathToCompress = installPath;
                    string tempCleanFolder = null;
                    bool isCleanStructure = false;

                    if (!cracked)
                    {
                        // Prepare clean game structure with depotcache and steamapps
                        string buildId = row.Cells["BuildID"].Value?.ToString() ?? "Unknown";
                        tempCleanFolder = await Task.Run(() => PrepareCleanGameStructure(appId, gameName, installPath, buildId));

                        if (!string.IsNullOrEmpty(tempCleanFolder))
                        {
                            pathToCompress = tempCleanFolder;
                            isCleanStructure = true;
                            System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Using prepared structure: {pathToCompress}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to prepare structure, using direct path");
                        }
                    }

                    // Show progress window
                    var progressForm = new RGBProgressWindow(gameName, cracked ? "Cracked" : "Clean");
                    progressForm.TopMost = this.TopMost;
                    progressForm.Show(this);
                    progressForm.CenterOverParent(this);
                    progressForm.UpdateStatus($"Compressing with {format.ToUpper()} level {level}..." + (usePassword ? " (Password protected)" : ""));

                    // Compress the game using the real compression with optional password
                    bool compressionSuccess = await Task.Run(() => CompressGameProper(pathToCompress, outputPath, format, level, usePassword ? "cs.rin.ru" : null, progressForm, isCleanStructure));

                    // Clean up temporary folder if created
                    if (!string.IsNullOrEmpty(tempCleanFolder) && Directory.Exists(tempCleanFolder))
                    {
                        try
                        {
                            Directory.Delete(tempCleanFolder, true);
                            System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Cleaned up temp folder: {tempCleanFolder}");
                        }
                        catch (Exception cleanupEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Failed to cleanup temp folder: {cleanupEx.Message}");
                        }
                    }

                    progressForm.Close();

                    if (!compressionSuccess)
                    {
                        MessageBox.Show("Compression failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        row.Cells[btnColumn].Value = cracked ? "🎮 Cracked" : "📦 Clean";
                        return;
                    }

                    // Compression done! Now show dialog with upload/explorer options
                    row.Cells[btnColumn].Value = "✅ Compressed";

                    var completionDialog = new Form
                    {
                        Text = "Compression Complete!",
                        Size = new Size(400, 200),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.None,
                        BackColor = Color.FromArgb(5, 8, 20)
                    };

                    // Apply acrylic to completion dialog
                    completionDialog.Load += (s, e) =>
                    {
                        APPID.AcrylicHelper.ApplyAcrylic(completionDialog, roundedCorners: true);
                    };

                    var label = new Label
                    {
                        Text = $"Successfully compressed:\n{zipName}\n\nWhat would you like to do?",
                        Size = new Size(360, 60),
                        Location = new Point(20, 20),
                        ForeColor = Color.White,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    var uploadBtn = new Button
                    {
                        Text = "📤 Upload to Backend",
                        Size = new Size(150, 40),
                        Location = new Point(40, 100),
                        BackColor = Color.FromArgb(40, 40, 50),
                        ForeColor = Color.FromArgb(150, 255, 150),
                        FlatStyle = FlatStyle.Flat,
                        DialogResult = DialogResult.Yes
                    };
                    uploadBtn.FlatAppearance.BorderColor = Color.FromArgb(150, 255, 150);
                    uploadBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 60);

                    var explorerBtn = new Button
                    {
                        Text = "📁 Show in Explorer",
                        Size = new Size(150, 40),
                        Location = new Point(210, 100),
                        BackColor = Color.FromArgb(40, 40, 50),
                        ForeColor = Color.FromArgb(150, 255, 150),
                        FlatStyle = FlatStyle.Flat,
                        DialogResult = DialogResult.No
                    };
                    explorerBtn.FlatAppearance.BorderColor = Color.FromArgb(150, 255, 150);
                    explorerBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 60);

                    completionDialog.Controls.Add(label);
                    completionDialog.Controls.Add(uploadBtn);
                    completionDialog.Controls.Add(explorerBtn);

                    // Match parent form's TopMost setting
                    completionDialog.TopMost = this.TopMost;

                    var result = completionDialog.ShowDialog(this);

                    if (result == DialogResult.Yes)
                    {
                        // User chose to upload
                        row.Cells[btnColumn].Value = "⏳ Uploading...";

                        var uploadProgress = new RGBProgressWindow(gameName, "Uploading");
                        uploadProgress.TopMost = this.TopMost;
                        string uploadUrl = await UploadFileWithProgress(outputPath, uploadProgress);

                        if (!string.IsNullOrEmpty(uploadUrl))
                        {
                            // Check if it's a 1fichier URL (not converted yet)
                            bool isOneFichier = uploadUrl.Contains("1fichier.com");

                            System.Diagnostics.Debug.WriteLine($"[SHARE] Final upload URL to show user: {uploadUrl} (isOneFichier: {isOneFichier})");

                            row.Cells[btnColumn].Value = "✅ Shared!";
                            row.Cells[btnColumn].Style.BackColor = Color.FromArgb(0, 60, 0);

                            // Save shared game data
                            SaveSharedGame(appId, row.Cells["BuildID"].Value?.ToString(), cracked ? "cracked" : "clean");

                            // Get file size for conversion timing
                            long fileSize = new FileInfo(outputPath).Length;

                            // Show success modal with optional convert button for 1fichier links
                            ShowUploadSuccessWithConvert(uploadUrl, gameName, cracked, isOneFichier, fileSize);
                        }
                        else
                        {
                            row.Cells[btnColumn].Value = "❌ Upload Failed";
                        }
                    }
                    else
                    {
                        // User chose to show in explorer (save locally)
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{outputPath}\"");
                        row.Cells[btnColumn].Value = "💾 Saved";

                        // Still save shared game data even if just saved locally
                        SaveSharedGame(appId, row.Cells["BuildID"].Value?.ToString(), cracked ? "cracked" : "clean");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                row.Cells[cracked ? "ShareCracked" : "ShareClean"].Value = cracked ? "🎮 Cracked" : "📦 Clean";
            }
        }

        /// <summary>
        /// Prepares the proper Steam folder structure for clean game sharing
        /// </summary>
        private string PrepareCleanGameStructure(string appId, string gameName, string installPath, string buildId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Preparing structure for {gameName} (AppID: {appId})");

                // Find the ACF file for this game
                string acfFilePath = null;
                string acfContent = null;

                var steamPaths = GetSteamLibraryPaths();
                foreach (var steamPath in steamPaths)
                {
                    var potentialAcfPath = Path.Combine(steamPath, $"appmanifest_{appId}.acf");
                    if (File.Exists(potentialAcfPath))
                    {
                        acfFilePath = potentialAcfPath;
                        acfContent = File.ReadAllText(potentialAcfPath);
                        System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Found ACF: {acfFilePath}");
                        break;
                    }
                }

                if (string.IsNullOrEmpty(acfFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] No ACF file found for AppID {appId}");
                    return null;
                }

                // Parse installed depots from ACF
                var depots = ParseInstalledDepots(acfContent);
                System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Found {depots.Count} depots");

                // Find depot manifests
                var manifestFiles = FindDepotManifests(appId, depots);
                System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Found {manifestFiles.Count} depot manifests");

                // Get install directory name from ACF
                var acfData = ParseAcfFile(acfContent);
                string installDir = acfData.ContainsKey("installdir") ? acfData["installdir"] : Path.GetFileName(installPath);

                // Create temp folder with proper naming: GameName.Build.BuildID.Win64.public
                string tempBasePath = Path.Combine(Path.GetTempPath(), "SACGUI_Clean_" + Guid.NewGuid().ToString("N").Substring(0, 8));
                string cleanFolderName = $"{gameName.Replace(" ", ".")}.Build.{buildId}.Win64.public";
                // Sanitize folder name
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    cleanFolderName = cleanFolderName.Replace(c.ToString(), "");
                }

                string cleanFolderPath = Path.Combine(tempBasePath, cleanFolderName);
                Directory.CreateDirectory(cleanFolderPath);

                // Create depotcache folder and copy manifests
                string depotcachePath = Path.Combine(cleanFolderPath, "depotcache");
                Directory.CreateDirectory(depotcachePath);

                foreach (var manifestFile in manifestFiles)
                {
                    string destPath = Path.Combine(depotcachePath, Path.GetFileName(manifestFile));
                    File.Copy(manifestFile, destPath, true);
                    System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Copied manifest: {Path.GetFileName(manifestFile)}");
                }

                // Create steamapps folder
                string steamappsPath = Path.Combine(cleanFolderPath, "steamapps");
                Directory.CreateDirectory(steamappsPath);

                // Copy ACF file
                string destAcfPath = Path.Combine(steamappsPath, Path.GetFileName(acfFilePath));
                File.Copy(acfFilePath, destAcfPath, true);
                System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Copied ACF file");

                // Create common folder and copy game files
                string commonPath = Path.Combine(steamappsPath, "common");
                Directory.CreateDirectory(commonPath);

                string gameDestPath = Path.Combine(commonPath, installDir);
                System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Copying game files from {installPath} to {gameDestPath}");

                // Copy entire game directory
                CopyDirectory(installPath, gameDestPath);

                System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Structure prepared successfully: {cleanFolderPath}");
                return cleanFolderPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Error preparing structure: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SHARE CLEAN] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Converts a path to long path format (bypasses 260 character limit)
        /// </summary>
        private string ToLongPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // Already a long path
            if (path.StartsWith(@"\\?\"))
                return path;

            // UNC path
            if (path.StartsWith(@"\\"))
                return @"\\?\UNC\" + path.Substring(2);

            // Regular path - must be absolute
            if (Path.IsPathRooted(path))
                return @"\\?\" + path;

            // Relative path - convert to absolute first
            return @"\\?\" + Path.GetFullPath(path);
        }

        /// <summary>
        /// Recursively copies a directory and all its contents using long paths
        /// </summary>
        private void CopyDirectory(string sourceDir, string destDir)
        {
            // Convert to long paths to bypass 260 character limit
            string longSourceDir = ToLongPath(sourceDir);
            string longDestDir = ToLongPath(destDir);

            // Ensure destination directory exists
            if (!Directory.Exists(longDestDir))
            {
                Directory.CreateDirectory(longDestDir);
            }

            // Copy files
            foreach (string file in Directory.GetFiles(longSourceDir))
            {
                try
                {
                    string destFile = Path.Combine(longDestDir, Path.GetFileName(file));

                    // Ensure parent directory exists
                    string destFileDir = Path.GetDirectoryName(destFile);
                    if (!Directory.Exists(destFileDir))
                    {
                        Directory.CreateDirectory(destFileDir);
                    }

                    File.Copy(file, destFile, true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[COPY] Error copying file {file}: {ex.Message}");
                    throw; // Re-throw to fail the operation
                }
            }

            // Copy subdirectories
            foreach (string subDir in Directory.GetDirectories(longSourceDir))
            {
                string destSubDir = Path.Combine(longDestDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        private bool CompressGameProper(string sourcePath, string outputPath, string format, string level, string password = null, RGBProgressWindow progressWindow = null, bool includeParentFolder = false)
        {
            try
            {
                // Use the embedded 7-Zip from _bin folder (64-bit version)
                string sevenZipPath = Path.Combine(Environment.CurrentDirectory, "_bin", "7z", "7za.exe");

                // Fallback to Program Files if _bin version doesn't exist
                if (!File.Exists(sevenZipPath))
                {
                    sevenZipPath = @"C:\Program Files\7-Zip\7z.exe";
                    if (!File.Exists(sevenZipPath))
                    {
                        sevenZipPath = @"C:\Program Files (x86)\7-Zip\7z.exe";
                    }
                }

                if (File.Exists(sevenZipPath))
                {
                    // Use 7-Zip with proper compression level
                    string compressionSwitch = "";
                    int levelNum = int.Parse(level ?? "0");

                    if (levelNum == 0)
                        compressionSwitch = "-mx0";  // Store only
                    else if (levelNum <= 3)
                        compressionSwitch = "-mx1";  // Fastest
                    else if (levelNum <= 6)
                        compressionSwitch = "-mx5";  // Normal
                    else if (levelNum <= 9)
                        compressionSwitch = "-mx7";  // Maximum
                    else
                    {
                        compressionSwitch = "-mx9";  // Ultra

                        // For ultra compression on large files, add solid block size limit
                        // This prevents memory exhaustion during compression
                        compressionSwitch += " -ms=512m";  // 512MB solid blocks instead of unlimited
                    }

                    string archiveType = format.ToLower() == "7z" ? "7z" : "zip";

                    // Smart dictionary size based on compression level and available RAM
                    string dictParams = "";
                    if (archiveType == "7z" && levelNum >= 7)
                    {
                        try
                        {
                            // Get available RAM
                            var pc = new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes");
                            float availRAM = pc.NextValue();

                            // Calculate appropriate dictionary size (in MB)
                            // 64-bit 7z.exe can use MUCH more memory!
                            int dictSize = 256; // Default 256MB

                            if (availRAM < 2048)
                                dictSize = 64;   // Very low RAM
                            else if (availRAM < 4096)
                                dictSize = 128;  // Low RAM
                            else if (availRAM < 8192)
                                dictSize = 256;  // Moderate RAM
                            else if (availRAM < 16384)
                                dictSize = 512;  // Good RAM
                            else if (availRAM < 32768)
                                dictSize = 1024; // Great RAM (1GB dictionary)
                            else
                                dictSize = 1536; // Excellent RAM (1.5GB dictionary for your 64GB system!)

                            // Add memory limit for working buffers (separate from dictionary)
                            // This prevents runaway memory usage during compression
                            dictParams = $" -md={dictSize}m -mmt=on -mmem={dictSize * 3}m";
                            System.Diagnostics.Debug.WriteLine($"[7z] Using dictionary size: {dictSize}MB (Available RAM: {availRAM}MB)");
                        }
                        catch
                        {
                            // If we can't detect RAM, use safe default
                            dictParams = " -md=256m";
                            System.Diagnostics.Debug.WriteLine("[7z] Could not detect RAM, using 256MB dictionary");
                        }
                    }

                    // Add password if provided
                    string passwordSwitch = !string.IsNullOrEmpty(password) ? $"-p\"{password}\"" : "";

                    // For clean structure, we need to include the parent folder in the archive
                    string arguments;
                    string workingDirectory = null;

                    if (includeParentFolder)
                    {
                        // Compress from parent directory to include folder name in archive
                        string parentDir = Path.GetDirectoryName(sourcePath);
                        string folderName = Path.GetFileName(sourcePath);
                        workingDirectory = parentDir;
                        // Use trailing backslash to indicate it's a directory (no -r needed, 7z will recurse automatically)
                        arguments = $"a -t{archiveType} {compressionSwitch}{dictParams} {passwordSwitch} -bsp1 \"{outputPath}\" \"{folderName}\\\"";
                    }
                    else
                    {
                        // Compress contents only (original behavior)
                        arguments = $"a -t{archiveType} {compressionSwitch}{dictParams} {passwordSwitch} -bsp1 \"{outputPath}\" \"{sourcePath}\\*\"";
                    }

                    System.Diagnostics.Debug.WriteLine($"[7z] Command: 7za.exe {arguments}");
                    System.Diagnostics.Debug.WriteLine($"[7z] Working dir: {workingDirectory ?? "default"}");

                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = sevenZipPath,
                        Arguments = arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    if (!string.IsNullOrEmpty(workingDirectory))
                    {
                        psi.WorkingDirectory = workingDirectory;
                    }

                    using (var process = System.Diagnostics.Process.Start(psi))
                    {
                        var errorOutput = new System.Text.StringBuilder();

                        // Read output asynchronously to capture progress
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                System.Diagnostics.Debug.WriteLine($"[7z OUT] {e.Data}");

                                if (progressWindow != null && progressWindow.IsHandleCreated)
                                {
                                    // 7-Zip outputs progress like "5%" or " 42%"
                                    var match = System.Text.RegularExpressions.Regex.Match(e.Data, @"(\d+)%");
                                    if (match.Success)
                                    {
                                        int percentage = int.Parse(match.Groups[1].Value);
                                        try
                                        {
                                            progressWindow.Invoke(new Action(() =>
                                            {
                                                progressWindow.progressBar.Value = Math.Min(percentage, 100);
                                                progressWindow.lblStatus.Text = $"Compressing... {percentage}%";
                                            }));
                                        }
                                        catch { }
                                    }
                                }
                            }
                        };

                        // Capture stderr for error messages
                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                System.Diagnostics.Debug.WriteLine($"[7z ERR] {e.Data}");
                                errorOutput.AppendLine(e.Data);
                            }
                        };

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        // NO TIMEOUT - Let it run as long as needed
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            string errorMsg = errorOutput.ToString();
                            System.Diagnostics.Debug.WriteLine($"[7z FAILED] Exit code: {process.ExitCode}");
                            System.Diagnostics.Debug.WriteLine($"[7z FAILED] Errors: {errorMsg}");

                            // Check for memory errors
                            if (errorMsg.Contains("Can't allocate") || errorMsg.Contains("memory") || errorMsg.Contains("ERROR:"))
                            {
                                var result = MessageBox.Show(
                                    "7-Zip ran out of memory during compression!\n\n" +
                                    "This usually happens with ultra compression on large files.\n\n" +
                                    "Options:\n" +
                                    "• YES - Retry with lower compression (level 5)\n" +
                                    "• NO - Cancel compression\n\n" +
                                    "For best results, install 64-bit 7-Zip from:\n" +
                                    "https://7-zip.org/download.html",
                                    "Memory Error",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);

                                if (result == DialogResult.Yes)
                                {
                                    // Retry with lower compression
                                    System.Diagnostics.Debug.WriteLine("[7z] Retrying with lower compression level 5");

                                    // Delete failed partial archive
                                    if (File.Exists(outputPath))
                                    {
                                        try { File.Delete(outputPath); } catch { }
                                    }

                                    // Retry with level 5 (normal compression)
                                    string retryArgs = $"a -t{archiveType} -mx5 -md=64m {passwordSwitch} -bsp1 \"{outputPath}\" \"{sourcePath}\\*\" -r";

                                    var retryPsi = new System.Diagnostics.ProcessStartInfo
                                    {
                                        FileName = sevenZipPath,
                                        Arguments = retryArgs,
                                        CreateNoWindow = true,
                                        UseShellExecute = false,
                                        RedirectStandardOutput = true,
                                        RedirectStandardError = true
                                    };

                                    using (var retryProcess = System.Diagnostics.Process.Start(retryPsi))
                                    {
                                        retryProcess.OutputDataReceived += (sender, e) =>
                                        {
                                            if (!string.IsNullOrEmpty(e.Data) && progressWindow != null && progressWindow.IsHandleCreated)
                                            {
                                                var match = System.Text.RegularExpressions.Regex.Match(e.Data, @"(\d+)%");
                                                if (match.Success)
                                                {
                                                    int percentage = int.Parse(match.Groups[1].Value);
                                                    try
                                                    {
                                                        progressWindow.Invoke(new Action(() =>
                                                        {
                                                            progressWindow.progressBar.Value = Math.Min(percentage, 100);
                                                            progressWindow.lblStatus.Text = $"Compressing (reduced)... {percentage}%";
                                                        }));
                                                    }
                                                    catch { }
                                                }
                                            }
                                        };

                                        retryProcess.BeginOutputReadLine();
                                        retryProcess.WaitForExit();
                                        return retryProcess.ExitCode == 0;
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(errorMsg))
                            {
                                MessageBox.Show($"7-Zip compression failed:\n{errorMsg}", "Compression Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        return process.ExitCode == 0;
                    }
                }
                else if (format.ToLower() == "zip")
                {
                    // Fallback to built-in ZIP compression with progress
                    var compressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                    int levelNum = int.Parse(level ?? "5");

                    if (levelNum == 0)
                        compressionLevel = System.IO.Compression.CompressionLevel.NoCompression;
                    else if (levelNum <= 5)
                        compressionLevel = System.IO.Compression.CompressionLevel.Fastest;
                    else
                        compressionLevel = System.IO.Compression.CompressionLevel.Optimal;

                    // Get all files first for progress tracking
                    var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
                    int totalFiles = files.Length;
                    int currentFile = 0;

                    using (var archive = System.IO.Compression.ZipFile.Open(outputPath, System.IO.Compression.ZipArchiveMode.Create))
                    {
                        foreach (string file in files)
                        {
                            currentFile++;
                            string relativePath = file.Substring(sourcePath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                            archive.CreateEntryFromFile(file, relativePath, compressionLevel);

                            int percentage = (currentFile * 100) / totalFiles;
                            if (progressWindow != null && progressWindow.IsHandleCreated)
                            {
                                try
                                {
                                    progressWindow.Invoke(new Action(() =>
                                    {
                                        progressWindow.progressBar.Value = Math.Min(percentage, 100);
                                        progressWindow.lblStatus.Text = $"Compressing... {percentage}% ({currentFile}/{totalFiles} files)";
                                    }));
                                }
                                catch { }
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    MessageBox.Show("7-Zip not found! Install 7-Zip for 7z format support.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Compression error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<string> UploadFileWithProgress(string filePath, RGBProgressWindow progressWindow)
        {
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] === Starting 1fichier upload from EnhancedShareWindow ===");
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] File: {filePath}");
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] File exists: {File.Exists(filePath)}");
            if (File.Exists(filePath))
            {
                var fi = new FileInfo(filePath);
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] File size: {fi.Length / (1024.0 * 1024.0):F2} MB");
            }

            try
            {
                var fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                // Create progress handler that updates UI thread - only updates progress bar, not status text
                var progress = new Progress<double>(value =>
                {
                    var percentage = (int)(value * 100);

                    // Only update progress bar if we have actual progress
                    if (progressWindow != null && !progressWindow.IsDisposed && progressWindow.IsHandleCreated)
                    {
                        try
                        {
                            progressWindow.BeginInvoke(new Action(() =>
                            {
                                // Only update progress bar - status text is handled by statusCallback with MB/s info
                                progressWindow.progressBar.Value = Math.Max(0, Math.Min(100, percentage));
                            }));
                        }
                        catch { }
                    }
                });

                // Status callback for upload status with speed info
                var statusProgress = new Progress<string>(status =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[STATUS CALLBACK] {status}");
                        if (progressWindow != null && !progressWindow.IsDisposed && progressWindow.IsHandleCreated)
                        {
                            progressWindow.BeginInvoke(new Action(() =>
                            {
                                progressWindow.lblStatus.Text = status;
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[STATUS CALLBACK ERROR] {ex.Message}");
                    }
                });

                // Show the progress window centered over parent (after creating progress handlers)
                progressWindow.TopMost = this.TopMost;
                progressWindow.lblStatus.Text = $"Starting upload...";
                progressWindow.Show(this);
                progressWindow.CenterOverParent(this);
                progressWindow.BringToFront();
                Application.DoEvents(); // Allow UI to update

                // Run upload on background thread to avoid UI lock
                OneFichierUploader.UploadResult result = null;
                var uploadTask = Task.Run(async () =>
                {
                    using (var uploader = new OneFichierUploader())
                    {
                        result = await uploader.UploadFileAsync(filePath, progress, statusProgress);
                    }
                });

                // Wait for upload or cancellation
                while (!uploadTask.IsCompleted && !progressWindow.WasCancelled)
                {
                    await Task.Delay(100);
                }

                // If cancelled, close window and return null
                if (progressWindow.WasCancelled)
                {
                    progressWindow.Close();
                    return null;
                }

                await uploadTask; // Ensure task completes

                if (result != null && !string.IsNullOrEmpty(result.DownloadUrl))
                {
                    System.Diagnostics.Debug.WriteLine($"[UPLOAD] 1fichier upload successful. Download URL: {result.DownloadUrl}");

                    // Close progress window
                    progressWindow.Close();

                    // Return the 1fichier URL - the calling code will show modal with conversion logic
                    return result.DownloadUrl;
                }
                else
                {
                    progressWindow.Close();
                    throw new Exception("Upload succeeded but no download URL was returned");
                }
            }
            catch (HttpRequestException httpEx)
            {
                progressWindow.Close();
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] HTTP Request Exception: {httpEx.Message}");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Inner Exception: {httpEx.InnerException?.Message}");
                throw new Exception($"Upload failed (HTTP): {httpEx.Message}. Inner: {httpEx.InnerException?.Message}");
            }
            catch (TaskCanceledException tcEx)
            {
                progressWindow.Close();
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Task Cancelled/Timeout: {tcEx.Message}");
                throw new Exception($"Upload timed out: {tcEx.Message}");
            }
            catch (Exception ex)
            {
                progressWindow.Close();
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] General Exception: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Stack: {ex.StackTrace}");
                throw new Exception($"Upload failed: {ex.Message}");
            }
        }

        private async Task<string> Convert1FichierLink(string oneFichierUrl, RGBProgressWindow progressWindow = null)
        {
            // Force HTTPS - AllDebrid requires it
            if (oneFichierUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                oneFichierUrl = "https://" + oneFichierUrl.Substring(7);
                System.Diagnostics.Debug.WriteLine($"[CONVERT] Converted HTTP to HTTPS: {oneFichierUrl}");
            }

            // 100 retries at 30 seconds each
            int maxRetries = 100;
            int retryDelay = 30000; // 30 seconds

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(30);

                        var requestBody = new
                        {
                            link = oneFichierUrl
                        };

                        var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                        System.Diagnostics.Debug.WriteLine($"[CONVERT] Converting 1fichier link (attempt {attempt}/{maxRetries}): {oneFichierUrl}");

                        if (progressWindow != null && progressWindow.IsHandleCreated)
                        {
                            progressWindow.Invoke(new Action(() =>
                            {
                                progressWindow.lblStatus.Text = $"Checking status...";
                            }));
                        }

                        var response = await client.PostAsync("https://pydrive.harryeffingpotter.com/convert-1fichier", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"[CONVERT] Response: {responseJson}");

                            // Parse the response to get the converted link
                            var jsonDoc = System.Text.Json.JsonDocument.Parse(responseJson);
                            if (jsonDoc.RootElement.TryGetProperty("link", out var linkProperty))
                            {
                                string convertedLink = linkProperty.GetString();
                                System.Diagnostics.Debug.WriteLine($"[CONVERT] Converted link: {convertedLink}");
                                return convertedLink;
                            }
                        }
                        else
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"[CONVERT] Failed with status: {response.StatusCode}, Content: {responseContent}");

                            // Check if it's a "still processing" error
                            if (responseContent.Contains("LINK_DOWN") || responseContent.Contains("wait"))
                            {
                                if (attempt < maxRetries)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[CONVERT] 1fichier still processing, waiting {retryDelay/1000}s before retry...");

                                    if (progressWindow != null && progressWindow.IsHandleCreated)
                                    {
                                        progressWindow.Invoke(new Action(() =>
                                        {
                                            progressWindow.lblStatus.Text = $"1fichier is still scanning the file, will retry again in {retryDelay/1000}s";
                                        }));
                                    }

                                    await Task.Delay(retryDelay);
                                    continue;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CONVERT] Exception (attempt {attempt}/{maxRetries}): {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        await Task.Delay(retryDelay);
                        continue;
                    }
                }

                // If we get here and haven't returned, retry unless it's the last attempt
                if (attempt < maxRetries)
                {
                    await Task.Delay(retryDelay);
                }
            }

            System.Diagnostics.Debug.WriteLine($"[CONVERT] All retry attempts exhausted, returning original link");
            // If conversion fails after all retries, return original link
            return oneFichierUrl;
        }

        private void GamesGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Format both button columns and text columns
            if (e.ColumnIndex >= 0)
            {
                var colName = gamesGrid.Columns[e.ColumnIndex].Name;
                var cell = gamesGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];

                // Style the Share columns
                if (colName == "ShareClean")
                {
                    // Green tint for clean - transparent background
                    cell.Style.BackColor = Color.FromArgb(8, 8, 12);
                    cell.Style.ForeColor = Color.FromArgb(100, 255, 150);
                    cell.Style.SelectionBackColor = Color.FromArgb(15, 25, 20);
                    cell.Style.SelectionForeColor = Color.FromArgb(150, 255, 200);
                }
                else if (colName == "ShareCracked")
                {
                    // Purple/magenta tint for cracked - transparent background
                    cell.Style.BackColor = Color.FromArgb(8, 8, 12);
                    cell.Style.ForeColor = Color.FromArgb(255, 100, 255);
                    cell.Style.SelectionBackColor = Color.FromArgb(20, 15, 25);
                    cell.Style.SelectionForeColor = Color.FromArgb(255, 150, 255);
                }
            }
        }

        private void EnhancedShareWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Main form stays visible
        }

        private void EnhancedShareWindow_Load(object sender, EventArgs e)
        {
            _ = LoadGames();
        }

        private async void ShowUploadSuccessWithConvert(string url, string gameName, bool cracked, bool isOneFichier, long fileSize)
        {
            // Auto-copy link to clipboard immediately
            Clipboard.SetText(url);

            var successForm = new Form
            {
                Text = "Upload Complete!",
                Size = new Size(500, 270),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(5, 8, 20),
                ForeColor = Color.White,
                ShowInTaskbar = false
            };

            // Apply acrylic effect and rounded corners
            successForm.Load += (s, e) =>
            {
                APPID.AcrylicHelper.ApplyAcrylic(successForm, roundedCorners: true);
            };

            // Match parent form's TopMost setting
            successForm.TopMost = this.TopMost;

            // Click off to close
            successForm.Deactivate += (s, e) => successForm.Close();

            string linkType = isOneFichier ? "1fichier" : "PyDrive";
            var lblSuccess = new Label
            {
                Text = $"✅ {gameName} ({(cracked ? "Cracked" : "Clean")}) uploaded successfully!\n\n{linkType} link copied to clipboard!",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 255, 150),
                Location = new Point(20, 20),
                Size = new Size(460, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var txtUrl = new TextBox
            {
                Text = url,
                Location = new Point(20, 90),
                Size = new Size(460, 30),
                ReadOnly = true,
                BackColor = Color.FromArgb(10, 15, 25),
                ForeColor = Color.Cyan,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                TextAlign = HorizontalAlignment.Center
            };

            // Countdown label for 1fichier links
            Label lblCountdown = null;
            Timer countdownTimer = null;
            int secondsRemaining = 0;
            bool isConverting = false;

            if (isOneFichier)
            {
                // Calculate initial wait time based on file size
                if (fileSize > 5L * 1024 * 1024 * 1024) // 5GB+
                {
                    long sizeInGB = fileSize / (1024 * 1024 * 1024);
                    secondsRemaining = (int)(sizeInGB * 12); // 12 seconds per GB
                    secondsRemaining = Math.Min(secondsRemaining, 1800); // Cap at 30 minutes
                    secondsRemaining = Math.Max(secondsRemaining, 30); // At least 30 seconds
                }
                else
                {
                    secondsRemaining = 3; // Small files
                }

                string reason = fileSize > 5L * 1024 * 1024 * 1024
                    ? $"Waiting for 1fichier to scan the {fileSize / (1024 * 1024 * 1024)}GB file... "
                    : "Waiting for 1fichier to process... ";

                lblCountdown = new Label
                {
                    Text = $"{reason}Auto-converting in {secondsRemaining}s",
                    Location = new Point(20, 125),
                    Size = new Size(460, 20),
                    ForeColor = Color.FromArgb(200, 200, 100),
                    Font = new Font("Segoe UI", 9),
                    TextAlign = ContentAlignment.MiddleCenter
                };
            }

            var btnCopy = new Button
            {
                Text = "📋 Copy Link",
                Location = new Point(isOneFichier ? 50 : 100, 160),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(0, 80, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCopy.FlatAppearance.BorderColor = Color.FromArgb(100, 200, 255);
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(txtUrl.Text);
                btnCopy.Text = "✓ Copied!";
                btnCopy.BackColor = Color.FromArgb(0, 100, 0);
            };

            // Conversion logic (shared between auto-convert and manual Convert button)
            Func<Task> attemptConversion = async () =>
            {
                if (isConverting) return;
                isConverting = true;

                countdownTimer?.Stop();
                if (lblCountdown != null) lblCountdown.Text = "Converting...";
                Button convertBtn = successForm.Controls.OfType<Button>().FirstOrDefault(b => b.Text.Contains("Convert") || b.Text.Contains("Converting"));
                if (convertBtn != null)
                {
                    convertBtn.Enabled = false;
                    convertBtn.Text = "Converting...";
                }

                try
                {
                    string convertedUrl = await Convert1FichierLink(url, null);

                    if (!string.IsNullOrEmpty(convertedUrl))
                    {
                        // Success! Update the modal
                        txtUrl.Text = convertedUrl;
                        Clipboard.SetText(convertedUrl);
                        lblSuccess.Text = $"✅ {gameName} ({(cracked ? "Cracked" : "Clean")}) uploaded successfully!\n\nPyDrive link copied to clipboard!";
                        if (lblCountdown != null) lblCountdown.Visible = false;
                        if (convertBtn != null) convertBtn.Visible = false;
                        var copyBtn = successForm.Controls.OfType<Button>().FirstOrDefault(b => b.Text.Contains("Copy"));
                        if (copyBtn != null)
                        {
                            copyBtn.Location = new Point(100, 160);
                            copyBtn.Size = new Size(140, 40);
                        }
                        var closeBtn = successForm.Controls.OfType<Button>().FirstOrDefault(b => b.Text.Contains("Close"));
                        if (closeBtn != null) closeBtn.Location = new Point(260, 160);
                    }
                    else
                    {
                        // Failed - setup retry
                        if (lblCountdown != null) lblCountdown.Text = "Trying again in 30s...";
                        if (convertBtn != null)
                        {
                            convertBtn.Text = "🔗 Convert";
                            convertBtn.Enabled = true;
                        }
                        secondsRemaining = 30;
                        countdownTimer?.Start();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CONVERT] Error: {ex.Message}");
                    // Failed - setup retry
                    if (lblCountdown != null) lblCountdown.Text = "Trying again in 30s...";
                    if (convertBtn != null)
                    {
                        convertBtn.Text = "🔗 Convert";
                        convertBtn.Enabled = true;
                    }
                    secondsRemaining = 30;
                    countdownTimer?.Start();
                }

                isConverting = false;
            };

            Button btnConvert = null;
            if (isOneFichier)
            {
                btnConvert = new Button
                {
                    Text = "🔗 Convert",
                    Location = new Point(190, 160),
                    Size = new Size(120, 40),
                    BackColor = Color.FromArgb(80, 40, 0),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btnConvert.FlatAppearance.BorderColor = Color.FromArgb(255, 150, 50);
                btnConvert.Click += async (s, e) => await attemptConversion();

                // Create countdown timer
                countdownTimer = new Timer { Interval = 1000 };
                countdownTimer.Tick += async (s, e) =>
                {
                    secondsRemaining--;
                    if (secondsRemaining > 0)
                    {
                        string reason = fileSize > 5L * 1024 * 1024 * 1024 && !lblCountdown.Text.Contains("Trying")
                            ? $"Waiting for 1fichier to scan the {fileSize / (1024 * 1024 * 1024)}GB file... "
                            : lblCountdown.Text.Contains("Trying") ? "" : "Waiting for 1fichier to process... ";
                        string action = lblCountdown.Text.Contains("Trying") ? "Trying again" : "Auto-converting";
                        lblCountdown.Text = $"{reason}{action} in {secondsRemaining}s";
                    }
                    else
                    {
                        await attemptConversion();
                    }
                };
                countdownTimer.Start();
            }

            var btnClose = new Button
            {
                Text = "✓ Close",
                Location = new Point(isOneFichier ? 330 : 260, 160),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(0, 100, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(100, 255, 150);
            btnClose.Click += (s, e) => {
                countdownTimer?.Stop();
                successForm.Close();
            };

            // Add controls to form
            if (btnConvert != null && lblCountdown != null)
                successForm.Controls.AddRange(new Control[] { lblSuccess, txtUrl, lblCountdown, btnCopy, btnConvert, btnClose });
            else
                successForm.Controls.AddRange(new Control[] { lblSuccess, txtUrl, btnCopy, btnClose });

            successForm.ShowDialog(this);
        }

        private void ShowUploadSuccess(string url, string gameName, bool cracked)
        {
            ShowUploadSuccessWithConvert(url, gameName, cracked, url.Contains("1fichier.com"), 0);
        }


        private List<SteamGame> ScanSteamLibraries()
        {
            var games = new List<SteamGame>();
            var paths = GetSteamLibraryPaths();

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    var manifests = Directory.GetFiles(path, "appmanifest_*.acf");
                    foreach (var manifest in manifests)
                    {
                        var game = ParseManifest(manifest);
                        if (game != null)
                        {
                            games.Add(game);
                        }
                    }
                }
            }

            return games;
        }

        private List<string> GetSteamLibraryPaths()
        {
            var paths = new HashSet<string>(); // Use HashSet to avoid duplicates

            // Get ALL drives on the system
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => d.RootDirectory.FullName);

            // Check common Steam locations on ALL drives
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
                    if (Directory.Exists(path))
                    {
                        paths.Add(path);

                        // Found a Steam install, check for libraryfolders.vdf
                        var vdfPath = Path.Combine(path, "libraryfolders.vdf");
                        if (File.Exists(vdfPath))
                        {
                            var vdfContent = File.ReadAllText(vdfPath);
                            var pathMatches = System.Text.RegularExpressions.Regex.Matches(vdfContent, @"""path""\s+""([^""]+)""");
                            foreach (System.Text.RegularExpressions.Match match in pathMatches)
                            {
                                var libPath = Path.Combine(match.Groups[1].Value.Replace(@"\\", @"\"), "steamapps");
                                if (Directory.Exists(libPath))
                                    paths.Add(libPath);
                            }
                        }
                    }
                }
            }

            return paths.ToList();
        }

        private SteamGame ParseManifest(string manifestPath)
        {
            try
            {
                var content = File.ReadAllText(manifestPath);

                var appIdMatch = System.Text.RegularExpressions.Regex.Match(content, @"""appid""\s+""(\d+)""");
                var nameMatch = System.Text.RegularExpressions.Regex.Match(content, @"""name""\s+""([^""]+)""");
                var buildIdMatch = System.Text.RegularExpressions.Regex.Match(content, @"""buildid""\s+""([^""]+)""");
                var installDirMatch = System.Text.RegularExpressions.Regex.Match(content, @"""installdir""\s+""([^""]+)""");
                var lastUpdatedMatch = System.Text.RegularExpressions.Regex.Match(content, @"""LastUpdated""\s+""(\d+)""");

                if (!appIdMatch.Success || !nameMatch.Success) return null;

                var steamPath = Path.GetDirectoryName(manifestPath);
                var installDir = installDirMatch.Success ?
                    Path.Combine(steamPath, "common", installDirMatch.Groups[1].Value) : "";

                long lastUpdated = 0;
                if (lastUpdatedMatch.Success)
                {
                    long.TryParse(lastUpdatedMatch.Groups[1].Value, out lastUpdated);
                }

                return new SteamGame
                {
                    AppId = appIdMatch.Groups[1].Value,
                    Name = nameMatch.Groups[1].Value,
                    BuildId = buildIdMatch.Success ? buildIdMatch.Groups[1].Value : "Unknown",
                    InstallDir = installDir,
                    LastUpdated = lastUpdated
                };
            }
            catch
            {
                return null;
            }
        }

        private class SteamGame
        {
            public string AppId { get; set; }
            public string Name { get; set; }
            public string InstallDir { get; set; }
            public string BuildId { get; set; }
            public long LastUpdated { get; set; }
        }

        private Point mouseDownPoint = Point.Empty;

        private void DataGrid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Get hit test info to check if we're on a column divider
                var hitTest = gamesGrid.HitTest(e.X, e.Y);

                // Don't drag if clicking on column header (for resizing/sorting)
                if (hitTest.Type == DataGridViewHitTestType.ColumnHeader)
                    return;

                // Start drag
                TitleBar_MouseDown(sender, e);
            }
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDownPoint = new Point(e.X, e.Y);
                this.Cursor = Cursors.SizeAll;

                // Handle mouse move
                var titleBarControl = sender as Control;
                titleBarControl.MouseMove += TitleBar_MouseMove;
                titleBarControl.MouseUp += TitleBar_MouseUp;
            }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && mouseDownPoint != Point.Empty)
            {
                this.Location = new Point(
                    this.Location.X + e.X - mouseDownPoint.X,
                    this.Location.Y + e.Y - mouseDownPoint.Y
                );
            }
        }

        private void TitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Default;
            mouseDownPoint = Point.Empty;

            var titleBarControl = sender as Control;
            titleBarControl.MouseMove -= TitleBar_MouseMove;
            titleBarControl.MouseUp -= TitleBar_MouseUp;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private async void BtnCustomPath_Click(object sender, EventArgs e)
        {
            // Create folder browser dialog
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder to scan for Steam games (will search for .acf files)";
                folderDialog.ShowNewFolderButton = false;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;

                    // Show progress
                    lblStatus.Text = "Scanning for games...";
                    lblStatus.Visible = true;
                    progressBar.Visible = true;
                    progressBar.Style = ProgressBarStyle.Marquee;

                    await Task.Run(() =>
                    {
                        // Search for .acf files recursively
                        var acfFiles = Directory.GetFiles(selectedPath, "appmanifest_*.acf", SearchOption.AllDirectories);

                        System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] Scanning: {selectedPath}");
                        System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] Found {acfFiles.Length} ACF files");

                        // Show visible status
                        this.Invoke(new Action(() =>
                        {
                            lblStatus.Text = $"Found {acfFiles.Length} ACF file(s) in {selectedPath}";
                        }));

                        if (acfFiles.Length > 0)
                        {
                            this.Invoke(new Action(() =>
                            {
                                lblStatus.Text = $"Found {acfFiles.Length} game manifest(s)";
                            }));

                            foreach (var acfFile in acfFiles)
                            {
                                try
                                {
                                    // Parse the ACF file
                                    string content = File.ReadAllText(acfFile);
                                    var manifest = ParseAcfFile(content);

                                    if (manifest.ContainsKey("appid") && manifest.ContainsKey("name") && manifest.ContainsKey("installdir"))
                                    {
                                        string appId = manifest["appid"];
                                        string gameName = manifest["name"];
                                        string installDir = manifest["installdir"];

                                        // ACF file directory (where the manifest is)
                                        string acfDirectory = Path.GetDirectoryName(acfFile);
                                        string gamePath = null;

                                        System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] ACF: {acfFile}");
                                        System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] InstallDir from ACF: {installDir}");
                                        System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] ACF Directory: {acfDirectory}");

                                        // Check 1: In common/ subfolder (standard Steam layout)
                                        string inCommon = Path.Combine(acfDirectory, "common", installDir);
                                        System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] Checking: {inCommon}");
                                        if (Directory.Exists(inCommon))
                                        {
                                            gamePath = inCommon;
                                            System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] ✓ Found in common/");
                                        }
                                        // Check 2: Next to ACF file (extracted/portable games)
                                        else
                                        {
                                            string nextToAcf = Path.Combine(acfDirectory, installDir);
                                            System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] Checking: {nextToAcf}");
                                            if (Directory.Exists(nextToAcf))
                                            {
                                                gamePath = nextToAcf;
                                                System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] ✓ Found next to ACF");
                                            }
                                            else
                                            {
                                                System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] ✗ Game folder not found!");
                                            }
                                        }

                                        if (gamePath != null)
                                        {
                                            // Add to grid if not already present
                                            this.Invoke(new Action(() =>
                                            {
                                                // Check if game already in grid
                                                bool exists = false;
                                                foreach (DataGridViewRow existingRow in gamesGrid.Rows)
                                                {
                                                    if (existingRow.Cells["AppID"].Value?.ToString() == appId)
                                                    {
                                                        exists = true;
                                                        break;
                                                    }
                                                }

                                                if (exists)
                                                {
                                                    System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] Skipping {gameName} - already in list");
                                                }

                                                if (!exists)
                                                {
                                                    var row = gamesGrid.Rows[gamesGrid.Rows.Add()];
                                                    row.Cells["GameName"].Value = gameName + " [Custom]";
                                                    row.Cells["AppID"].Value = appId;
                                                    row.Cells["InstallPath"].Value = gamePath;
                                                    row.Cells["BuildID"].Value = manifest.ContainsKey("buildid") ? manifest["buildid"] : "Unknown";

                                                    // Get size from ACF file directly
                                                    if (manifest.ContainsKey("SizeOnDisk"))
                                                    {
                                                        if (long.TryParse(manifest["SizeOnDisk"], out long sizeBytes))
                                                        {
                                                            row.Cells["GameSize"].Value = FormatSize(sizeBytes);
                                                            row.Cells["GameSize"].Tag = sizeBytes;
                                                        }
                                                        else
                                                        {
                                                            row.Cells["GameSize"].Value = "Unknown";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        row.Cells["GameSize"].Value = "Unknown";
                                                    }

                                                    // Set button values
                                                    row.Cells["CrackOnly"].Value = "⚡ Crack";
                                                    row.Cells["ShareClean"].Value = "📦 Clean";
                                                    row.Cells["ShareCracked"].Value = "🎮 Cracked";

                                                    // Mark as custom path game with different color
                                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 200, 255);
                                                }
                                            }));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[CUSTOM PATH] Error parsing {acfFile}: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            // No ACF files found - scan for game folders anyway
                            this.Invoke(new Action(() =>
                            {
                                lblStatus.Text = "No Steam manifests found, scanning for game folders...";
                            }));

                            // Look for common game indicators (exe files, steam_api.dll, etc)
                            var exeFiles = Directory.GetFiles(selectedPath, "*.exe", SearchOption.AllDirectories);
                            var potentialGames = new HashSet<string>();

                            foreach (var exe in exeFiles)
                            {
                                var dir = Path.GetDirectoryName(exe);
                                // Check if this directory has steam_api.dll or steam_api64.dll
                                if (File.Exists(Path.Combine(dir, "steam_api.dll")) ||
                                    File.Exists(Path.Combine(dir, "steam_api64.dll")))
                                {
                                    potentialGames.Add(dir);
                                }
                            }

                            foreach (var gameDir in potentialGames)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    var gameName = Path.GetFileName(gameDir);
                                    var row = gamesGrid.Rows[gamesGrid.Rows.Add()];
                                    row.Cells["GameName"].Value = gameName + " [Unknown]";
                                    row.Cells["AppID"].Value = "Manual";
                                    row.Cells["InstallPath"].Value = gameDir;
                                    row.Cells["BuildID"].Value = "Unknown";
                                    row.Cells["GameSize"].Value = "...";

                                    // Set button values - only crack available for unknown games
                                    row.Cells["CrackOnly"].Value = "⚡ Crack";
                                    row.Cells["ShareClean"].Value = "❌ No ID";
                                    row.Cells["ShareCracked"].Value = "❌ No ID";

                                    // Mark as unknown game with different color
                                    row.DefaultCellStyle.ForeColor = Color.FromArgb(255, 200, 100);

                                    // Calculate size since we don't have ACF
                                    int rowIndex = row.Index;
                                    string gamePath = gameDir;
                                    _ = Task.Run(() =>
                                    {
                                        long size = GetDirectorySize(gamePath);
                                        this.Invoke(new Action(() =>
                                        {
                                            if (rowIndex < gamesGrid.Rows.Count)
                                            {
                                                gamesGrid.Rows[rowIndex].Cells["GameSize"].Value = FormatSize(size);
                                                gamesGrid.Rows[rowIndex].Cells["GameSize"].Tag = size;
                                            }
                                        }));
                                    });
                                }));
                            }
                        }
                    });

                    // Hide progress
                    lblStatus.Visible = false;
                    progressBar.Visible = false;
                    progressBar.Style = ProgressBarStyle.Blocks;
                }
            }
        }

        // Helper method to parse ACF files (if not in SteamManifestParser)
        private static Dictionary<string, string> ParseAcfFile(string content)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var keyValuePattern = @"""(\w+)""\s+""([^""]*)""";
            var matches = System.Text.RegularExpressions.Regex.Matches(content, keyValuePattern);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Groups.Count == 3)
                {
                    string key = match.Groups[1].Value;
                    string value = match.Groups[2].Value;
                    if (!result.ContainsKey(key))
                    {
                        result[key] = value;
                    }
                }
            }
            return result;
        }

        private List<(string depotId, string manifestId)> ParseInstalledDepots(string acfContent)
        {
            var depots = new List<(string, string)>();

            var depotPattern = @"""InstalledDepots"".*?\{(.*?)\n\t\}";
            var depotMatch = System.Text.RegularExpressions.Regex.Match(acfContent, depotPattern, System.Text.RegularExpressions.RegexOptions.Singleline);

            if (depotMatch.Success)
            {
                var depotSection = depotMatch.Groups[1].Value;
                var depotIdPattern = @"""(\d+)""\s*\{";
                var manifestPattern = @"""manifest""\s+""(\d+)""";

                var depotIds = System.Text.RegularExpressions.Regex.Matches(depotSection, depotIdPattern);
                var manifestIds = System.Text.RegularExpressions.Regex.Matches(depotSection, manifestPattern);

                for (int i = 0; i < Math.Min(depotIds.Count, manifestIds.Count); i++)
                {
                    depots.Add((depotIds[i].Groups[1].Value, manifestIds[i].Groups[1].Value));
                }
            }

            return depots;
        }

        private List<string> FindDepotManifests(string appId, List<(string depotId, string manifestId)> depots)
        {
            var manifestFiles = new List<string>();

            var steamPaths = GetSteamLibraryPaths();

            foreach (var steamPath in steamPaths)
            {
                var depotcachePath = Path.Combine(Path.GetDirectoryName(steamPath), "depotcache");
                if (Directory.Exists(depotcachePath))
                {
                    foreach (var (depotId, manifestId) in depots)
                    {
                        var manifestFile = Path.Combine(depotcachePath, $"{depotId}_{manifestId}.manifest");
                        if (File.Exists(manifestFile))
                        {
                            manifestFiles.Add(manifestFile);
                            System.Diagnostics.Debug.WriteLine($"[DEPOT] Found manifest: {manifestFile}");
                        }
                    }
                }
            }

            return manifestFiles;
        }

        // Helper method to get directory size
        private long GetDirectorySize(string path)
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

        // Helper method to format file size
        private string FormatSize(long bytes)
        {
            if (bytes >= 1073741824)
                return $"{bytes / 1073741824.0:F1} GB";
            else if (bytes >= 1048576)
                return $"{bytes / 1048576.0:F1} MB";
            else if (bytes >= 1024)
                return $"{bytes / 1024.0:F1} KB";
            else
                return $"{bytes} B";
        }

        private void gamesGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void GamesGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var colName = gamesGrid.Columns[e.ColumnIndex].Name;

                // Set cursor for clickable cells
                if (colName == "ShareClean" || colName == "ShareCracked" || colName == "CrackOnly")
                {
                    gamesGrid.Cursor = Cursors.Hand;
                }
            }
            // Show tooltips only on column headers (row index -1)
            else if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                var colName = gamesGrid.Columns[e.ColumnIndex].Name;
                string tooltipText = "";
                switch (colName)
                {
                    case "GameName":
                        tooltipText = "Game name";
                        break;
                    case "InstallPath":
                        tooltipText = "Full path where the game is installed";
                        break;
                    case "GameSize":
                        tooltipText = "Total size of the game installation";
                        break;
                    case "BuildID":
                        tooltipText = "Steam build ID version";
                        break;
                    case "AppID":
                        tooltipText = "Steam Application ID";
                        break;
                    case "CrackOnly":
                        tooltipText = "Click to crack this game (generates crack files only)";
                        break;
                    case "ShareClean":
                        tooltipText = "Click to share the clean game (original Steam version)";
                        break;
                    case "ShareCracked":
                        tooltipText = "Click to share the cracked game (includes emulator)";
                        break;
                }

                if (!string.IsNullOrEmpty(tooltipText))
                {
                    toolTip.SetToolTip(gamesGrid, tooltipText);
                }
            }
        }

        private void GamesGrid_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            gamesGrid.Cursor = Cursors.Default;
        }

        private Dictionary<string, string> LoadSharedGamesData()
        {
            var result = new Dictionary<string, string>();
            try
            {
                string data = APPID.AppSettings.Default.SharedGamesData;
                if (!string.IsNullOrEmpty(data))
                {
                    // Format: appid_buildid:type,type|appid_buildid:type,type|...
                    var entries = data.Split('|');
                    foreach (var entry in entries)
                    {
                        if (string.IsNullOrEmpty(entry)) continue;
                        var parts = entry.Split(':');
                        if (parts.Length == 2)
                        {
                            result[parts[0]] = parts[1];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SHARED GAMES] Error loading: {ex.Message}");
            }
            return result;
        }

        private void SaveSharedGame(string appId, string buildId, string shareType)
        {
            try
            {
                var sharedGames = LoadSharedGamesData();
                string key = $"{appId}_{buildId}";

                // Add or update the share type
                if (sharedGames.ContainsKey(key))
                {
                    var types = sharedGames[key].Split(',').ToList();
                    if (!types.Contains(shareType))
                    {
                        types.Add(shareType);
                        sharedGames[key] = string.Join(",", types);
                    }
                }
                else
                {
                    sharedGames[key] = shareType;
                }

                // Save back to settings
                var entries = sharedGames.Select(kvp => $"{kvp.Key}:{kvp.Value}");
                APPID.AppSettings.Default.SharedGamesData = string.Join("|", entries);
                APPID.AppSettings.Default.Save();

                System.Diagnostics.Debug.WriteLine($"[SHARED GAMES] Saved {appId} build {buildId} as {shareType}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SHARED GAMES] Error saving: {ex.Message}");
            }
        }
    }

    // Modern Progress Bar with cool blue gradient
    public class ModernProgressBar : ProgressBar
    {
        public ModernProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw dark background
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(15, 15, 15)), e.ClipRectangle);

            // Calculate progress bar fill width
            int progressWidth = (int)(e.ClipRectangle.Width * ((double)Value / Maximum));

            if (progressWidth > 0)
            {
                // Create gradient brush for cool blue effect (like the slider)
                var progressRect = new Rectangle(0, 0, progressWidth, e.ClipRectangle.Height);
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    progressRect,
                    Color.FromArgb(60, 140, 230),   // Light blue
                    Color.FromArgb(30, 100, 200),   // Darker blue
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, progressRect);
                }

                // Add subtle highlight on top for depth
                var highlightRect = new Rectangle(0, 0, progressWidth, e.ClipRectangle.Height / 3);
                using (var highlightBrush = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                {
                    e.Graphics.FillRectangle(highlightBrush, highlightRect);
                }
            }
        }
    }

    // RGB Progress Window
    public class RGBProgressWindow : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        internal ProgressBar progressBar;
        internal Label lblStatus;
        private Timer rgbTimer;
        private int colorStep = 0;
        private Button btnCancel;
        public bool WasCancelled { get; private set; } = false;
        private Label lblScrollingInfo;
        private Timer scrollTimer;
        private int scrollOffset = 0;
        private string scrollText = "";
        private DateTime countdownStartTime;
        private int totalCountdownMinutes = 0;
        private long currentFileSize = 0;
        public string OneFichierUrl { get; set; }
        public string GameName { get; private set; }

        public RGBProgressWindow(string gameName, string type)
        {
            GameName = gameName;
            InitializeWindow(gameName, type);

            // Enable dragging the form
            this.MouseDown += RGBProgressWindow_MouseDown;
        }

        public void CenterOverParent(Form parent)
        {
            if (parent != null && parent.IsHandleCreated)
            {
                // Calculate center position
                int x = parent.Left + (parent.Width - this.Width) / 2;
                int y = parent.Top + (parent.Height - this.Height) / 2;
                this.Location = new Point(x, y);
            }
        }

        private void InitializeWindow(string gameName, string type)
        {
            this.Text = $"Sharing {gameName} ({type})";
            this.Size = new Size(500, 260);  // Increased height for scrolling text
            this.StartPosition = FormStartPosition.Manual;  // We'll set position manually
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(5, 8, 20);
            this.ShowInTaskbar = false;  // Don't show in taskbar

            // Apply rounded corners when form loads
            this.Load += (s, e) =>
            {
                try
                {
                    int preference = APPID.NativeMethods.DWMWCP_ROUND;
                    APPID.NativeMethods.DwmSetWindowAttribute(this.Handle, APPID.NativeMethods.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
                }
                catch { }
            };

            // Scrolling info label - hidden by default
            lblScrollingInfo = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(120, 192, 255),  // Soft cyan
                Location = new Point(25, 25),
                Size = new Size(450, 20),
                AutoSize = false,
                Visible = false
            };

            lblStatus = new Label
            {
                Text = "Preparing...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Cyan,  // Start with a color, will be animated
                Location = new Point(25, 70),
                Size = new Size(450, 30)
            };

            progressBar = new ModernProgressBar
            {
                Location = new Point(25, 110),
                Size = new Size(450, 30),
                Style = ProgressBarStyle.Continuous,
                BackColor = Color.FromArgb(15, 15, 15),
                Value = 0,
                Minimum = 0,
                Maximum = 100
            };

            btnCancel = new Button
            {
                Text = "✖ Cancel",
                Location = new Point(200, 165),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Standard,
                Font = new Font("Segoe UI", 9)
            };
            btnCancel.Click += (s, e) =>
            {
                WasCancelled = true;
                lblStatus.Text = "Cancelled by user";
                lblStatus.ForeColor = Color.Orange;
                btnCancel.Enabled = false;
                rgbTimer?.Stop();
                scrollTimer?.Stop();
                this.Close();
            };

            this.Controls.AddRange(new Control[] { lblScrollingInfo, lblStatus, progressBar, btnCancel });

            // RGB effect
            SetupRGBEffect();

            // Apply acrylic on load
            this.Load += (s, e) => ApplyAcrylicToProgressWindow();
        }

        private void ApplyAcrylicToProgressWindow()
        {
            APPID.AcrylicHelper.ApplyAcrylic(this, roundedCorners: false);
        }

        private void SetupRGBEffect()
        {
            rgbTimer = new Timer { Interval = 50 };
            rgbTimer.Tick += (s, e) =>
            {
                colorStep = (colorStep + 5) % 360;
                var color = HSLToRGB(colorStep, 1.0, 0.72);  // Bright vibrant colors
                // Apply RGB effect to the text
                lblStatus.ForeColor = color;

                // Apply RGB to scrolling info text too
                if (lblScrollingInfo != null && lblScrollingInfo.Visible)
                {
                    lblScrollingInfo.ForeColor = color;
                }

                // Try to color the progress bar (might not work on all Windows versions)
                try
                {
                    progressBar.ForeColor = color;
                    // For Windows 10+, we can try to use visual styles
                    progressBar.Style = ProgressBarStyle.Continuous;
                }
                catch { }
            };
            rgbTimer.Start();
        }

        private Color HSLToRGB(double h, double s, double l)
        {
            double r, g, b;
            h = h / 360.0;

            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;
                r = HueToRGB(p, q, h + 1.0 / 3);
                g = HueToRGB(p, q, h);
                b = HueToRGB(p, q, h - 1.0 / 3);
            }

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        private double HueToRGB(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }

        public void UpdateStatus(string status)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    lblStatus.Text = status;
                    progressBar.Value = Math.Min(progressBar.Value + 20, 100);
                }));
            }
        }

        public void SetProgress(int percentage, string status)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    lblStatus.Text = status;
                    progressBar.Value = Math.Max(0, Math.Min(100, percentage));
                }));
            }
        }

        public void Complete(string url)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    lblStatus.Text = "✅ Complete! Upload URL copied to clipboard.";
                    lblStatus.ForeColor = Color.Lime;
                    progressBar.Value = 100;
                    Clipboard.SetText(url);
                    rgbTimer.Stop();
                    this.BackColor = Color.FromArgb(0, 50, 0);
                }));
            }
        }

        public void ShowLargeFileWarning(long fileSizeBytes, int estimatedMinutes)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    // Set up countdown
                    countdownStartTime = DateTime.Now;
                    totalCountdownMinutes = estimatedMinutes;
                    currentFileSize = fileSizeBytes;

                    // Update scrolling text
                    UpdateCountdownText();
                    lblScrollingInfo.Visible = true;

                    // Start scrolling animation with countdown updates
                    if (scrollTimer == null)
                    {
                        scrollTimer = new Timer();
                        scrollTimer.Interval = 1000; // Update every second for countdown
                        scrollTimer.Tick += (s, e) =>
                        {
                            UpdateCountdownText();

                            // Do scrolling effect
                            if (!string.IsNullOrEmpty(scrollText))
                            {
                                scrollOffset = (scrollOffset + 2) % scrollText.Length;
                                string displayText = scrollText.Substring(scrollOffset) + scrollText.Substring(0, scrollOffset);
                                lblScrollingInfo.Text = displayText.Substring(0, Math.Min(displayText.Length, 60)); // Show 60 chars
                            }
                        };
                    }
                    scrollTimer.Start();
                }));
            }
        }

        private void UpdateCountdownText()
        {
            var elapsed = DateTime.Now - countdownStartTime;
            var remaining = totalCountdownMinutes - (int)elapsed.TotalMinutes;
            if (remaining < 1) remaining = 1; // Always show at least 1 minute

            string minuteText = remaining == 1 ? "minute" : "minutes";

            // Only show compression tip for files over 10GB
            string compressionTip = "";
            if (currentFileSize > 10L * 1024 * 1024 * 1024) // 10GB
            {
                compressionTip = "     💡 Tip: 7z ultra compression can make processing up to 40% faster!";
            }

            scrollText = $"     Waiting for 1fichier to scan the file...     This will take roughly {remaining} more {minuteText}...{compressionTip}     Cancel anytime to get the 1fichier link...     ";
        }

        public void ShowError(string error)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    lblStatus.Text = error;
                    lblStatus.ForeColor = Color.Red;
                    rgbTimer.Stop();
                    this.BackColor = Color.FromArgb(50, 0, 0);
                }));
            }
        }

        private void RGBProgressWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
    }
}