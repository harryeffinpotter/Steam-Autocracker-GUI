using APPID;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    public partial class EnhancedShareWindow : Form
    {
        private DataGridView gamesGrid;
        private Dictionary<string, RequestAPI.RequestedGame> requestedGames;
        private Form mainForm;
        private System.Windows.Forms.Timer pulseTimer;
        private List<DataGridViewRow> urgentRows = new List<DataGridViewRow>();

        public EnhancedShareWindow(Form parent)
        {
            mainForm = parent;
            requestedGames = new Dictionary<string, RequestAPI.RequestedGame>();
            InitializeComponent();  // Use the Designer.cs file instead!
            SetupColumns();         // Setup columns after InitializeComponent

            // Hide main form and show again when this closes
            mainForm.Visible = false;
            this.FormClosed += (s, e) => mainForm.Visible = true;
        }

        private void InitializeWindow_OLD()  // Renamed - not used anymore since we use Designer.cs
        {
            this.Text = "Share Your Games - Community Needs You!";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.ForeColor = Color.White;

            // Hide main form
            mainForm.Visible = false;
            this.FormClosed += (s, e) => mainForm.Visible = true;

            // Create main panel
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Header with request count
            var headerPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            var lblHeader = new Label
            {
                Text = "Loading requests...",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(10, 10),
                Size = new Size(500, 30),
                Name = "lblHeader"
            };
            headerPanel.Controls.Add(lblHeader);

            // Create grid
            gamesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(10, 10, 10),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(40, 40, 50),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };

            // Style
            gamesGrid.DefaultCellStyle.BackColor = Color.FromArgb(15, 15, 15);
            gamesGrid.DefaultCellStyle.ForeColor = Color.White;
            gamesGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 50, 70);
            gamesGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 25, 25);
            gamesGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            SetupColumns();

            mainPanel.Controls.Add(gamesGrid);
            this.Controls.Add(mainPanel);
            this.Controls.Add(headerPanel);

            // Setup pulse effect for urgent games
            SetupPulseEffect();

            // Load games and requests
            _ = LoadGamesAndRequests();
        }

        private void SetupColumns()
        {
            // Game name
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GameName",
                HeaderText = "Game",
                Width = 250,
                ReadOnly = true
            });

            // Build ID
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BuildID",
                HeaderText = "Your Build",
                Width = 100,
                ReadOnly = true
            });

            // Request status
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RequestStatus",
                HeaderText = "Community Needs",
                Width = 150,
                ReadOnly = true
            });

            // Request type
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RequestType",
                HeaderText = "Type Needed",
                Width = 100,
                ReadOnly = true
            });

            // Clean share button
            var cleanBtn = new DataGridViewButtonColumn
            {
                Name = "ShareClean",
                HeaderText = "Share Clean",
                Text = "📦 Clean",
                UseColumnTextForButtonValue = false,
                Width = 100
            };
            gamesGrid.Columns.Add(cleanBtn);

            // Cracked share button
            var crackedBtn = new DataGridViewButtonColumn
            {
                Name = "ShareCracked",
                HeaderText = "Share Cracked",
                Text = "🎮 Cracked",
                UseColumnTextForButtonValue = false,
                Width = 100
            };
            gamesGrid.Columns.Add(crackedBtn);

            // No request button - this is for sharing YOUR games
            // Remove any Request column if it exists (cleanup from old version)
            if (gamesGrid.Columns["Request"] != null)
            {
                gamesGrid.Columns.Remove("Request");
            }

            // Hidden columns
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "AppID", Visible = false });
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InstallPath", Visible = false });

            // Event handlers are already added in Designer.cs

            // Additional cell formatting for button styling
            gamesGrid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && gamesGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
                {
                    var cell = gamesGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    cell.Style.BackColor = Color.FromArgb(30, 30, 30);
                    cell.Style.ForeColor = Color.FromArgb(150, 150, 150);
                    cell.Style.SelectionBackColor = Color.FromArgb(40, 40, 40);
                }
            };
        }

        private async Task LoadGamesAndRequests()
        {
            // Get active requests first
            var requests = await RequestAPI.GetActiveRequests();
            foreach (var req in requests)
            {
                requestedGames[req.AppId] = req;
            }

            // Update header
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    var lblHeader = this.Controls.Find("lblHeader", true).FirstOrDefault() as Label;
                    if (lblHeader != null)
                    {
                        int urgentCount = requests.Count(r => r.RequestCount > 5);
                        if (urgentCount > 0)
                        {
                            lblHeader.Text = $"🚨 {urgentCount} GAMES URGENTLY NEEDED BY COMMUNITY!";
                            lblHeader.ForeColor = Color.FromArgb(255, 100, 100);
                        }
                        else if (requests.Count > 0)
                        {
                            lblHeader.Text = $"🔥 {requests.Count} games requested by community";
                            lblHeader.ForeColor = Color.FromArgb(255, 200, 100);
                        }
                        else
                        {
                            lblHeader.Text = "Your Steam Library";
                            lblHeader.ForeColor = Color.FromArgb(100, 200, 255);
                        }
                    }
                }));
            }

            // Scan Steam libraries
            var games = ScanSteamLibraries();

            // Add to grid with request highlighting
            foreach (var game in games)
            {
                var row = gamesGrid.Rows[gamesGrid.Rows.Add()];
                row.Cells["GameName"].Value = game.Name;
                row.Cells["BuildID"].Value = game.BuildId;
                row.Cells["AppID"].Value = game.AppId;
                row.Cells["InstallPath"].Value = game.InstallDir;

                // Default button texts
                row.Cells["ShareClean"].Value = "📦 Clean";
                row.Cells["ShareCracked"].Value = "🎮 Cracked";

                // Check if requested
                if (requestedGames.ContainsKey(game.AppId))
                {
                    var req = requestedGames[game.AppId];
                    ApplyRequestStyling(row, req);
                }
            }
        }

        private void ApplyRequestStyling(DataGridViewRow row, RequestAPI.RequestedGame req)
        {
            // Update request status
            if (req.RequestCount > 10)
            {
                row.Cells["RequestStatus"].Value = $"🚨 {req.RequestCount} REQUESTS!";
                row.Cells["RequestStatus"].Style.ForeColor = Color.Red;
                row.Cells["RequestStatus"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                row.DefaultCellStyle.BackColor = Color.FromArgb(40, 0, 0);
                urgentRows.Add(row);
            }
            else if (req.RequestCount > 5)
            {
                row.Cells["RequestStatus"].Value = $"🔥 {req.RequestCount} requests";
                row.Cells["RequestStatus"].Style.ForeColor = Color.FromArgb(255, 150, 0);
                row.Cells["RequestStatus"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                row.DefaultCellStyle.BackColor = Color.FromArgb(40, 20, 0);
            }
            else
            {
                row.Cells["RequestStatus"].Value = $"{req.RequestCount} requests";
                row.Cells["RequestStatus"].Style.ForeColor = Color.FromArgb(255, 255, 100);
            }

            // Show what type is needed
            row.Cells["RequestType"].Value = req.RequestType;

            // Color the appropriate share button
            if (req.RequestType == "Clean" || req.RequestType == "Both")
            {
                row.Cells["ShareClean"].Value = "📦 NEEDED!";
                row.Cells["ShareClean"].Style.BackColor = Color.FromArgb(255, 20, 147); // Hot pink
                row.Cells["ShareClean"].Style.ForeColor = Color.White;
                row.Cells["ShareClean"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }

            if (req.RequestType == "Cracked" || req.RequestType == "Both")
            {
                row.Cells["ShareCracked"].Value = "🎮 NEEDED!";
                row.Cells["ShareCracked"].Style.BackColor = Color.FromArgb(255, 20, 147); // Hot pink
                row.Cells["ShareCracked"].Style.ForeColor = Color.White;
                row.Cells["ShareCracked"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }
        }

        private void SetupPulseEffect()
        {
            pulseTimer = new System.Windows.Forms.Timer { Interval = 100 };
            int pulseStep = 0;

            pulseTimer.Tick += (s, e) =>
            {
                pulseStep = (pulseStep + 1) % 20;
                double pulse = Math.Sin(pulseStep * Math.PI / 10) * 0.3 + 0.7;

                foreach (var row in urgentRows)
                {
                    if (!row.IsNewRow && row.Visible)
                    {
                        int r = (int)(40 * pulse);
                        row.DefaultCellStyle.BackColor = Color.FromArgb(r, 0, 0);
                    }
                }
            };

            pulseTimer.Start();
        }

        private async void GamesGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = gamesGrid.Rows[e.RowIndex];
            var gameName = row.Cells["GameName"].Value?.ToString();
            var appId = row.Cells["AppID"].Value?.ToString();
            var installPath = row.Cells["InstallPath"].Value?.ToString();

            // No request button anymore - this window is for sharing YOUR games

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

        private async Task ShareGame(string gameName, string installPath, string appId, bool cracked, DataGridViewRow row)
        {
            if (string.IsNullOrEmpty(installPath) || !Directory.Exists(installPath))
            {
                MessageBox.Show("Game installation path not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Show the compression settings dialog
                using (var compressionForm = new SteamAutocrackGUI.CompressionSettingsForm())
                {
                    compressionForm.StartPosition = FormStartPosition.CenterParent;
                    compressionForm.TopMost = true;

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

                    // Build output path
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string prefix = cracked ? "[SACGUI] CRACKED" : "[SACGUI] CLEAN";
                    string zipName = $"{prefix} {gameName}.{format.ToLower()}";
                    string outputPath = Path.Combine(desktopPath, zipName);

                    // Show progress window
                    var progressForm = new RGBProgressWindow(gameName, cracked ? "Cracked" : "Clean");
                    progressForm.Show(this);
                    progressForm.UpdateStatus($"Compressing with {format.ToUpper()} level {level}...");

                    // Compress the game using the real compression (not placeholder!)
                    bool compressionSuccess = await Task.Run(() => CompressGameProper(installPath, outputPath, format, level));

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
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        BackColor = Color.FromArgb(20, 20, 30)
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
                        BackColor = Color.FromArgb(0, 100, 0),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        DialogResult = DialogResult.Yes
                    };

                    var explorerBtn = new Button
                    {
                        Text = "📁 Show in Explorer",
                        Size = new Size(150, 40),
                        Location = new Point(210, 100),
                        BackColor = Color.FromArgb(50, 50, 70),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        DialogResult = DialogResult.No
                    };

                    completionDialog.Controls.Add(label);
                    completionDialog.Controls.Add(uploadBtn);
                    completionDialog.Controls.Add(explorerBtn);

                    var result = completionDialog.ShowDialog(this);

                    if (result == DialogResult.Yes)
                    {
                        // User chose to upload
                        row.Cells[btnColumn].Value = "⏳ Uploading...";

                        string uploadUrl = await UploadFileWithProgress(outputPath, new RGBProgressWindow(gameName, "Uploading"));

                        if (!string.IsNullOrEmpty(uploadUrl))
                        {
                            row.Cells[btnColumn].Value = "✅ Shared!";
                            row.Cells[btnColumn].Style.BackColor = Color.FromArgb(0, 100, 0);

                            // Notify backend about completion
                            long fileSize = new FileInfo(outputPath).Length;
                            string uploadType = cracked ? "cracked" : "clean";
                            await NotifyBackendCompletion(appId, uploadType, uploadUrl, fileSize);

                            ShowUploadSuccess(uploadUrl, gameName, cracked);
                        }
                        else
                        {
                            row.Cells[btnColumn].Value = "❌ Upload Failed";
                        }
                    }
                    else
                    {
                        // User chose to show in explorer
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{outputPath}\"");
                        row.Cells[btnColumn].Value = "💾 Saved";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                row.Cells[cracked ? "ShareCracked" : "ShareClean"].Value = cracked ? "🎮 Cracked" : "📦 Clean";
            }
        }

        private bool CompressGameProper(string sourcePath, string outputPath, string format, string level)
        {
            try
            {
                // Try to use 7-Zip first for better compression
                string sevenZipPath = @"C:\Program Files\7-Zip\7z.exe";
                if (!File.Exists(sevenZipPath))
                {
                    sevenZipPath = @"C:\Program Files (x86)\7-Zip\7z.exe";
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
                        compressionSwitch = "-mx9";  // Ultra

                    string archiveType = format.ToLower() == "7z" ? "7z" : "zip";
                    string arguments = $"a -t{archiveType} {compressionSwitch} \"{outputPath}\" \"{sourcePath}\\*\" -r";

                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = sevenZipPath,
                        Arguments = arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (var process = System.Diagnostics.Process.Start(psi))
                    {
                        process.WaitForExit();
                        return process.ExitCode == 0;
                    }
                }
                else if (format.ToLower() == "zip")
                {
                    // Fallback to built-in ZIP compression
                    var compressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                    int levelNum = int.Parse(level ?? "5");

                    if (levelNum == 0)
                        compressionLevel = System.IO.Compression.CompressionLevel.NoCompression;
                    else if (levelNum <= 5)
                        compressionLevel = System.IO.Compression.CompressionLevel.Fastest;
                    else
                        compressionLevel = System.IO.Compression.CompressionLevel.Optimal;

                    System.IO.Compression.ZipFile.CreateFromDirectory(sourcePath, outputPath, compressionLevel, false);
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

        private async Task CompressAndUploadFallback(string installPath, string outputPath, string gameName, string format, string level, bool cracked, string appId, DataGridViewRow row)
        {
            var progressForm = new RGBProgressWindow(gameName, cracked ? "Cracked" : "Clean");
            progressForm.Show(this);

            try
            {
                // Use the existing compression method but with proper parameters
                progressForm.UpdateStatus("Compressing files...");
                string compressedPath = await CompressGameWithProgress(installPath, gameName, cracked, progressForm);

                // Upload the file
                progressForm.UpdateStatus("Uploading to server...");
                string uploadUrl = await UploadFileWithProgress(compressedPath, progressForm);

                // Get file size
                long fileSize = new FileInfo(compressedPath).Length;

                // Notify backend
                progressForm.UpdateStatus("Notifying community...");
                string uploadType = cracked ? "cracked" : "clean";
                await NotifyBackendCompletion(appId, uploadType, uploadUrl, fileSize);

                // Update UI
                var btnColumn = cracked ? "ShareCracked" : "ShareClean";
                row.Cells[btnColumn].Value = "✅ Shared!";
                row.Cells[btnColumn].Style.BackColor = Color.FromArgb(0, 100, 0);

                progressForm.Complete(uploadUrl);
                ShowUploadSuccess(uploadUrl, gameName, cracked);

                // Check if this fulfilled a request
                if (requestedGames.ContainsKey(appId))
                {
                    var req = requestedGames[appId];
                    if ((cracked && (req.RequestType == "Cracked" || req.RequestType == "Both")) ||
                        (!cracked && (req.RequestType == "Clean" || req.RequestType == "Both")))
                    {
                        ShowHonorNotification(gameName);
                    }
                }
            }
            catch (Exception ex)
            {
                progressForm.ShowError($"Error: {ex.Message}");
                row.Cells[cracked ? "ShareCracked" : "ShareClean"].Value = cracked ? "🎮 Cracked" : "📦 Clean";
            }
        }

        private async Task<string> CompressGameWithProgress(string sourcePath, string gameName, bool cracked, RGBProgressWindow progressWindow)
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "SteamAutoCracker");
                Directory.CreateDirectory(tempPath);

                string cleanName = string.Join("_", gameName.Split(Path.GetInvalidFileNameChars()));
                string zipFileName = $"[SACGUI] {(cracked ? "CRACKED" : "CLEAN")} {cleanName}.zip";
                string outputPath = Path.Combine(tempPath, zipFileName);

                // Delete existing file if it exists
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                progressWindow.UpdateStatus($"Compressing {gameName}...");

                await Task.Run(() =>
                {
                    using (var archive = System.IO.Compression.ZipFile.Open(outputPath, System.IO.Compression.ZipArchiveMode.Create))
                    {
                        // Add all files from the game directory
                        var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
                        int totalFiles = files.Length;
                        int currentFile = 0;

                        foreach (string file in files)
                        {
                            currentFile++;
                            // .NET Framework compatible way to get relative path
                            string relativePath = file.Substring(sourcePath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                            string entryName = Path.Combine(cleanName, relativePath).Replace('\\', '/');

                            archive.CreateEntryFromFile(file, entryName);

                            int percentage = (currentFile * 100) / totalFiles;
                            if (progressWindow.IsHandleCreated)
                            {
                                progressWindow.Invoke(new Action(() =>
                                {
                                    progressWindow.UpdateStatus($"Compressing... {percentage}% ({currentFile}/{totalFiles} files)");
                                }));
                            }
                        }
                    }
                });

                return outputPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Compression failed: {ex.Message}");
            }
        }

        private async Task<string> UploadFileWithProgress(string filePath, RGBProgressWindow progressWindow)
        {
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] === Starting upload from EnhancedShareWindow ===");
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] File: {filePath}");
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] File exists: {File.Exists(filePath)}");
            if (File.Exists(filePath))
            {
                var fi = new FileInfo(filePath);
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] File size: {fi.Length / (1024.0 * 1024.0):F2} MB");
            }

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    // Configure client for large uploads
                    client.Timeout = TimeSpan.FromHours(2);
                    client.DefaultRequestHeaders.Add("User-Agent", "SACGUI-Uploader/2.0");

                    var fileInfo = new FileInfo(filePath);
                    long fileSize = fileInfo.Length;

                    progressWindow.UpdateStatus($"Uploading... (0/{fileSize / 1024 / 1024}MB)");

                    using (var content = new System.Net.Http.MultipartFormDataContent())
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var fileContent = new System.Net.Http.StreamContent(fileStream);
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                        content.Add(fileContent, "file", Path.GetFileName(filePath));

                        // Add required form fields for SACGUI
                        string hwid = HWIDManager.GetHWID(); // Get the hardware ID
                        content.Add(new System.Net.Http.StringContent(hwid), "hwid");
                        content.Add(new System.Net.Http.StringContent("SACGUI-2.0"), "version");
                        System.Diagnostics.Debug.WriteLine($"[UPLOAD] Added HWID: {hwid}");
                        System.Diagnostics.Debug.WriteLine($"[UPLOAD] Added Version: SACGUI-2.0");

                        // Note: Progress tracking would require a custom HttpMessageHandler
                        // For now, just show that upload is in progress
                        progressWindow.UpdateStatus($"Uploading {fileSize / 1024 / 1024}MB to server...");

                        // Use streaming upload endpoint for large files
                        var retentionHours = 168; // 7 days
                        var uploadUrl = $"https://pydrive.harryeffingpotter.com/upload/stream?retention_hours={retentionHours}";
                        System.Diagnostics.Debug.WriteLine($"[UPLOAD] Sending POST to {uploadUrl}");

                        // Create simpler content for streaming endpoint (only needs file)
                        using (var streamContent = new System.Net.Http.MultipartFormDataContent())
                        {
                            fileStream.Position = 0;
                            var streamFileContent = new System.Net.Http.StreamContent(fileStream);
                            streamFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                            streamContent.Add(streamFileContent, "file", Path.GetFileName(filePath));

                            var response = await client.PostAsync(uploadUrl, streamContent);

                            if (response.IsSuccessStatusCode)
                            {
                                var responseContent = await response.Content.ReadAsStringAsync();
                                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Response: {responseContent}");

                                // Parse streaming endpoint response (returns JSON with share_url)
                                try
                                {
                                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);

                                    // Get the share URL from response
                                    string shareUrl = result?.share_url?.ToString();

                                    if (!string.IsNullOrEmpty(shareUrl))
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[UPLOAD] Extracted share URL: {shareUrl}");
                                        return shareUrl;
                                    }

                                    // Check for file_id to construct download URL
                                    string fileId = result?.file_id?.ToString();
                                    if (!string.IsNullOrEmpty(fileId))
                                    {
                                        shareUrl = $"https://pydrive.harryeffingpotter.com/download/{fileId}";
                                        System.Diagnostics.Debug.WriteLine($"[UPLOAD] Constructed download URL: {shareUrl}");
                                        return shareUrl;
                                    }

                                    throw new Exception($"Upload succeeded but no share_url or file_id was returned. Response: {responseContent}");
                                }
                                catch (Exception parseEx)
                                {
                                    throw new Exception($"Failed to parse upload response: {parseEx.Message}");
                                }
                            }
                            else
                            {
                                string error = await response.Content.ReadAsStringAsync();
                                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Failed with status {response.StatusCode}: {error}");
                                throw new Exception($"Upload failed: {response.StatusCode} - {error}");
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] HTTP Request Exception: {httpEx.Message}");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Inner Exception: {httpEx.InnerException?.Message}");
                throw new Exception($"Upload failed (HTTP): {httpEx.Message}. Inner: {httpEx.InnerException?.Message}");
            }
            catch (TaskCanceledException tcEx)
            {
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Task Cancelled/Timeout: {tcEx.Message}");
                throw new Exception($"Upload timed out: {tcEx.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] General Exception: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Stack: {ex.StackTrace}");
                throw new Exception($"Upload failed: {ex.Message}");
            }
        }

        private async Task NotifyBackendCompletion(string appId, string uploadType, string uploadUrl = null, long fileSize = 0)
        {
            // Use the proper RequestAPIEnhanced.CompleteUpload method
            // This will send the data in the correct format to /sacgui/api/uploadcomplete
            await RequestAPIEnhanced.CompleteUpload(appId, uploadType, uploadUrl ?? "", fileSize);
        }

        private void RequestButton_Click(object sender, EventArgs e)
        {
            // Open the Request window to search and request games
            var requestForm = new RequestsWindow((SteamAppId)mainForm);
            requestForm.Show();
        }

        private void PulseTimer_Tick(object sender, EventArgs e)
        {
            // Timer tick implementation if needed
        }

        private void GamesGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Cell formatting implementation
        }

        private void EnhancedShareWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainForm.Visible = true;
        }

        private void EnhancedShareWindow_Load(object sender, EventArgs e)
        {
            _ = LoadGamesAndRequests();
        }

        private void ShowUploadSuccess(string url, string gameName, bool cracked)
        {
            var successForm = new Form
            {
                Text = "Upload Complete!",
                Size = new Size(500, 300),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            var lblSuccess = new Label
            {
                Text = $"✅ {gameName} ({(cracked ? "Cracked" : "Clean")}) uploaded successfully!",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Lime,
                Location = new Point(20, 20),
                Size = new Size(450, 30)
            };

            var txtUrl = new TextBox
            {
                Text = url,
                Location = new Point(20, 60),
                Size = new Size(450, 25),
                ReadOnly = true,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.Cyan
            };

            var btnCopy = new Button
            {
                Text = "📋 Copy URL",
                Location = new Point(150, 100),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(url);
                btnCopy.Text = "✓ Copied!";
            };

            successForm.Controls.AddRange(new Control[] { lblSuccess, txtUrl, btnCopy });
            successForm.ShowDialog(this);
        }

        private void ShowHonorNotification(string gameName)
        {
            var toast = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(400, 150),
                BackColor = Color.FromArgb(0, 50, 0),
                TopMost = true,
                ShowInTaskbar = false
            };

            var screen = Screen.PrimaryScreen.WorkingArea;
            toast.Location = new Point(screen.Width - 420, screen.Height - 170);

            var lblTitle = new Label
            {
                Text = "🏆 REQUEST HONORED!",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(20, 20),
                Size = new Size(360, 30)
            };

            var lblMessage = new Label
            {
                Text = $"Thank you for sharing {gameName}!\nThe community is grateful! 💚",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 60),
                Size = new Size(360, 60)
            };

            toast.Controls.AddRange(new Control[] { lblTitle, lblMessage });
            toast.Show();

            var timer = new Timer { Interval = 5000 };
            timer.Tick += (s, e) =>
            {
                toast.Close();
                timer.Stop();
            };
            timer.Start();
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
            var paths = new List<string>();

            // Common Steam locations
            var commonPaths = new[]
            {
                @"C:\Program Files (x86)\Steam\steamapps",
                @"C:\Program Files\Steam\steamapps",
                @"D:\Steam\steamapps",
                @"D:\SteamLibrary\steamapps",
                @"E:\Steam\steamapps",
                @"E:\SteamLibrary\steamapps"
            };

            paths.AddRange(commonPaths.Where(Directory.Exists));

            // Check for additional libraries from libraryfolders.vdf
            var mainSteamPath = commonPaths.FirstOrDefault(Directory.Exists);
            if (mainSteamPath != null)
            {
                var vdfPath = Path.Combine(mainSteamPath, "libraryfolders.vdf");
                if (File.Exists(vdfPath))
                {
                    var vdfContent = File.ReadAllText(vdfPath);
                    var pathMatches = System.Text.RegularExpressions.Regex.Matches(vdfContent, @"""path""\s+""([^""]+)""");
                    foreach (System.Text.RegularExpressions.Match match in pathMatches)
                    {
                        var libPath = Path.Combine(match.Groups[1].Value.Replace(@"\\", @"\"), "steamapps");
                        if (Directory.Exists(libPath) && !paths.Contains(libPath))
                            paths.Add(libPath);
                    }
                }
            }

            return paths;
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

                if (!appIdMatch.Success || !nameMatch.Success) return null;

                var steamPath = Path.GetDirectoryName(manifestPath);
                var installDir = installDirMatch.Success ?
                    Path.Combine(steamPath, "common", installDirMatch.Groups[1].Value) : "";

                return new SteamGame
                {
                    AppId = appIdMatch.Groups[1].Value,
                    Name = nameMatch.Groups[1].Value,
                    BuildId = buildIdMatch.Success ? buildIdMatch.Groups[1].Value : "Unknown",
                    InstallDir = installDir
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
        }
    }

    // RGB Progress Window
    public class RGBProgressWindow : Form
    {
        private ProgressBar progressBar;
        private Label lblStatus;
        private Timer rgbTimer;
        private int colorStep = 0;

        public RGBProgressWindow(string gameName, string type)
        {
            InitializeWindow(gameName, type);
        }

        private void InitializeWindow(string gameName, string type)
        {
            this.Text = $"Sharing {gameName} ({type})";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            lblStatus = new Label
            {
                Text = "Preparing...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(20, 30),
                Size = new Size(450, 30)
            };

            progressBar = new ProgressBar
            {
                Location = new Point(20, 70),
                Size = new Size(450, 30),
                Style = ProgressBarStyle.Continuous
            };

            this.Controls.AddRange(new Control[] { lblStatus, progressBar });

            // RGB effect
            SetupRGBEffect();
        }

        private void SetupRGBEffect()
        {
            rgbTimer = new Timer { Interval = 50 };
            rgbTimer.Tick += (s, e) =>
            {
                colorStep = (colorStep + 5) % 360;
                var color = HSLToRGB(colorStep, 1.0, 0.5);
                this.BackColor = Color.FromArgb(30 + color.R / 10, 30 + color.G / 10, 30 + color.B / 10);
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
    }
}