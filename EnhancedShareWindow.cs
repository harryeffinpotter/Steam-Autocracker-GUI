using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    public class EnhancedShareWindow : Form
    {
        private DataGridView gamesGrid;
        private Dictionary<string, RequestAPI.RequestedGame> requestedGames;
        private Form mainForm;
        private Timer pulseTimer;
        private List<DataGridViewRow> urgentRows = new List<DataGridViewRow>();

        public EnhancedShareWindow(Form parent)
        {
            mainForm = parent;
            requestedGames = new Dictionary<string, RequestAPI.RequestedGame>();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            this.Text = "Share Your Games - Community Needs You!";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 30);
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
                BackColor = Color.FromArgb(30, 30, 40)
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
                BackgroundColor = Color.FromArgb(15, 15, 20),
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
            gamesGrid.DefaultCellStyle.BackColor = Color.FromArgb(25, 25, 30);
            gamesGrid.DefaultCellStyle.ForeColor = Color.White;
            gamesGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 50, 70);
            gamesGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 50);
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

            // Request button
            var requestBtn = new DataGridViewButtonColumn
            {
                Name = "RequestGame",
                HeaderText = "Request",
                Text = "📢 Request",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            gamesGrid.Columns.Add(requestBtn);

            // Hidden columns
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "AppID", Visible = false });
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InstallPath", Visible = false });

            // Cell click handler
            gamesGrid.CellClick += GamesGrid_CellClick;
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
            pulseTimer = new Timer { Interval = 100 };
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

            // Handle Request button
            if (e.ColumnIndex == gamesGrid.Columns["RequestGame"].Index)
            {
                var dialog = new RequestGameDialog(gameName, appId);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    MessageBox.Show($"Request submitted for {gameName} ({dialog.SelectedType})",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reload requests
                    _ = LoadGamesAndRequests();
                }
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

        private async Task ShareGame(string gameName, string installPath, string appId, bool cracked, DataGridViewRow row)
        {
            if (string.IsNullOrEmpty(installPath) || !Directory.Exists(installPath))
            {
                MessageBox.Show("Game installation path not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create progress window with RGB effect
            var progressForm = new RGBProgressWindow(gameName, cracked ? "Cracked" : "Clean");
            progressForm.Show(this);

            try
            {
                // Update button to show processing
                var btnColumn = cracked ? "ShareCracked" : "ShareClean";
                row.Cells[btnColumn].Value = "⏳ Processing...";

                // Compress the game
                progressForm.UpdateStatus("Compressing files...");
                string outputPath = await CompressGameWithProgress(installPath, gameName, cracked, progressForm);

                // Upload the file
                progressForm.UpdateStatus("Uploading to server...");
                string uploadUrl = await UploadFileWithProgress(outputPath, progressForm);

                // Notify backend about completion
                progressForm.UpdateStatus("Notifying community...");
                string uploadType = cracked ? "cracked" : "clean";
                await NotifyBackendCompletion(appId, uploadType);

                // Update button to show success
                row.Cells[btnColumn].Value = "✅ Shared!";
                row.Cells[btnColumn].Style.BackColor = Color.FromArgb(0, 100, 0);

                progressForm.Complete(uploadUrl);

                // Show success dialog with URL
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
            // Implementation would compress with progress updates
            await Task.Delay(2000); // Simulate
            return Path.Combine(Path.GetTempPath(), $"{gameName}.zip");
        }

        private async Task<string> UploadFileWithProgress(string filePath, RGBProgressWindow progressWindow)
        {
            // Implementation would upload with progress updates
            await Task.Delay(2000); // Simulate
            return $"https://pydrive.harryeffingpotter.com/files/{Guid.NewGuid()}";
        }

        private async Task NotifyBackendCompletion(string appId, string uploadType)
        {
            string endpoint = $"https://pydrive.harryeffingpotter.com/sacgui/uploadcomplete?type={uploadType}&appid={appId}";
            // Make HTTP call to endpoint
            await RequestAPI.HonorRequest(appId, uploadType);
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
            // Implementation from original Form1.cs
            return new List<SteamGame>();
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
            this.Invoke(new Action(() =>
            {
                lblStatus.Text = status;
                progressBar.Value = Math.Min(progressBar.Value + 20, 100);
            }));
        }

        public void Complete(string url)
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

        public void ShowError(string error)
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