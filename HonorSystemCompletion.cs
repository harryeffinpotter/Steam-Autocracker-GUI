using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SteamAppIdIdentifier
{
    public static class HonorSystem
    {
        // After user uploads and copies link, show completion button
        public static void ShowCompletionButton(DataGridView grid, int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < grid.Rows.Count)
            {
                var row = grid.Rows[rowIndex];

                // Change upload button to completion checkmark
                var cell = row.Cells["UploadStatus"] as DataGridViewButtonCell;
                if (cell != null)
                {
                    cell.Value = "✓ Mark Complete";
                    cell.Style.BackColor = Color.FromArgb(0, 100, 50);
                    cell.Style.ForeColor = Color.White;
                    cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                    // Store state
                    cell.Tag = "ready_to_complete";
                }
            }
        }

        // Handle completion click
        public static async Task HandleCompletionClick(DataGridView grid, int rowIndex, string appId, string gameName)
        {
            var cell = grid.Rows[rowIndex].Cells["UploadStatus"];

            if (cell.Tag?.ToString() == "ready_to_complete")
            {
                // Show confirmation
                var result = MessageBox.Show(
                    $"Did you post the link for {gameName} on CS.RIN.RU?\n\n" +
                    "Click YES only if you've posted in the game thread.",
                    "Confirm Upload Posted",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Update UI immediately
                    cell.Value = "⏳ Marking...";
                    cell.Style.BackColor = Color.FromArgb(100, 100, 0);

                    // Send completion to server
                    bool success = await MarkRequestComplete(appId);

                    if (success)
                    {
                        // Success!
                        cell.Value = "✅ HONORED!";
                        cell.Style.BackColor = Color.FromArgb(0, 150, 0);
                        cell.Tag = "completed";

                        // Update user's honor score locally
                        IncrementHonorScore();

                        // Show thank you message
                        ShowThankYouToast(gameName);

                        // Update global stats display
                        await UpdateGlobalStats();
                    }
                    else
                    {
                        // Failed - revert
                        cell.Value = "✓ Mark Complete";
                        cell.Style.BackColor = Color.FromArgb(0, 100, 50);

                        MessageBox.Show(
                            "Failed to mark as complete. Please try again.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            }
        }

        // Send completion to API
        private static async Task<bool> MarkRequestComplete(string appId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var data = new
                    {
                        appId = appId,
                        completerId = RequestAPI.GetHWID(),
                        completedAt = DateTime.UtcNow
                    };

                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"https://your-api.com/api/complete", content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        // Local honor tracking
        private static void IncrementHonorScore()
        {
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\SteamAutoCracker");
                var currentScore = (int)(key.GetValue("HonorScore") ?? 0);
                key.SetValue("HonorScore", currentScore + 1);
                key.Close();
            }
            catch { }
        }

        public static int GetHonorScore()
        {
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\SteamAutoCracker");
                return (int)(key?.GetValue("HonorScore") ?? 0);
            }
            catch
            {
                return 0;
            }
        }

        // Thank you notification
        private static void ShowThankYouToast(string gameName)
        {
            var toast = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(400, 120),
                BackColor = Color.FromArgb(0, 50, 0),
                TopMost = true,
                ShowInTaskbar = false,
                Opacity = 0
            };

            var screen = Screen.PrimaryScreen.WorkingArea;
            toast.Location = new Point(screen.Width - toast.Width - 20, screen.Height - toast.Height - 20);

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 100, 0),
                BorderStyle = BorderStyle.FixedSingle
            };

            var titleLabel = new Label
            {
                Text = "🏆 REQUEST HONORED!",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(20, 15),
                Size = new Size(360, 30)
            };

            var messageLabel = new Label
            {
                Text = $"Thank you for sharing {gameName}!\nYour honor score: {GetHonorScore()}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 45),
                Size = new Size(360, 40)
            };

            var statsLabel = new Label
            {
                Text = "The community thanks you! 💚",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.LightGreen,
                Location = new Point(20, 85),
                Size = new Size(360, 20)
            };

            panel.Controls.AddRange(new Control[] { titleLabel, messageLabel, statsLabel });
            toast.Controls.Add(panel);

            toast.Show();

            // Fade in
            var fadeInTimer = new Timer { Interval = 30 };
            fadeInTimer.Tick += (s, e) =>
            {
                if (toast.Opacity < 0.95)
                    toast.Opacity += 0.05;
                else
                {
                    fadeInTimer.Stop();

                    // Auto close after 5 seconds
                    Task.Delay(5000).ContinueWith(_ =>
                    {
                        toast.Invoke(new Action(() =>
                        {
                            // Fade out
                            var fadeOutTimer = new Timer { Interval = 30 };
                            fadeOutTimer.Tick += (s2, e2) =>
                            {
                                if (toast.Opacity > 0.05)
                                    toast.Opacity -= 0.05;
                                else
                                {
                                    fadeOutTimer.Stop();
                                    toast.Close();
                                }
                            };
                            fadeOutTimer.Start();
                        }));
                    });
                }
            };
            fadeInTimer.Start();
        }

        // Update the global stats display
        private static async Task UpdateGlobalStats()
        {
            // This would update the main form's status display
            // "Requests: 51 | Honored: 52 | Filled: 47"
            await Task.CompletedTask;
        }
    }

    // Simple completion flow in main form
    public class CompletionFlowManager
    {
        private DataGridView grid;
        private string currentAppId;
        private string currentGameName;

        public void InitializeFlow(DataGridView gameGrid)
        {
            this.grid = gameGrid;

            // Handle cell clicks
            grid.CellClick += async (s, e) =>
            {
                if (e.ColumnIndex == grid.Columns["UploadStatus"]?.Index && e.RowIndex >= 0)
                {
                    var cell = grid.Rows[e.RowIndex].Cells["UploadStatus"];
                    var cellValue = cell.Value?.ToString() ?? "";

                    if (cellValue.Contains("Mark Complete"))
                    {
                        currentAppId = grid.Rows[e.RowIndex].Cells["AppID"]?.Value?.ToString();
                        currentGameName = grid.Rows[e.RowIndex].Cells["GameName"]?.Value?.ToString();

                        await HonorSystem.HandleCompletionClick(grid, e.RowIndex, currentAppId, currentGameName);
                    }
                }
            };
        }
    }

    // Honor score display for main form
    public class HonorScoreDisplay : Label
    {
        public HonorScoreDisplay()
        {
            this.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.ForeColor = Color.Gold;
            this.BackColor = Color.Transparent;
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.Size = new Size(150, 25);

            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            int score = HonorSystem.GetHonorScore();
            this.Text = $"⭐ Honor: {score}";

            // Change color based on score
            if (score >= 50)
                this.ForeColor = Color.Gold;
            else if (score >= 20)
                this.ForeColor = Color.Silver;
            else if (score >= 10)
                this.ForeColor = Color.FromArgb(205, 127, 50); // Bronze
            else
                this.ForeColor = Color.Gray;
        }
    }
}