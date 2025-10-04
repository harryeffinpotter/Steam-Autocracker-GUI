using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SteamAppIdIdentifier
{
    public class TopRequestedPanel : Panel
    {
        private FlowLayoutPanel gamesList;
        private Label headerLabel;
        private Timer refreshTimer;
        private List<TopRequestedGame> currentGames;
        private bool isAdminMode = false;

        public class TopRequestedGame
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public int RequestCount { get; set; }
            public DateTime FirstRequested { get; set; }
            public int DaysSinceFirstRequest => (DateTime.UtcNow - FirstRequested).Days;
            public bool IsStale => DaysSinceFirstRequest > 30; // 30+ days = stale
            public string RequestType { get; set; }
        }

        public TopRequestedPanel()
        {
            InitializePanel();
            currentGames = new List<TopRequestedGame>();
            _ = LoadTopRequested();
            SetupAutoRefresh();
        }

        private void InitializePanel()
        {
            this.Size = new Size(350, 500);
            this.BackColor = Color.FromArgb(20, 20, 25);
            this.BorderStyle = BorderStyle.FixedSingle;

            // Header
            headerLabel = new Label
            {
                Text = "🔥 TOP REQUESTED GAMES",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 100, 100),
                Location = new Point(10, 10),
                Size = new Size(320, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Admin toggle (hidden, activated by secret click sequence)
            headerLabel.Click += OnHeaderClick;

            // Scrollable games list
            gamesList = new FlowLayoutPanel
            {
                Location = new Point(5, 45),
                Size = new Size(335, 445),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            this.Controls.Add(headerLabel);
            this.Controls.Add(gamesList);
        }

        private int headerClickCount = 0;
        private DateTime lastHeaderClick = DateTime.MinValue;

        private void OnHeaderClick(object sender, EventArgs e)
        {
            // Secret admin mode: Click header 5 times within 3 seconds
            if ((DateTime.Now - lastHeaderClick).TotalSeconds > 3)
            {
                headerClickCount = 0;
            }

            headerClickCount++;
            lastHeaderClick = DateTime.Now;

            if (headerClickCount >= 5)
            {
                ToggleAdminMode();
                headerClickCount = 0;
            }
        }

        private void ToggleAdminMode()
        {
            isAdminMode = !isAdminMode;

            if (isAdminMode)
            {
                headerLabel.Text = "🛠️ ADMIN MODE - TOP REQUESTED";
                headerLabel.ForeColor = Color.Orange;

                // Show admin controls
                MessageBox.Show(
                    "Admin mode activated!\n\n" +
                    "• Right-click games to remove them\n" +
                    "• Stale requests (30+ days) shown in darker colors\n" +
                    "• Click header again to exit admin mode",
                    "Admin Mode",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                headerLabel.Text = "🔥 TOP REQUESTED GAMES";
                headerLabel.ForeColor = Color.FromArgb(255, 100, 100);
            }

            RefreshGamesList();
        }

        private async Task LoadTopRequested()
        {
            try
            {
                var topGames = await RequestAPI.GetTopRequestedGames(20); // Top 20
                currentGames = topGames.Select(g => new TopRequestedGame
                {
                    AppId = g.AppId,
                    GameName = g.GameName,
                    RequestCount = g.RequestCount,
                    FirstRequested = g.LastRequested ?? DateTime.UtcNow.AddDays(-7), // Use LastRequested or default to 7 days ago
                    RequestType = "Both" // Default since RequestAPI.TopRequestedGame doesn't have this
                }).ToList();

                RefreshGamesList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TOP REQUESTED] Load failed: {ex.Message}");
            }
        }

        private void RefreshGamesList()
        {
            gamesList.Controls.Clear();

            if (!currentGames.Any())
            {
                var noGamesLabel = new Label
                {
                    Text = "No active requests",
                    ForeColor = Color.Gray,
                    Size = new Size(320, 30),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                gamesList.Controls.Add(noGamesLabel);
                return;
            }

            // Sort by request count (highest first)
            var sortedGames = currentGames.OrderByDescending(g => g.RequestCount).ToList();

            for (int i = 0; i < sortedGames.Count; i++)
            {
                var game = sortedGames[i];
                var gamePanel = CreateGamePanel(game, i);
                gamesList.Controls.Add(gamePanel);
            }
        }

        private Panel CreateGamePanel(TopRequestedGame game, int rank)
        {
            var panel = new Panel
            {
                Size = new Size(315, 60),
                Margin = new Padding(2),
                Tag = game
            };

            // Calculate red intensity based on rank and request count
            Color bgColor = CalculateUrgencyColor(game, rank);
            panel.BackColor = bgColor;

            // Rank number
            var rankLabel = new Label
            {
                Text = $"#{rank + 1}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = GetContrastColor(bgColor),
                Location = new Point(5, 5),
                Size = new Size(30, 20)
            };

            // Game name (truncated if too long)
            var gameName = game.GameName.Length > 30 ?
                game.GameName.Substring(0, 30) + "..." :
                game.GameName;

            var nameLabel = new Label
            {
                Text = gameName,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = GetContrastColor(bgColor),
                Location = new Point(40, 5),
                Size = new Size(200, 20)
            };

            // Request count and type
            var requestLabel = new Label
            {
                Text = $"{game.RequestCount} requests ({game.RequestType})",
                Font = new Font("Segoe UI", 8),
                ForeColor = GetContrastColor(bgColor),
                Location = new Point(40, 25),
                Size = new Size(150, 15)
            };

            // Days since first request
            var daysLabel = new Label
            {
                Text = $"{game.DaysSinceFirstRequest} days old",
                Font = new Font("Segoe UI", 8),
                ForeColor = game.IsStale ? Color.Orange : GetContrastColor(bgColor),
                Location = new Point(40, 40),
                Size = new Size(100, 15)
            };

            // Steam store button
            var storeButton = new Button
            {
                Text = "🛒",
                Size = new Size(25, 25),
                Location = new Point(250, 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 200),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            storeButton.Click += (s, e) => OpenSteamStore(game.AppId);

            // Admin remove button (only in admin mode)
            if (isAdminMode)
            {
                var removeButton = new Button
                {
                    Text = "❌",
                    Size = new Size(25, 25),
                    Location = new Point(280, 20),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(150, 0, 0),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8)
                };
                removeButton.Click += async (s, e) => await RemoveGame(game);
                panel.Controls.Add(removeButton);

                // Right-click context menu for admin
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Mark as Uncrackable", null, async (s, e) => await MarkAsUncrackable(game));
                contextMenu.Items.Add("Remove All Requests", null, async (s, e) => await RemoveAllRequests(game));
                contextMenu.Items.Add("Blacklist Game", null, async (s, e) => await BlacklistGame(game));
                panel.ContextMenuStrip = contextMenu;
            }

            // Stale game warning
            if (game.IsStale && isAdminMode)
            {
                var staleIcon = new Label
                {
                    Text = "⚠️",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.Orange,
                    Location = new Point(220, 5),
                    Size = new Size(20, 20)
                };
                panel.Controls.Add(staleIcon);
            }

            panel.Controls.AddRange(new Control[] { rankLabel, nameLabel, requestLabel, daysLabel, storeButton });

            // Hover effects
            panel.MouseEnter += (s, e) =>
            {
                panel.BackColor = LightenColor(bgColor, 20);
            };
            panel.MouseLeave += (s, e) =>
            {
                panel.BackColor = bgColor;
            };

            return panel;
        }

        private Color CalculateUrgencyColor(TopRequestedGame game, int rank)
        {
            // Base intensity from request count (more requests = more red)
            int requestIntensity = Math.Min(game.RequestCount * 8, 200); // Cap at 200

            // Rank modifier (top ranked games get brighter)
            float rankModifier = 1.0f - (rank * 0.05f); // Each rank down reduces by 5%
            rankModifier = Math.Max(0.2f, rankModifier); // Min 20% brightness

            // Age modifier (older requests get dimmer - possible uncrackables)
            float ageModifier = 1.0f;
            if (game.DaysSinceFirstRequest > 30)
            {
                ageModifier = 0.6f; // 40% dimmer for stale requests
            }
            else if (game.DaysSinceFirstRequest > 14)
            {
                ageModifier = 0.8f; // 20% dimmer for older requests
            }

            // Calculate final red value
            int finalRed = (int)(requestIntensity * rankModifier * ageModifier);
            finalRed = Math.Max(30, Math.Min(255, finalRed)); // Keep between 30-255

            // Green and blue stay low for red theme
            int green = Math.Min(finalRed / 4, 50);
            int blue = Math.Min(finalRed / 6, 30);

            return Color.FromArgb(finalRed, green, blue);
        }

        private Color GetContrastColor(Color bgColor)
        {
            // Return white or black based on background brightness
            int brightness = (bgColor.R + bgColor.G + bgColor.B) / 3;
            return brightness > 128 ? Color.Black : Color.White;
        }

        private Color LightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount)
            );
        }

        private void OpenSteamStore(string appId)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = $"https://store.steampowered.com/app/{appId}",
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private async Task RemoveGame(TopRequestedGame game)
        {
            var result = MessageBox.Show(
                $"Remove {game.GameName} from requests?\n\n" +
                $"This will delete all {game.RequestCount} active requests.\n" +
                "This action cannot be undone.",
                "Confirm Removal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                bool success = await RequestAPI.RemoveGameRequests(game.AppId, "admin_removal");
                if (success)
                {
                    currentGames.Remove(game);
                    RefreshGamesList();
                    MessageBox.Show($"Removed {game.GameName} and all requests.", "Success");
                }
                else
                {
                    MessageBox.Show("Failed to remove game.", "Error");
                }
            }
        }

        private async Task MarkAsUncrackable(TopRequestedGame game)
        {
            var result = MessageBox.Show(
                $"Mark {game.GameName} as uncrackable?\n\n" +
                "This will:\n" +
                "• Remove all active requests\n" +
                "• Add to uncrackable games list\n" +
                "• Prevent future requests",
                "Mark as Uncrackable",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                bool success = await RequestAPI.MarkAsUncrackable(game.AppId);
                if (success)
                {
                    currentGames.Remove(game);
                    RefreshGamesList();
                    MessageBox.Show($"Marked {game.GameName} as uncrackable.", "Success");
                }
            }
        }

        private async Task RemoveAllRequests(TopRequestedGame game)
        {
            await RemoveGame(game);
        }

        private async Task BlacklistGame(TopRequestedGame game)
        {
            var result = MessageBox.Show(
                $"Blacklist {game.GameName}?\n\n" +
                "This will permanently prevent any requests for this game.",
                "Blacklist Game",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Stop
            );

            if (result == DialogResult.Yes)
            {
                bool success = await RequestAPI.BlacklistGame(game.AppId);
                if (success)
                {
                    currentGames.Remove(game);
                    RefreshGamesList();
                    MessageBox.Show($"Blacklisted {game.GameName}.", "Success");
                }
            }
        }

        private void SetupAutoRefresh()
        {
            refreshTimer = new Timer { Interval = 60000 }; // Refresh every minute
            refreshTimer.Tick += async (s, e) => await LoadTopRequested();
            refreshTimer.Start();
        }

        public async Task ForceRefresh()
        {
            await LoadTopRequested();
        }
    }

    // Extension to RequestAPI for admin functions
    public static partial class RequestAPI
    {
        public static async Task<bool> RemoveGameRequests(string appId, string reason)
        {
            try
            {
                var data = new
                {
                    appId = appId,
                    reason = reason,
                    adminId = GetHWID()
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/admin/remove-requests", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> MarkAsUncrackable(string appId)
        {
            try
            {
                var data = new
                {
                    appId = appId,
                    adminId = GetHWID(),
                    reason = "uncrackable"
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/admin/mark-uncrackable", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> BlacklistGame(string appId)
        {
            try
            {
                var data = new
                {
                    appId = appId,
                    adminId = GetHWID()
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{API_BASE}/admin/blacklist", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}