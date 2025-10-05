using APPID;
using SAC_GUI;
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
        private Form mainForm;
        private Timer rgbTimer;
        private int colorHue = 0;

        public EnhancedShareWindow(Form parent)
        {
            mainForm = parent;
            InitializeComponent();  // Use the Designer.cs file instead!

            // Setup modern progress bar style
            SetupModernProgressBar();

            // Hide main form and show again when this closes
            mainForm.Visible = false;
            this.FormClosed += (s, e) => mainForm.Visible = true;
        }

        private void SetupModernProgressBar()
        {
            // Start RGB animation timer
            rgbTimer = new Timer();
            rgbTimer.Interval = 30; // Smooth animation
            rgbTimer.Tick += (s, e) =>
            {
                colorHue = (colorHue + 2) % 360;
                if (progressBar.Visible)
                {
                    progressBar.Invalidate(); // Force redraw
                }
            };
            rgbTimer.Start();

            progressBar.Paint += (s, e) =>
            {
                Rectangle rec = e.ClipRectangle;
                rec.Width = (int)(rec.Width * ((double)progressBar.Value / progressBar.Maximum)) - 4;
                if (ProgressBarRenderer.IsSupported)
                    rec.Height = rec.Height - 4;

                // Draw dark background matching form
                e.Graphics.Clear(Color.FromArgb(15, 15, 15));

                // Draw RGB gradient progress bar
                rec.Height = rec.Height - 4;
                if (rec.Width > 0)
                {
                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        new Point(0, 0),
                        new Point(rec.Width, 0),
                        HSLToRGB(colorHue, 1.0, 0.5),
                        HSLToRGB((colorHue + 60) % 360, 1.0, 0.5)))
                    {
                        e.Graphics.FillRectangle(brush, 2, 2, rec.Width, rec.Height);
                    }
                }
            };
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

            mainPanel.Controls.Add(gamesGrid);
            this.Controls.Add(mainPanel);
            this.Controls.Add(headerPanel);

            // Load games
            _ = LoadGames();
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

            // Add to grid
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
            }
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
                var mainFormTyped = mainForm as SteamAppId;
                if (mainFormTyped != null)
                {
                    // Set the game directory and app ID in the main form
                    mainFormTyped.GameDirectory = installPath;
                    SteamAppId.APPID = appId;

                    // Show status
                    row.Cells["ShareCracked"].Value = "⚙️ Cracking...";

                    try
                    {
                        // Crack the game
                        bool crackSuccess = await mainFormTyped.CrackAsync();

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
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
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
                            // Convert the 1fichier link
                            row.Cells[btnColumn].Value = "⏳ Converting link...";
                            string convertedUrl = await Convert1FichierLink(uploadUrl);

                            row.Cells[btnColumn].Value = "✅ Shared!";
                            row.Cells[btnColumn].Style.BackColor = Color.FromArgb(0, 100, 0);

                            // Notify backend about completion with converted URL
                            long fileSize = new FileInfo(outputPath).Length;
                            string uploadType = cracked ? "cracked" : "clean";
                            await NotifyBackendCompletion(appId, uploadType, convertedUrl, fileSize);

                            ShowUploadSuccess(convertedUrl, gameName, cracked);
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
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] === Starting 1fichier upload from EnhancedShareWindow ===");
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] File: {filePath}");
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] File exists: {File.Exists(filePath)}");
            if (File.Exists(filePath))
            {
                var fi = new FileInfo(filePath);
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] File size: {fi.Length / (1024.0 * 1024.0):F2} MB");
            }

            // Show the progress window
            progressWindow.Show(this);
            progressWindow.Refresh();

            try
            {
                var fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                progressWindow.UpdateStatus($"Uploading to 1fichier... (0/{fileSize / 1024 / 1024}MB)");

                // Create progress handler
                var progress = new Progress<double>(value =>
                {
                    var mbUploaded = (long)(fileSize * value) / 1024 / 1024;
                    var mbTotal = fileSize / 1024 / 1024;
                    var percentage = (int)(value * 100);
                    progressWindow.UpdateStatus($"Uploading to 1fichier... {percentage}% ({mbUploaded}/{mbTotal}MB)");
                });

                // Use 1fichier uploader
                using (var uploader = new OneFichierUploader())
                {
                    var result = await uploader.UploadFileAsync(filePath, progress);

                    if (!string.IsNullOrEmpty(result.DownloadUrl))
                    {
                        System.Diagnostics.Debug.WriteLine($"[UPLOAD] 1fichier upload successful. Download URL: {result.DownloadUrl}");
                        progressWindow.UpdateStatus($"Upload complete!");
                        await Task.Delay(1000); // Show completion briefly
                        progressWindow.Close();
                        return result.DownloadUrl;
                    }
                    else
                    {
                        progressWindow.Close();
                        throw new Exception("Upload succeeded but no download URL was returned");
                    }
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

        private async Task<string> Convert1FichierLink(string oneFichierUrl)
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

                    System.Diagnostics.Debug.WriteLine($"[CONVERT] Converting 1fichier link: {oneFichierUrl}");
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
                        System.Diagnostics.Debug.WriteLine($"[CONVERT] Failed with status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONVERT] Exception: {ex.Message}");
            }

            // If conversion fails, return original link
            return oneFichierUrl;
        }

        private async Task NotifyBackendCompletion(string appId, string uploadType, string uploadUrl = null, long fileSize = 0)
        {
            // Use the proper RequestAPIEnhanced.CompleteUpload method
            // This will send the data in the correct format to /sacgui/api/uploadcomplete
            await RequestAPIEnhanced.CompleteUpload(appId, uploadType, uploadUrl ?? "", fileSize);
        }

        private void PulseTimer_Tick(object sender, EventArgs e)
        {
            // Timer tick implementation if needed
        }

        private void GamesGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Additional cell formatting for button styling
            if (e.ColumnIndex >= 0 && gamesGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                var cell = gamesGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.Style.BackColor = Color.FromArgb(30, 30, 30);
                cell.Style.ForeColor = Color.FromArgb(150, 150, 150);
                cell.Style.SelectionBackColor = Color.FromArgb(40, 40, 40);
            }
        }

        private void EnhancedShareWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainForm.Visible = true;
        }

        private void EnhancedShareWindow_Load(object sender, EventArgs e)
        {
            _ = LoadGames();
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

        private void lblHeader_Click(object sender, EventArgs e)
        {

        }

        // Custom title bar drag functionality
        private Point mouseDownPoint = Point.Empty;

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

        private void gamesGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }

    // Modern Progress Bar
    public class ModernProgressBar : ProgressBar
    {
        public ModernProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = e.ClipRectangle;

            rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
            if (ProgressBarRenderer.IsSupported)
                rec.Height = rec.Height - 4;

            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(20, 20, 20)), e.ClipRectangle);
            rec.Height = rec.Height - 4;
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 150, 200)), 2, 2, rec.Width, rec.Height);
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
                ForeColor = Color.Cyan,  // Start with a color, will be animated
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
                // Apply RGB effect to the text
                lblStatus.ForeColor = color;

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