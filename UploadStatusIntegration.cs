using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    public static class UploadStatusManager
    {
        // Add upload status column to DataGridView
        public static void AddUploadStatusColumn(DataGridView grid)
        {
            // Add column for upload status
            var uploadColumn = new DataGridViewButtonColumn
            {
                Name = "UploadStatus",
                HeaderText = "Upload",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(40, 40, 40),
                    ForeColor = Color.White,
                    SelectionBackColor = Color.FromArgb(60, 60, 60)
                }
            };
            grid.Columns.Add(uploadColumn);
        }

        // After successful upload - update the row
        public static void OnUploadSuccess(DataGridView grid, int rowIndex, string uploadUrl, string gameName)
        {
            if (rowIndex >= 0 && rowIndex < grid.Rows.Count)
            {
                var row = grid.Rows[rowIndex];

                // Change button to show success with copy icon
                var cell = row.Cells["UploadStatus"] as DataGridViewButtonCell;
                if (cell != null)
                {
                    cell.Value = "✅ Uploaded! 📋";
                    cell.Style.BackColor = Color.FromArgb(0, 100, 0);
                    cell.Style.ForeColor = Color.White;
                    cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                    // Store upload URL in tag
                    cell.Tag = uploadUrl;
                }

                // Also update a status column if exists
                if (row.Cells["Status"] != null)
                {
                    row.Cells["Status"].Value = "Ready to post on RIN";
                    row.Cells["Status"].Style.ForeColor = Color.Lime;
                }

                // Auto-copy to clipboard
                Clipboard.SetText(uploadUrl);

                // Show toast notification
                ShowUploadToast(gameName, uploadUrl);
            }
        }

        // Handle click on upload status button
        public static void HandleUploadStatusClick(DataGridView grid, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == grid.Columns["UploadStatus"]?.Index && e.RowIndex >= 0)
            {
                var cell = grid.Rows[e.RowIndex].Cells["UploadStatus"];
                var uploadUrl = cell.Tag as string;

                if (!string.IsNullOrEmpty(uploadUrl))
                {
                    // Copy to clipboard
                    Clipboard.SetText(uploadUrl);

                    // Visual feedback
                    cell.Value = "📋 Copied!";

                    // Reset after 2 seconds
                    Task.Delay(2000).ContinueWith(_ =>
                    {
                        grid.Invoke(new Action(() =>
                        {
                            cell.Value = "✅ Uploaded! 📋";
                        }));
                    });

                    // Optional: Open RIN search
                    var gameName = grid.Rows[e.RowIndex].Cells["GameName"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(gameName))
                    {
                        var result = MessageBox.Show(
                            $"Link copied!\n\nOpen CS.RIN.RU search for {gameName}?",
                            "Upload Ready",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords={Uri.EscapeDataString(gameName)}&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = searchUrl,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
        }

        // Show small toast notification
        private static void ShowUploadToast(string gameName, string uploadUrl)
        {
            var toast = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(350, 80),
                BackColor = Color.FromArgb(0, 150, 0),
                TopMost = true,
                ShowInTaskbar = false
            };

            // Position in bottom-right corner
            var screen = Screen.PrimaryScreen.WorkingArea;
            toast.Location = new Point(screen.Width - toast.Width - 20, screen.Height - toast.Height - 20);

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 120, 0),
                Padding = new Padding(1)
            };

            var innerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 30, 20)
            };

            var iconLabel = new Label
            {
                Text = "✅",
                Font = new Font("Segoe UI", 20),
                Location = new Point(10, 20),
                Size = new Size(40, 40)
            };

            var textLabel = new Label
            {
                Text = $"{gameName}\nUploaded! Link copied to clipboard",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(60, 15),
                Size = new Size(280, 50)
            };

            var closeButton = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray,
                Location = new Point(320, 5),
                Size = new Size(20, 20),
                Cursor = Cursors.Hand
            };
            closeButton.Click += (s, e) => toast.Close();

            innerPanel.Controls.AddRange(new Control[] { iconLabel, textLabel, closeButton });
            panel.Controls.Add(innerPanel);
            toast.Controls.Add(panel);

            toast.Show();

            // Auto-close after 5 seconds
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 5000;
            timer.Tick += (s, e) =>
            {
                toast.Close();
                timer.Stop();
            };
            timer.Start();

            // Fade in effect
            toast.Opacity = 0;
            var fadeTimer = new System.Windows.Forms.Timer();
            fadeTimer.Interval = 50;
            fadeTimer.Tick += (s, e) =>
            {
                if (toast.Opacity < 1)
                    toast.Opacity += 0.1;
                else
                    fadeTimer.Stop();
            };
            fadeTimer.Start();
        }
    }

    // Integration with main form DataGridView
    public static class GridIntegration
    {
        public static void SetupGameGrid(DataGridView grid)
        {
            // Add columns
            grid.Columns.Add("GameName", "Game");
            grid.Columns.Add("AppID", "App ID");
            grid.Columns.Add("BuildID", "Build");
            grid.Columns.Add("RINStatus", "RIN Status");

            // Add upload status column
            var uploadBtn = new DataGridViewButtonColumn
            {
                Name = "UploadStatus",
                HeaderText = "Upload",
                Text = "Share",
                UseColumnTextForButtonValue = true,
                Width = 100
            };
            grid.Columns.Add(uploadBtn);

            grid.Columns.Add("Status", "Status");

            // Style
            grid.BackgroundColor = Color.FromArgb(30, 30, 30);
            grid.GridColor = Color.FromArgb(50, 50, 50);
            grid.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            grid.DefaultCellStyle.ForeColor = Color.White;

            // Handle cell clicks
            grid.CellClick += (s, e) =>
            {
                if (e.ColumnIndex == grid.Columns["UploadStatus"]?.Index && e.RowIndex >= 0)
                {
                    var cell = grid.Rows[e.RowIndex].Cells["UploadStatus"];

                    if (cell.Tag != null) // Already uploaded
                    {
                        UploadStatusManager.HandleUploadStatusClick(grid, e);
                    }
                    else // Start upload
                    {
                        // Trigger upload process
                        var gameName = grid.Rows[e.RowIndex].Cells["GameName"]?.Value?.ToString();
                        var installPath = grid.Rows[e.RowIndex].Cells["InstallPath"]?.Value?.ToString();

                        if (!string.IsNullOrEmpty(gameName) && !string.IsNullOrEmpty(installPath))
                        {
                            // Start upload process
                            Task.Run(async () =>
                            {
                                // Your upload logic here
                                var uploadUrl = await UploadGame(gameName, installPath);

                                if (!string.IsNullOrEmpty(uploadUrl))
                                {
                                    grid.Invoke(new Action(() =>
                                    {
                                        UploadStatusManager.OnUploadSuccess(grid, e.RowIndex, uploadUrl, gameName);
                                    }));
                                }
                            });
                        }
                    }
                }
            };
        }

        private static async Task<string> UploadGame(string gameName, string installPath)
        {
            // Implement upload logic
            await Task.Delay(1000); // Simulated upload
            return $"https://filehost.com/{Guid.NewGuid():N}";
        }
    }
}