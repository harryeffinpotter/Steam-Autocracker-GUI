using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SteamAppIdIdentifier
{
    public class UnifiedShareRequestForm : Form
    {
        private DataGridView gamesGrid;
        private Dictionary<string, RequestAPI.RequestedGame> allRequests;
        private Dictionary<string, SteamGame> userGames;
        private Dictionary<string, string> userActiveRequests; // Track user's active requests
        private Timer pulseTimer;
        private Form mainForm;
        private Label lblHeader;
        private Label lblUserStats;

        public UnifiedShareRequestForm(Form parent)
        {
            mainForm = parent;
            allRequests = new Dictionary<string, RequestAPI.RequestedGame>();
            userGames = new Dictionary<string, SteamGame>();
            userActiveRequests = new Dictionary<string, string>();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            this.Text = "Community Share & Request Hub";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 30);
            this.ForeColor = Color.White;

            // Hide main form
            mainForm.Visible = false;
            this.FormClosed += (s, e) => mainForm.Visible = true;

            // Header panel
            var headerPanel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 30, 40)
            };

            lblHeader = new Label
            {
                Text = "Loading community requests...",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(20, 10),
                Size = new Size(800, 35)
            };

            lblUserStats = new Label
            {
                Text = $"Your Honor: {HWIDManager.GetHonorScore()} ⭐",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gold,
                Location = new Point(20, 45),
                Size = new Size(800, 25)
            };

            // Hidden stats panel - click to expand
            var statsPanel = new Panel
            {
                Location = new Point(850, 10),
                Size = new Size(300, 60),
                BackColor = Color.FromArgb(35, 35, 45),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            var lblStatsTitle = new Label
            {
                Text = "📊 Your Stats (click to expand)",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(150, 150, 150),
                Location = new Point(5, 5),
                Size = new Size(290, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblDetailedStats = new Label
            {
                Text = "Loading...",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(5, 25),
                Size = new Size(290, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Name = "DetailedStats"
            };

            // Click to expand/collapse detailed stats
            bool expanded = false;
            statsPanel.Click += async (s, e) =>
            {
                expanded = !expanded;
                if (expanded)
                {
                    statsPanel.Size = new Size(300, 120);
                    lblStatsTitle.Text = "📊 Your Community Impact";
                    await LoadDetailedUserStats(lblDetailedStats);
                }
                else
                {
                    statsPanel.Size = new Size(300, 60);
                    lblStatsTitle.Text = "📊 Your Stats (click to expand)";
                    lblDetailedStats.Text = "Loading...";
                }
            };

            statsPanel.Controls.Add(lblStatsTitle);
            statsPanel.Controls.Add(lblDetailedStats);

            headerPanel.Controls.Add(lblHeader);
            headerPanel.Controls.Add(lblUserStats);
            headerPanel.Controls.Add(statsPanel);

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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            SetupColumns();
            SetupGridStyles();

            this.Controls.Add(gamesGrid);
            this.Controls.Add(headerPanel);

            // Load everything
            _ = LoadGamesAndRequests();
        }

        private void SetupColumns()
        {
            // Game name
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GameName",
                HeaderText = "Game",
                Width = 300,
                ReadOnly = true
            });

            // Ownership status
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                Width = 120,
                ReadOnly = true
            });

            // Request count
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Requests",
                HeaderText = "Requests",
                Width = 100,
                ReadOnly = true
            });

            // Request type needed
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TypeNeeded",
                HeaderText = "Type Needed",
                Width = 120,
                ReadOnly = true
            });

            // Your build
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "YourBuild",
                HeaderText = "Your Build",
                Width = 100,
                ReadOnly = true
            });

            // Action buttons - Share Clean
            var shareCleanBtn = new DataGridViewButtonColumn
            {
                Name = "ShareClean",
                HeaderText = "Share Clean",
                Width = 110,
                UseColumnTextForButtonValue = false
            };
            gamesGrid.Columns.Add(shareCleanBtn);

            // Share Cracked
            var shareCrackedBtn = new DataGridViewButtonColumn
            {
                Name = "ShareCracked",
                HeaderText = "Share Cracked",
                Width = 110,
                UseColumnTextForButtonValue = false
            };
            gamesGrid.Columns.Add(shareCrackedBtn);

            // Request/Store button
            var actionBtn = new DataGridViewButtonColumn
            {
                Name = "Action",
                HeaderText = "Action",
                Width = 120,
                UseColumnTextForButtonValue = false
            };
            gamesGrid.Columns.Add(actionBtn);

            // Hidden columns
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "AppID", Visible = false });
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InstallPath", Visible = false });
            gamesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsOwned", Visible = false });

            // Handle clicks
            gamesGrid.CellClick += GamesGrid_CellClick;
            gamesGrid.CellFormatting += GamesGrid_CellFormatting;
        }

        private void SetupGridStyles()
        {
            gamesGrid.DefaultCellStyle.BackColor = Color.FromArgb(25, 25, 30);
            gamesGrid.DefaultCellStyle.ForeColor = Color.White;
            gamesGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 50, 70);
            gamesGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9);

            gamesGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 50);
            gamesGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gamesGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            gamesGrid.ColumnHeadersHeight = 35;
        }

        private async Task LoadGamesAndRequests()
        {
            try
            {
                // Get all active requests
                var requests = await RequestAPI.GetActiveRequests();

                // Get user's active requests (one per HWID)
                var userRequests = await RequestAPI.GetUserActiveRequests(HWIDManager.GetUserId());
                foreach (var req in userRequests)
                {
                    userActiveRequests[req.AppId] = req.RequestType;
                }

                // Scan user's Steam library
                var steamGames = ScanSteamLibraries();
                foreach (var game in steamGames)
                {
                    userGames[game.AppId] = game;
                }

                // Build combined list
                var allAppIds = new HashSet<string>();

                // Add all requested games
                foreach (var req in requests)
                {
                    allRequests[req.AppId] = req;
                    allAppIds.Add(req.AppId);
                }

                // Add all owned games
                foreach (var appId in userGames.Keys)
                {
                    allAppIds.Add(appId);
                }

                // Update header
                this.Invoke(new Action(() =>
                {
                    int urgentCount = requests.Count(r => r.RequestCount > 10);
                    int hotCount = requests.Count(r => r.RequestCount > 5);

                    if (urgentCount > 0)
                    {
                        lblHeader.Text = $"🚨 {urgentCount} GAMES CRITICALLY NEEDED! 🔥 {hotCount} HOT REQUESTS";
                        lblHeader.ForeColor = Color.Red;
                    }
                    else if (hotCount > 0)
                    {
                        lblHeader.Text = $"🔥 {hotCount} games are hot requests from the community";
                        lblHeader.ForeColor = Color.Orange;
                    }
                    else
                    {
                        lblHeader.Text = $"📦 {requests.Count} active community requests";
                        lblHeader.ForeColor = Color.FromArgb(100, 200, 255);
                    }
                }));

                // Populate grid - OWNED GAMES FIRST, then sorted by request count
                var sortedList = allAppIds.OrderByDescending(appId =>
                {
                    int score = 0;

                    // Owned games get highest priority
                    if (userGames.ContainsKey(appId))
                        score += 10000;

                    // Then sort by request count
                    if (allRequests.ContainsKey(appId))
                        score += allRequests[appId].RequestCount;

                    return score;
                });

                foreach (var appId in sortedList)
                {
                    var row = gamesGrid.Rows[gamesGrid.Rows.Add()];
                    row.Cells["AppID"].Value = appId;

                    bool isOwned = userGames.ContainsKey(appId);
                    row.Cells["IsOwned"].Value = isOwned;

                    if (isOwned)
                    {
                        var game = userGames[appId];
                        row.Cells["GameName"].Value = game.Name;
                        row.Cells["Status"].Value = "✅ Owned";
                        row.Cells["YourBuild"].Value = game.BuildId;
                        row.Cells["InstallPath"].Value = game.InstallDir;

                        // Share buttons
                        row.Cells["ShareClean"].Value = "📦 Share Clean";
                        row.Cells["ShareCracked"].Value = "🎮 Share Cracked";

                        // Check if we already requested this
                        if (userActiveRequests.ContainsKey(appId))
                        {
                            row.Cells["Action"].Value = "📢 Already Requested";
                            row.Cells["Action"].Style.BackColor = Color.FromArgb(60, 60, 60);
                        }
                        else
                        {
                            row.Cells["Action"].Value = "📢 Request Update";
                        }
                    }
                    else if (allRequests.ContainsKey(appId))
                    {
                        var req = allRequests[appId];
                        row.Cells["GameName"].Value = req.GameName;
                        row.Cells["Status"].Value = "❌ Not Owned";
                        row.Cells["YourBuild"].Value = "-";

                        // Disable share buttons
                        row.Cells["ShareClean"].Value = "-";
                        row.Cells["ShareCracked"].Value = "-";

                        // Steam store button
                        row.Cells["Action"].Value = "🛒 View on Steam";
                        row.Cells["Action"].Style.BackColor = Color.FromArgb(0, 100, 200);
                        row.Cells["Action"].Style.ForeColor = Color.White;
                    }

                    // Add request info if exists
                    if (allRequests.ContainsKey(appId))
                    {
                        var req = allRequests[appId];
                        row.Cells["Requests"].Value = req.RequestCount.ToString();
                        row.Cells["TypeNeeded"].Value = req.RequestType;

                        // Apply urgency styling
                        ApplyUrgencyStyling(row, req, isOwned);
                    }
                    else
                    {
                        row.Cells["Requests"].Value = "0";
                        row.Cells["TypeNeeded"].Value = "-";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyUrgencyStyling(DataGridViewRow row, RequestAPI.RequestedGame req, bool isOwned)
        {
            // Color gradient based on request count
            Color bgColor;
            Color fgColor;
            Font font = new Font("Segoe UI", 9);

            // Highlight the specific type requested
            if (isOwned && req.RequestCount > 0)
            {
                // Highlight the SPECIFIC share button based on request type
                if (req.RequestType == "Clean")
                {
                    row.Cells["ShareClean"].Style.BackColor = Color.FromArgb(255, 20, 147);
                    row.Cells["ShareClean"].Style.ForeColor = Color.White;
                    row.Cells["ShareClean"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    row.Cells["ShareClean"].Value = "📦 CLEAN NEEDED!";
                }
                else if (req.RequestType == "Cracked")
                {
                    row.Cells["ShareCracked"].Style.BackColor = Color.FromArgb(255, 20, 147);
                    row.Cells["ShareCracked"].Style.ForeColor = Color.White;
                    row.Cells["ShareCracked"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    row.Cells["ShareCracked"].Value = "🎮 CRACKED NEEDED!";
                }
                else if (req.RequestType == "Both")
                {
                    // Both types needed - highlight both
                    row.Cells["ShareClean"].Style.BackColor = Color.FromArgb(255, 20, 147);
                    row.Cells["ShareClean"].Style.ForeColor = Color.White;
                    row.Cells["ShareClean"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    row.Cells["ShareClean"].Value = "📦 NEEDED!";

                    row.Cells["ShareCracked"].Style.BackColor = Color.FromArgb(255, 20, 147);
                    row.Cells["ShareCracked"].Style.ForeColor = Color.White;
                    row.Cells["ShareCracked"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    row.Cells["ShareCracked"].Value = "🎮 NEEDED!";
                }
            }

            if (!isOwned)
            {
                // Not owned - grey to red gradient based on requests
                if (req.RequestCount >= 20)
                {
                    bgColor = Color.FromArgb(80, 0, 0); // Dark red
                    fgColor = Color.FromArgb(255, 100, 100);
                    font = new Font("Segoe UI", 10, FontStyle.Bold);
                    row.Cells["GameName"].Value = "🔥🔥 " + row.Cells["GameName"].Value;
                }
                else if (req.RequestCount >= 10)
                {
                    bgColor = Color.FromArgb(60, 20, 20); // Medium red
                    fgColor = Color.FromArgb(255, 150, 150);
                    font = new Font("Segoe UI", 9, FontStyle.Bold);
                    row.Cells["GameName"].Value = "🔥 " + row.Cells["GameName"].Value;
                }
                else if (req.RequestCount >= 5)
                {
                    bgColor = Color.FromArgb(40, 30, 30); // Light red tint
                    fgColor = Color.FromArgb(200, 150, 150);
                }
                else if (req.RequestCount >= 2)
                {
                    bgColor = Color.FromArgb(35, 35, 35); // Slightly less grey
                    fgColor = Color.FromArgb(150, 150, 150);
                }
                else
                {
                    bgColor = Color.FromArgb(30, 30, 30); // Dark grey
                    fgColor = Color.FromArgb(120, 120, 120);
                }
            }
            else
            {
                // Owned - show urgency for sharing
                if (req.RequestCount >= 10)
                {
                    bgColor = Color.FromArgb(0, 50, 0); // Green - you can help!
                    fgColor = Color.FromArgb(100, 255, 100);
                    font = new Font("Segoe UI", 10, FontStyle.Bold);

                    // Make share buttons hot pink
                    if (req.RequestType == "Clean" || req.RequestType == "Both")
                    {
                        row.Cells["ShareClean"].Value = "📦 NEEDED!";
                        row.Cells["ShareClean"].Style.BackColor = Color.FromArgb(255, 20, 147);
                        row.Cells["ShareClean"].Style.ForeColor = Color.White;
                        row.Cells["ShareClean"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }

                    if (req.RequestType == "Cracked" || req.RequestType == "Both")
                    {
                        row.Cells["ShareCracked"].Value = "🎮 NEEDED!";
                        row.Cells["ShareCracked"].Style.BackColor = Color.FromArgb(255, 20, 147);
                        row.Cells["ShareCracked"].Style.ForeColor = Color.White;
                        row.Cells["ShareCracked"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
                else if (req.RequestCount >= 5)
                {
                    bgColor = Color.FromArgb(30, 30, 0); // Yellow tint
                    fgColor = Color.FromArgb(255, 255, 150);
                }
                else
                {
                    bgColor = Color.FromArgb(25, 25, 30);
                    fgColor = Color.White;
                }
            }

            row.DefaultCellStyle.BackColor = bgColor;
            row.DefaultCellStyle.ForeColor = fgColor;
            row.DefaultCellStyle.Font = font;

            // Make request count stand out
            if (req.RequestCount >= 10)
            {
                row.Cells["Requests"].Style.ForeColor = Color.Red;
                row.Cells["Requests"].Style.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                row.Cells["Requests"].Value = $"🚨 {req.RequestCount}";
            }
            else if (req.RequestCount >= 5)
            {
                row.Cells["Requests"].Style.ForeColor = Color.Orange;
                row.Cells["Requests"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                row.Cells["Requests"].Value = $"🔥 {req.RequestCount}";
            }
        }

        private void GamesGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Additional formatting if needed
        }

        private async void GamesGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = gamesGrid.Rows[e.RowIndex];
            var appId = row.Cells["AppID"].Value?.ToString();
            var gameName = row.Cells["GameName"].Value?.ToString();
            var isOwned = row.Cells["IsOwned"].Value?.ToString() == "True";

            // Handle Action button (Request or Steam Store)
            if (e.ColumnIndex == gamesGrid.Columns["Action"].Index)
            {
                if (!isOwned)
                {
                    // Open Steam store
                    OpenSteamStore(appId);
                }
                else
                {
                    // Check if already requested
                    if (userActiveRequests.ContainsKey(appId))
                    {
                        MessageBox.Show(
                            $"You already have an active request for {gameName}.\n" +
                            "Only one request per game is allowed.",
                            "Request Already Active",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    // Show request dialog
                    var dialog = new RequestGameDialog(gameName, appId);
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Submit request
                        bool success = await RequestAPIEnhanced.SubmitGameRequest(
                            appId, gameName, dialog.SelectedType.ToString());

                        if (success)
                        {
                            userActiveRequests[appId] = dialog.SelectedType.ToString();
                            row.Cells["Action"].Value = "📢 Already Requested";
                            row.Cells["Action"].Style.BackColor = Color.FromArgb(60, 60, 60);

                            MessageBox.Show(
                                $"Request submitted for {gameName} ({dialog.SelectedType})",
                                "Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Reload to update counts
                            _ = LoadGamesAndRequests();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to submit request. Please try again.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
                return;
            }

            // Handle Share Clean
            if (e.ColumnIndex == gamesGrid.Columns["ShareClean"].Index && isOwned)
            {
                var installPath = row.Cells["InstallPath"].Value?.ToString();
                if (!string.IsNullOrEmpty(installPath))
                {
                    await ShareGame(gameName, installPath, appId, false, row);
                }
                return;
            }

            // Handle Share Cracked
            if (e.ColumnIndex == gamesGrid.Columns["ShareCracked"].Index && isOwned)
            {
                var installPath = row.Cells["InstallPath"].Value?.ToString();
                if (!string.IsNullOrEmpty(installPath))
                {
                    await ShareGame(gameName, installPath, appId, true, row);
                }
                return;
            }
        }

        private void OpenSteamStore(string appId)
        {
            try
            {
                var steamUrl = $"https://store.steampowered.com/app/{appId}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = steamUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show("Failed to open Steam store page.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ShareGame(string gameName, string installPath, string appId, bool cracked, DataGridViewRow row)
        {
            if (!cracked)
            {
                // Use the new auto-cleanup system for clean shares
                if (!await ShareCleanIntegration.PrepareCleanShare(
                    installPath, gameName, appId, this))
                {
                    return;
                }
            }

            var progressForm = new RGBProgressWindow(gameName, cracked ? "Cracked" : "Clean");
            progressForm.Show(this);

            try
            {
                var btnColumn = cracked ? "ShareCracked" : "ShareClean";
                row.Cells[btnColumn].Value = "⏳ Processing...";

                // Your compression and upload logic here
                progressForm.UpdateStatus("Compressing files...");
                await Task.Delay(2000); // Simulate work

                progressForm.UpdateStatus("Uploading...");
                await Task.Delay(2000); // Simulate work

                // Notify backend
                string uploadType = cracked ? "cracked" : "clean";
                string uploadUrl = $"https://example.com/{Guid.NewGuid()}";

                await RequestAPIEnhanced.CompleteUpload(appId, uploadType, uploadUrl);

                row.Cells[btnColumn].Value = "✅ Shared!";
                row.Cells[btnColumn].Style.BackColor = Color.FromArgb(0, 100, 0);

                progressForm.Complete(uploadUrl);

                // Check if this fulfilled a request
                if (allRequests.ContainsKey(appId))
                {
                    ShowHonorNotification(gameName);

                    // Update honor display
                    lblUserStats.Text = $"Your Honor: {HWIDManager.GetHonorScore()} ⭐";

                    // Update detailed stats if panel is expanded
                    var statsPanel = this.Controls.Find("DetailedStats", true).FirstOrDefault();
                    if (statsPanel != null)
                    {
                        _ = LoadDetailedUserStats(statsPanel as Label);
                    }
                }

                // Reload to update request counts
                _ = LoadGamesAndRequests();
            }
            catch (Exception ex)
            {
                progressForm.ShowError($"Error: {ex.Message}");
                row.Cells[cracked ? "ShareCracked" : "ShareClean"].Value =
                    cracked ? "🎮 Share Cracked" : "📦 Share Clean";
            }
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
                ShowInTaskbar = false,
                Opacity = 0.95
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
                Text = $"Thank you for sharing {gameName}!\nThe community is grateful! 💚\n+1 Honor Point",
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

        private async Task LoadDetailedUserStats(Label statsLabel)
        {
            try
            {
                statsLabel.Text = "Loading stats...";

                // Get detailed user statistics from backend
                var userStats = await RequestAPI.GetDetailedUserStats(HWIDManager.GetUserId());

                if (userStats != null)
                {
                    var statsText = $"Fulfilled: {userStats.Fulfilled} 🎆\n";
                    statsText += $"Requested: {userStats.Requested} 📢\n";
                    statsText += $"Filled: {userStats.Filled} 🙏\n";
                    statsText += $"\nRatio: {userStats.GetGiveToTakeRatio():F1}:1";

                    statsLabel.Text = statsText;

                    // Color code based on ratio
                    if (userStats.GetGiveToTakeRatio() >= 2.0)
                    {
                        statsLabel.ForeColor = Color.FromArgb(100, 255, 100); // Green - generous contributor
                    }
                    else if (userStats.GetGiveToTakeRatio() >= 1.0)
                    {
                        statsLabel.ForeColor = Color.FromArgb(255, 255, 100); // Yellow - balanced
                    }
                    else if (userStats.GetGiveToTakeRatio() >= 0.5)
                    {
                        statsLabel.ForeColor = Color.FromArgb(255, 200, 100); // Orange - could give more
                    }
                    else
                    {
                        statsLabel.ForeColor = Color.FromArgb(255, 150, 150); // Light red - mostly taking
                    }
                }
                else
                {
                    statsLabel.Text = "Fulfilled: 0 🎆\nRequested: 0 📢\nFilled: 0 🙏\n\nNew User!";
                    statsLabel.ForeColor = Color.FromArgb(150, 150, 150);
                }
            }
            catch
            {
                statsLabel.Text = "Stats unavailable";
                statsLabel.ForeColor = Color.FromArgb(150, 150, 150);
            }
        }

        private List<SteamGame> ScanSteamLibraries()
        {
            // Implementation would scan Steam libraries
            // This is placeholder
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

    // Updated RequestAPI to handle one request per HWID
    public static partial class RequestAPI
    {
        public static async Task<List<RequestedGame>> GetUserActiveRequests(string userId)
        {
            try
            {
                var response = await client.GetAsync($"{API_BASE}/requests/user/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<RequestedGame>>(json);
                }
            }
            catch { }
            return new List<RequestedGame>();
        }

        public static async Task<bool> SubmitGameRequest(string appId, string gameName, string requestType)
        {
            try
            {
                // Check if user already has a request for this game
                var userRequests = await GetUserActiveRequests(HWIDManager.GetUserId());
                if (userRequests.Any(r => r.AppId == appId))
                {
                    return false; // Already requested
                }

                // Submit new request
                var request = new
                {
                    appId = appId,
                    gameName = gameName,
                    requestType = requestType,
                    userId = HWIDManager.GetUserId(),
                    hwid = HWIDManager.GetHWID(),
                    timestamp = DateTime.UtcNow
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/requests", content);

                if (response.IsSuccessStatusCode)
                {
                    await HWIDManager.RecordRequest(appId, requestType);
                    return true;
                }

                // Check if it's because they already requested
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return false;
                }
            }
            catch { }
            return false;
        }
    }
}