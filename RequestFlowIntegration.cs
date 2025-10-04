using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    public static class RequestFlowManager
    {
        // On app startup - check if user has requested games
        public static async Task<List<RequestedGameAlert>> CheckForRequestedGames(List<SteamGame> userGames)
        {
            var alerts = new List<RequestedGameAlert>();

            try
            {
                // Get list of requested AppIDs from API
                var requestedAppIds = await GetRequestedAppIds();

                foreach (var appId in requestedAppIds)
                {
                    var userGame = userGames.FirstOrDefault(g => g.AppId == appId);
                    if (userGame != null)
                    {
                        alerts.Add(new RequestedGameAlert
                        {
                            Game = userGame,
                            Message = $"🔥 {userGame.Name} - NEEDED BY COMMUNITY!"
                        });
                    }
                }
            }
            catch { }

            return alerts;
        }

        // After user uploads to a file host
        public static void OnFileUploaded(string gameName, string uploadUrl, string appId)
        {
            // Show completion dialog with RIN search link
            var dialog = new UploadCompleteDialog(gameName, uploadUrl, appId);
            dialog.ShowDialog();
        }

        private static async Task<List<string>> GetRequestedAppIds()
        {
            // Call API endpoint
            // For now, mock data
            await Task.CompletedTask; // Placeholder for future async operation
            return new List<string> { "371660", "418240", "242760" }; // Example AppIDs
        }

        public class RequestedGameAlert
        {
            public SteamGame Game { get; set; }
            public string Message { get; set; }
        }

        public class SteamGame
        {
            public string AppId { get; set; }
            public string Name { get; set; }
            public string InstallDir { get; set; }
            public string BuildId { get; set; }
        }
    }

    // Dialog shown after successful upload
    public class UploadCompleteDialog : Form
    {
        private readonly string gameName;
        private readonly string uploadUrl;
        private readonly string appId;

        public UploadCompleteDialog(string gameName, string uploadUrl, string appId)
        {
            this.gameName = gameName;
            this.uploadUrl = uploadUrl;
            this.appId = appId;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Upload Complete!";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Success message
            var successLabel = new Label
            {
                Text = "✅ UPLOAD SUCCESSFUL!",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Lime,
                Location = new Point(20, 20),
                Size = new Size(460, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Game info
            var gameLabel = new Label
            {
                Text = $"Game: {gameName}",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(20, 70),
                Size = new Size(460, 25)
            };

            // Upload URL (with copy button)
            var urlLabel = new Label
            {
                Text = "Upload URL:",
                ForeColor = Color.Gray,
                Location = new Point(20, 100),
                Size = new Size(100, 25)
            };

            var urlBox = new TextBox
            {
                Text = uploadUrl,
                Location = new Point(20, 125),
                Size = new Size(360, 25),
                ReadOnly = true,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.Cyan
            };

            var copyButton = new Button
            {
                Text = "📋 Copy",
                Location = new Point(385, 124),
                Size = new Size(75, 27),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            copyButton.Click += (s, e) =>
            {
                Clipboard.SetText(uploadUrl);
                copyButton.Text = "✓ Copied!";
                Task.Delay(2000).ContinueWith(_ =>
                {
                    this.Invoke(new Action(() => copyButton.Text = "📋 Copy"));
                });
            };

            // Instructions
            var instructionLabel = new Label
            {
                Text = "📌 NEXT STEPS:\n\n" +
                       "1. Click button below to search CS.RIN.RU\n" +
                       "2. Find the game thread (NOT Content Sharing)\n" +
                       "3. Post your upload link in the thread\n" +
                       "4. You're a hero! 🎉",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 160),
                Size = new Size(460, 100)
            };

            // RIN Search button
            var rinButton = new Button
            {
                Text = "🔍 OPEN CS.RIN.RU SEARCH",
                Location = new Point(100, 260),
                Size = new Size(300, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            rinButton.Click += (s, e) =>
            {
                // Open CS.RIN.RU search for this game
                var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords={Uri.EscapeDataString(gameName)}&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
                Process.Start(new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true
                });

                // Mark request as honored in API
                Task.Run(async () => await MarkRequestHonored(appId));

                // Close dialog
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.AddRange(new Control[] {
                successLabel, gameLabel, urlLabel, urlBox, copyButton,
                instructionLabel, rinButton
            });
        }

        private async Task MarkRequestHonored(string appId)
        {
            try
            {
                // Call API to mark this request as honored
                await RequestAPI.HonorRequest(appId, "both");
            }
            catch { }
        }
    }

    // Notification panel for main form
    public class RequestNotificationPanel : Panel
    {
        private List<RequestFlowManager.RequestedGameAlert> alerts;

        public RequestNotificationPanel()
        {
            this.Size = new Size(400, 100);
            this.BackColor = Color.FromArgb(50, 0, 0);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Visible = false;
        }

        public void ShowAlerts(List<RequestFlowManager.RequestedGameAlert> gameAlerts)
        {
            if (gameAlerts == null || !gameAlerts.Any())
            {
                this.Visible = false;
                return;
            }

            alerts = gameAlerts;
            this.Controls.Clear();

            var titleLabel = new Label
            {
                Text = $"🔥 {alerts.Count} GAMES NEEDED BY COMMUNITY",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Yellow,
                Location = new Point(10, 10),
                Size = new Size(380, 25)
            };

            var listLabel = new Label
            {
                Text = string.Join("\n", alerts.Take(3).Select(a => $"• {a.Game.Name}")),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(10, 35),
                Size = new Size(280, 50)
            };

            var shareButton = new Button
            {
                Text = "SHARE",
                Location = new Point(300, 35),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 200, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            shareButton.Click += (s, e) =>
            {
                // Open share dialog for first needed game
                var firstGame = alerts.First().Game;
                // Trigger share process for this game
            };

            this.Controls.AddRange(new Control[] { titleLabel, listLabel, shareButton });
            this.Visible = true;
        }
    }
}