using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SteamAppIdIdentifier;

namespace SteamAutocrackGUI
{
    public class BatchGameItem
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string AppId { get; set; }
        public bool Crack { get; set; } = true;
        public bool Zip { get; set; } = false;
        public bool Upload { get; set; } = false;
    }

    /// <summary>
    /// Custom progress bar with neon blue gradient styling
    /// </summary>
    public class NeonProgressBar : ProgressBar
    {
        public NeonProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = this.ClientRectangle;

            // Dark background
            using (var bgBrush = new SolidBrush(Color.FromArgb(10, 12, 20)))
            {
                e.Graphics.FillRectangle(bgBrush, rect);
            }

            // Border
            using (var borderPen = new Pen(Color.FromArgb(40, 50, 70)))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
            }

            // Progress fill with neon blue gradient
            if (this.Value > 0)
            {
                int fillWidth = (int)((rect.Width - 4) * ((double)this.Value / this.Maximum));
                if (fillWidth > 0)
                {
                    using (var brush = new LinearGradientBrush(
                        new Rectangle(2, 2, fillWidth, rect.Height - 4),
                        Color.FromArgb(0, 150, 255),   // Bright neon blue
                        Color.FromArgb(0, 100, 200),   // Darker blue
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(brush, 2, 2, fillWidth, rect.Height - 4);
                    }

                    // Add glow effect on top
                    using (var glowBrush = new SolidBrush(Color.FromArgb(40, 150, 220, 255)))
                    {
                        e.Graphics.FillRectangle(glowBrush, 2, 2, fillWidth, (rect.Height - 4) / 3);
                    }
                }
            }
        }
    }

    public class BatchGameSelectionForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private DataGridView gameGrid;
        private List<string> gamePaths;
        private Dictionary<int, string> detectedAppIds = new Dictionary<int, string>();
        private Dictionary<string, string> convertingUrls = new Dictionary<string, string>(); // gamePath -> 1fichier URL during conversion
        private Dictionary<string, string> finalUrls = new Dictionary<string, string>(); // gamePath -> final URL (pydrive or 1fichier)
        private Dictionary<string, APPID.SteamAppId.CrackDetails> crackDetailsMap = new Dictionary<string, APPID.SteamAppId.CrackDetails>(); // gamePath -> crack details
        private Label compressionLabel;

        // Upload details panel
        private Panel uploadDetailsPanel;
        private Label lblUploadGame;
        private Label lblUploadSize;
        private Label lblUploadSpeed;
        private Label lblUploadEta;
        private NeonProgressBar uploadProgressBar;
        private Button btnSkip;
        private Button btnCancelAll;

        // Skip/Cancel tracking
        private bool skipCurrentGame = false;
        private bool cancelAllRemaining = false;

        // Tooltip for batch form
        private ToolTip batchToolTip;

        public List<BatchGameItem> SelectedGames { get; private set; } = new List<BatchGameItem>();
        public string CompressionFormat { get; private set; } = "ZIP";
        public string CompressionLevel { get; private set; } = "0";
        public bool UseRinPassword { get; private set; } = false;

        // Event for when Process is clicked
        public event Action<List<BatchGameItem>, string, string, bool> ProcessRequested;

        public BatchGameSelectionForm(List<string> paths)
        {
            gamePaths = paths;
            InitializeForm();

            this.Load += (s, e) =>
            {
                ApplyAcrylicEffect();
                CenterToParentWithScreenClamp();
            };
            this.MouseDown += Form_MouseDown;
        }

        private void CenterToParentWithScreenClamp()
        {
            if (this.Owner != null)
            {
                // Center relative to parent
                int x = this.Owner.Location.X + (this.Owner.Width - this.Width) / 2;
                int y = this.Owner.Location.Y + (this.Owner.Height - this.Height) / 2;

                // Clamp to screen bounds
                var screen = Screen.FromControl(this.Owner).WorkingArea;
                x = Math.Max(screen.Left, Math.Min(x, screen.Right - this.Width));
                y = Math.Max(screen.Top, Math.Min(y, screen.Bottom - this.Height));

                this.Location = new Point(x, y);
            }
        }

        private void InitializeForm()
        {
            this.Text = "Batch Process - Select Games";
            this.Size = new Size(760, 580);
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.FromArgb(5, 8, 20);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // Title label
            var titleLabel = new Label
            {
                Text = "Batch Process",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(15, 15),
                Size = new Size(200, 30),
                BackColor = Color.Transparent
            };
            this.Controls.Add(titleLabel);

            // Subtitle
            var subtitleLabel = new Label
            {
                Text = $"Found {gamePaths.Count} games. Select actions for each:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 180, 185),
                Location = new Point(15, 48),
                Size = new Size(480, 20),
                BackColor = Color.Transparent
            };
            this.Controls.Add(subtitleLabel);

            // DataGridView for games
            gameGrid = new DataGridView
            {
                Location = new Point(15, 75),
                Size = new Size(725, 300),
                BackgroundColor = Color.FromArgb(15, 18, 30),
                ForeColor = Color.FromArgb(220, 255, 255),
                GridColor = Color.FromArgb(40, 40, 45),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                EditMode = DataGridViewEditMode.EditProgrammatically,
                Font = new Font("Segoe UI", 9)
            };

            // Style headers
            gameGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 28, 40);
            gameGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(150, 200, 255);
            gameGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            gameGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(25, 28, 40);
            gameGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gameGrid.ColumnHeadersHeight = 32;

            // Style rows
            gameGrid.DefaultCellStyle.BackColor = Color.FromArgb(15, 18, 30);
            gameGrid.DefaultCellStyle.ForeColor = Color.FromArgb(220, 255, 255);
            gameGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 60, 80);
            gameGrid.DefaultCellStyle.SelectionForeColor = Color.White;
            gameGrid.RowTemplate.Height = 28;

            // Alternate row style
            gameGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 23, 35);

            // Add columns
            var nameCol = new DataGridViewTextBoxColumn
            {
                Name = "GameName",
                HeaderText = "Game",
                Width = 200,
                ReadOnly = true
            };
            gameGrid.Columns.Add(nameCol);

            var appIdCol = new DataGridViewTextBoxColumn
            {
                Name = "AppId",
                HeaderText = "AppID",
                Width = 70,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(100, 200, 255)
                }
            };
            gameGrid.Columns.Add(appIdCol);

            var sizeCol = new DataGridViewTextBoxColumn
            {
                Name = "Size",
                HeaderText = "Size",
                Width = 60,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            gameGrid.Columns.Add(sizeCol);

            var crackCol = new DataGridViewCheckBoxColumn
            {
                Name = "Crack",
                HeaderText = "Crack",
                Width = 50
            };
            gameGrid.Columns.Add(crackCol);

            var zipCol = new DataGridViewCheckBoxColumn
            {
                Name = "Zip",
                HeaderText = "Zip",
                Width = 45
            };
            gameGrid.Columns.Add(zipCol);

            var uploadCol = new DataGridViewCheckBoxColumn
            {
                Name = "Upload",
                HeaderText = "Upload",
                Width = 55
            };
            gameGrid.Columns.Add(uploadCol);

            var statusCol = new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = Color.Gray
                }
            };
            gameGrid.Columns.Add(statusCol);

            var detailsCol = new DataGridViewButtonColumn
            {
                Name = "Details",
                HeaderText = "",
                Width = 50,
                Text = "Info",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(150, 200, 255),
                    BackColor = Color.FromArgb(35, 40, 55),
                    Font = new Font("Segoe UI", 8)
                }
            };
            gameGrid.Columns.Add(detailsCol);

            // Custom paint for Details buttons to match dark theme with icon
            Image infoIcon = null;
            try { infoIcon = APPID.Properties.Resources.info_icon; } catch { }

            gameGrid.CellPainting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && gameGrid.Columns[e.ColumnIndex].Name == "Details" && e.RowIndex >= 0)
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

                    // Just draw the icon centered, no background or border
                    if (infoIcon != null)
                    {
                        int iconSize = Math.Min(e.CellBounds.Width - 8, e.CellBounds.Height - 8);
                        iconSize = Math.Min(iconSize, 20); // Cap at 20px
                        int iconX = e.CellBounds.X + (e.CellBounds.Width - iconSize) / 2;
                        int iconY = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;
                        e.Graphics.DrawImage(infoIcon, iconX, iconY, iconSize, iconSize);
                    }
                    else
                    {
                        // Fallback to simple "i" text if no icon
                        using (var textBrush = new SolidBrush(Color.FromArgb(150, 200, 255)))
                        using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
                        {
                            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            e.Graphics.DrawString("ⓘ", font, textBrush, e.CellBounds, sf);
                        }
                    }
                    e.Handled = true;
                }
            };

            // Add game rows
            for (int i = 0; i < gamePaths.Count; i++)
            {
                string path = gamePaths[i];
                int rowIndex = gameGrid.Rows.Add();

                // Check for suspicious structure (multiple steam_api DLLs)
                int steamApiCount = APPID.SteamAppId.CountSteamApiDlls(path);
                string gameName = Path.GetFileName(path);
                if (steamApiCount > 2)
                {
                    gameName = "⚠️ " + gameName;
                    gameGrid.Rows[rowIndex].Cells["GameName"].ToolTipText =
                        $"Warning: Found {steamApiCount} steam_api DLLs in this folder.\nThis might contain multiple games or have an unusual structure.";
                }
                gameGrid.Rows[rowIndex].Cells["GameName"].Value = gameName;

                // Auto-detect AppID
                string appId = DetectAppId(path);
                detectedAppIds[rowIndex] = appId;
                gameGrid.Rows[rowIndex].Cells["AppId"].Value = string.IsNullOrEmpty(appId) ? "?" : appId;

                // Style the AppID cell based on whether it was found
                if (string.IsNullOrEmpty(appId))
                {
                    gameGrid.Rows[rowIndex].Cells["AppId"].Style.ForeColor = Color.Orange;
                    gameGrid.Rows[rowIndex].Cells["AppId"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }

                gameGrid.Rows[rowIndex].Cells["Size"].Value = GetFolderSizeString(path);
                gameGrid.Rows[rowIndex].Cells["Crack"].Value = true;
                gameGrid.Rows[rowIndex].Cells["Zip"].Value = false;
                gameGrid.Rows[rowIndex].Cells["Upload"].Value = false;
                gameGrid.Rows[rowIndex].Cells["Status"].Value = steamApiCount > 2 ? "⚠️ Check structure" : "Pending";
                gameGrid.Rows[rowIndex].Tag = path;
            }

            // Handle clicks on cells
            gameGrid.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;

                string colName = gameGrid.Columns[e.ColumnIndex].Name;

                // Single-click on AppID opens search dialog (works for any value)
                if (colName == "AppId")
                {
                    string gameName = gameGrid.Rows[e.RowIndex].Cells["GameName"].Value?.ToString() ?? "";
                    string currentAppId = gameGrid.Rows[e.RowIndex].Cells["AppId"].Value?.ToString() ?? "";
                    if (currentAppId == "?") currentAppId = "";

                    string newAppId = ShowAppIdSearchDialog(gameName, currentAppId);
                    if (newAppId != null)
                    {
                        detectedAppIds[e.RowIndex] = newAppId;
                        gameGrid.Rows[e.RowIndex].Cells["AppId"].Value = string.IsNullOrEmpty(newAppId) ? "?" : newAppId;

                        var cell = gameGrid.Rows[e.RowIndex].Cells["AppId"];
                        if (string.IsNullOrEmpty(newAppId))
                        {
                            cell.Style.ForeColor = Color.Orange;
                            cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                        }
                        else
                        {
                            cell.Style.ForeColor = Color.FromArgb(100, 200, 255);
                            cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                        }
                    }
                    return;
                }

                // Single-click on Status column - copy URL if available
                if (colName == "Status")
                {
                    string gamePath = gameGrid.Rows[e.RowIndex].Tag?.ToString();
                    string status = gameGrid.Rows[e.RowIndex].Cells["Status"].Value?.ToString() ?? "";
                    string urlToCopy = null;

                    // Check for converting URL (1fichier link during conversion)
                    if (status.StartsWith("Converting") && !string.IsNullOrEmpty(gamePath) && convertingUrls.ContainsKey(gamePath))
                    {
                        urlToCopy = convertingUrls[gamePath];
                    }
                    // Check for final URL (PyDrive or 1fichier after completion)
                    else if ((status.Contains("PyDrive") || status.Contains("1fichier")) && !string.IsNullOrEmpty(gamePath) && finalUrls.ContainsKey(gamePath))
                    {
                        urlToCopy = finalUrls[gamePath];
                    }

                    if (!string.IsNullOrEmpty(urlToCopy))
                    {
                        try
                        {
                            Clipboard.SetText(urlToCopy);
                            // Visual feedback - show "Copied! ✓" for 3 seconds
                            string originalStatus = status;
                            Color originalColor = gameGrid.Rows[e.RowIndex].Cells["Status"].Style.ForeColor;
                            gameGrid.Rows[e.RowIndex].Cells["Status"].Value = "Copied! ✓";
                            gameGrid.Rows[e.RowIndex].Cells["Status"].Style.ForeColor = Color.Cyan;

                            var timer = new Timer { Interval = 3000 };
                            int rowIdx = e.RowIndex;
                            timer.Tick += (ts, te) =>
                            {
                                timer.Stop();
                                timer.Dispose();
                                if (!this.IsDisposed && rowIdx < gameGrid.Rows.Count)
                                {
                                    gameGrid.Rows[rowIdx].Cells["Status"].Value = originalStatus;
                                    gameGrid.Rows[rowIdx].Cells["Status"].Style.ForeColor = originalColor;
                                }
                            };
                            timer.Start();
                        }
                        catch { }
                    }
                    return;
                }

                // Handle checkbox columns
                if (colName == "Crack" || colName == "Zip" || colName == "Upload")
                {
                    var cell = gameGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    bool currentValue = (bool)(cell.Value ?? false);
                    bool newValue = !currentValue;
                    cell.Value = newValue;

                    // Dependency logic - Updated to allow zip/share without crack
                    // Valid combos: crack, crack+zip, crack+zip+upload, zip, zip+upload
                    // Invalid: upload without zip (1fichier requires zipped file)
                    if (colName == "Upload" && newValue)
                    {
                        // Upload requires Zip only (not Crack - can share uncracked games)
                        gameGrid.Rows[e.RowIndex].Cells["Zip"].Value = true;
                    }
                    else if (colName == "Zip" && !newValue)
                    {
                        // Unchecking Zip unchecks Upload (can't upload without zip)
                        gameGrid.Rows[e.RowIndex].Cells["Upload"].Value = false;
                    }
                    // Note: Crack is now fully independent - unchecking it does NOT affect Zip/Upload

                    UpdateCountLabel();
                }

                // Handle Details button click
                if (colName == "Details")
                {
                    string gamePath = gameGrid.Rows[e.RowIndex].Tag?.ToString();
                    if (!string.IsNullOrEmpty(gamePath) && crackDetailsMap.ContainsKey(gamePath))
                    {
                        ShowCrackDetails(crackDetailsMap[gamePath]);
                    }
                    else
                    {
                        MessageBox.Show("No crack details available yet.\nDetails are populated after cracking.",
                            "No Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            this.Controls.Add(gameGrid);

            // Initialize tooltip
            batchToolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 300,
                ReshowDelay = 200
            };

            // Count label
            var countLabel = new Label
            {
                Name = "countLabel",
                Location = new Point(15, 385),
                Size = new Size(200, 20),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9),
                Text = $"{gamePaths.Count} game(s) to crack"
            };
            this.Controls.Add(countLabel);

            // Compression settings row
            var settingsBtn = CreateStyledButton("⚙", new Point(15, 410), new Size(35, 35));
            settingsBtn.Font = new Font("Segoe UI", 12);
            settingsBtn.Click += (s, e) => OpenCompressionSettings();
            batchToolTip.SetToolTip(settingsBtn, "Open compression settings (format, level, password)");
            this.Controls.Add(settingsBtn);

            compressionLabel = new Label
            {
                Location = new Point(55, 418),
                Size = new Size(200, 20),
                ForeColor = Color.FromArgb(140, 140, 145),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8),
                Text = "ZIP Level 0 (No compression)"
            };
            this.Controls.Add(compressionLabel);

            // Bulk action buttons - compact, minimal padding
            var selectAllCrackBtn = CreateStyledButton("All Crack", new Point(313, 388), new Size(68, 28));
            selectAllCrackBtn.Font = new Font("Segoe UI", 8);
            selectAllCrackBtn.Click += (s, e) => SetAllCheckboxes("Crack", true);
            batchToolTip.SetToolTip(selectAllCrackBtn, "Enable Crack for all games");
            this.Controls.Add(selectAllCrackBtn);

            var selectAllZipBtn = CreateStyledButton("All Zip", new Point(384, 388), new Size(55, 28));
            selectAllZipBtn.Font = new Font("Segoe UI", 8);
            selectAllZipBtn.Click += (s, e) => SetAllCheckboxes("Zip", true);
            batchToolTip.SetToolTip(selectAllZipBtn, "Enable Zip for all games");
            this.Controls.Add(selectAllZipBtn);

            var clearAllBtn = CreateStyledButton("Clear All", new Point(442, 388), new Size(62, 28));
            clearAllBtn.Font = new Font("Segoe UI", 8);
            clearAllBtn.Click += (s, e) =>
            {
                foreach (DataGridViewRow row in gameGrid.Rows)
                {
                    row.Cells["Crack"].Value = false;
                    row.Cells["Zip"].Value = false;
                    row.Cells["Upload"].Value = false;
                }
                UpdateCountLabel();
            };
            batchToolTip.SetToolTip(clearAllBtn, "Clear all selections (uncheck all Crack/Zip/Upload)");
            this.Controls.Add(clearAllBtn);

            // Upload details panel (hidden by default, shown during uploads)
            // Position right below the grid (grid ends at Y=375)
            uploadDetailsPanel = new Panel
            {
                Location = new Point(15, 378),
                Size = new Size(725, 40),
                BackColor = Color.FromArgb(20, 23, 35),
                Visible = false
            };

            // Game name being uploaded (left side)
            lblUploadGame = new Label
            {
                Location = new Point(5, 3),
                Size = new Size(350, 16),
                ForeColor = Color.FromArgb(100, 200, 255),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Text = "Uploading: "
            };
            uploadDetailsPanel.Controls.Add(lblUploadGame);

            // Custom-painted progress bar (modern neon blue style) - shrunk to fit buttons
            uploadProgressBar = new NeonProgressBar
            {
                Location = new Point(5, 21),
                Size = new Size(350, 14),
                Maximum = 100
            };
            uploadDetailsPanel.Controls.Add(uploadProgressBar);

            // Size info (e.g., "1.2 GB / 7.6 GB")
            lblUploadSize = new Label
            {
                Location = new Point(360, 21),
                Size = new Size(85, 14),
                ForeColor = Color.FromArgb(180, 180, 185),
                Font = new Font("Segoe UI", 8),
                Text = "",
                TextAlign = ContentAlignment.MiddleLeft
            };
            uploadDetailsPanel.Controls.Add(lblUploadSize);

            // Speed (e.g., "12.5 MB/s")
            lblUploadSpeed = new Label
            {
                Location = new Point(450, 21),
                Size = new Size(70, 14),
                ForeColor = Color.FromArgb(100, 255, 150),
                Font = new Font("Segoe UI", 8),
                Text = "",
                TextAlign = ContentAlignment.MiddleLeft
            };
            uploadDetailsPanel.Controls.Add(lblUploadSpeed);

            // ETA (e.g., "ETA: 5:32")
            lblUploadEta = new Label
            {
                Location = new Point(525, 21),
                Size = new Size(70, 14),
                ForeColor = Color.FromArgb(255, 200, 100),
                Font = new Font("Segoe UI", 8),
                Text = "",
                TextAlign = ContentAlignment.MiddleLeft
            };
            uploadDetailsPanel.Controls.Add(lblUploadEta);

            // Skip button
            btnSkip = new Button
            {
                Name = "btnSkip",
                Location = new Point(625, 5),
                Size = new Size(45, 30),
                Text = "Skip",
                BackColor = Color.FromArgb(80, 70, 20),
                ForeColor = Color.FromArgb(255, 220, 120),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            btnSkip.FlatAppearance.BorderColor = Color.FromArgb(150, 120, 40);
            batchToolTip.SetToolTip(btnSkip, "Skip this game and continue with the next one");
            btnSkip.Click += (s, e) =>
            {
                skipCurrentGame = true;
                btnSkip.Text = "...";
                btnSkip.Enabled = false;
            };
            uploadDetailsPanel.Controls.Add(btnSkip);

            // Cancel All button
            btnCancelAll = new Button
            {
                Name = "btnCancelAll",
                Location = new Point(673, 5),
                Size = new Size(50, 30),
                Text = "Cancel",
                BackColor = Color.FromArgb(100, 30, 30),
                ForeColor = Color.FromArgb(255, 150, 150),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            btnCancelAll.FlatAppearance.BorderColor = Color.FromArgb(150, 60, 60);
            batchToolTip.SetToolTip(btnCancelAll, "Cancel this and all remaining uploads");
            btnCancelAll.Click += (s, e) =>
            {
                cancelAllRemaining = true;
                skipCurrentGame = true;
                btnCancelAll.Text = "...";
                btnCancelAll.Enabled = false;
                btnSkip.Enabled = false;
            };
            uploadDetailsPanel.Controls.Add(btnCancelAll);

            this.Controls.Add(uploadDetailsPanel);

            // Process button
            var processBtn = CreateStyledButton("Process", new Point(560, 495), new Size(90, 40));
            processBtn.BackColor = Color.FromArgb(0, 100, 70);
            processBtn.Click += (s, e) =>
            {
                // Check for missing AppIDs on games that need cracking
                var missingAppIds = new List<string>();
                for (int i = 0; i < gameGrid.Rows.Count; i++)
                {
                    var row = gameGrid.Rows[i];
                    bool crack = (bool)(row.Cells["Crack"].Value ?? false);
                    string appId = row.Cells["AppId"].Value?.ToString();

                    if (crack && (string.IsNullOrEmpty(appId) || appId == "?"))
                    {
                        missingAppIds.Add(row.Cells["GameName"].Value?.ToString() ?? "Unknown");
                    }
                }

                if (missingAppIds.Count > 0)
                {
                    string msg = $"The following games are missing AppIDs:\n\n" +
                        string.Join("\n", missingAppIds.Take(5));
                    if (missingAppIds.Count > 5)
                        msg += $"\n...and {missingAppIds.Count - 5} more";
                    msg += "\n\nDouble-click on the AppID column to set them.\nContinue anyway? (games without AppID will be skipped)";

                    if (MessageBox.Show(msg, "Missing AppIDs", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        return;
                }

                for (int i = 0; i < gameGrid.Rows.Count; i++)
                {
                    var row = gameGrid.Rows[i];
                    bool crack = (bool)(row.Cells["Crack"].Value ?? false);
                    bool zip = (bool)(row.Cells["Zip"].Value ?? false);
                    bool upload = (bool)(row.Cells["Upload"].Value ?? false);
                    string appId = row.Cells["AppId"].Value?.ToString();
                    if (appId == "?") appId = "";

                    if (crack || zip || upload)
                    {
                        SelectedGames.Add(new BatchGameItem
                        {
                            Path = gamePaths[i],
                            Name = Path.GetFileName(gamePaths[i]),
                            AppId = appId,
                            Crack = crack,
                            Zip = zip,
                            Upload = upload
                        });
                    }
                }

                if (SelectedGames.Count == 0)
                {
                    MessageBox.Show("Please select at least one action for at least one game.",
                        "No Actions Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Fire the event - form stays open
                ProcessRequested?.Invoke(SelectedGames, CompressionFormat, CompressionLevel, UseRinPassword);
            };
            batchToolTip.SetToolTip(processBtn, "Start processing selected games (crack, zip, upload)");
            this.Controls.Add(processBtn);

            // Cancel button
            var cancelBtn = CreateStyledButton("Cancel", new Point(660, 495), new Size(80, 40));
            cancelBtn.Click += (s, e) =>
            {
                this.Close();
            };
            batchToolTip.SetToolTip(cancelBtn, "Close this window without processing");
            this.Controls.Add(cancelBtn);
        }

        /// <summary>
        /// Updates the status for a game by its path
        /// </summary>
        public void UpdateStatus(string gamePath, string status, Color? color = null)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => UpdateStatus(gamePath, status, color))); } catch { }
                return;
            }

            for (int i = 0; i < gameGrid.Rows.Count; i++)
            {
                if (gameGrid.Rows[i].Tag?.ToString() == gamePath)
                {
                    gameGrid.Rows[i].Cells["Status"].Value = status;
                    if (color.HasValue)
                    {
                        gameGrid.Rows[i].Cells["Status"].Style.ForeColor = color.Value;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the status for a game by row index
        /// </summary>
        public void UpdateStatusByIndex(int rowIndex, string status, Color? color = null)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => UpdateStatusByIndex(rowIndex, status, color))); } catch { }
                return;
            }

            if (rowIndex >= 0 && rowIndex < gameGrid.Rows.Count)
            {
                gameGrid.Rows[rowIndex].Cells["Status"].Value = status;
                if (color.HasValue)
                {
                    gameGrid.Rows[rowIndex].Cells["Status"].Style.ForeColor = color.Value;
                }
            }
        }

        /// <summary>
        /// Stores the 1fichier URL for a game during conversion (allows click-to-copy)
        /// </summary>
        public void SetConvertingUrl(string gamePath, string oneFichierUrl)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => SetConvertingUrl(gamePath, oneFichierUrl))); } catch { }
                return;
            }

            if (!string.IsNullOrEmpty(gamePath) && !string.IsNullOrEmpty(oneFichierUrl))
            {
                convertingUrls[gamePath] = oneFichierUrl;
            }
        }

        /// <summary>
        /// Clears the converting URL for a game (after conversion completes)
        /// </summary>
        public void ClearConvertingUrl(string gamePath)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => ClearConvertingUrl(gamePath))); } catch { }
                return;
            }

            if (convertingUrls.ContainsKey(gamePath))
            {
                convertingUrls.Remove(gamePath);
            }
        }

        /// <summary>
        /// Stores the final URL for a game (for click-to-copy after completion)
        /// </summary>
        public void SetFinalUrl(string gamePath, string finalUrl)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => SetFinalUrl(gamePath, finalUrl))); } catch { }
                return;
            }

            if (!string.IsNullOrEmpty(gamePath) && !string.IsNullOrEmpty(finalUrl))
            {
                finalUrls[gamePath] = finalUrl;
            }
        }

        /// <summary>
        /// Shows the upload details panel with initial info
        /// </summary>
        public void ShowUploadDetails(string gameName, long totalBytes)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => ShowUploadDetails(gameName, totalBytes))); } catch { }
                return;
            }

            // Reset skip flag for new game (but not cancel - that stays)
            skipCurrentGame = false;
            btnSkip.Text = "Skip";
            btnSkip.Enabled = !cancelAllRemaining;
            btnCancelAll.Text = "Cancel";
            btnCancelAll.Enabled = !cancelAllRemaining;

            uploadDetailsPanel.Visible = true;
            lblUploadGame.Text = $"Uploading: {gameName}";
            lblUploadSize.Text = $"0 / {FormatBytes(totalBytes)}";
            lblUploadSpeed.Text = "";
            lblUploadEta.Text = "";
            uploadProgressBar.Value = 0;
            uploadProgressBar.Invalidate(); // Force repaint for custom drawing
        }

        /// <summary>
        /// Reset all skip/cancel state - call before starting a batch
        /// </summary>
        public void ResetSkipCancelState()
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => ResetSkipCancelState())); } catch { }
                return;
            }

            skipCurrentGame = false;
            cancelAllRemaining = false;
            btnSkip.Text = "Skip";
            btnSkip.Enabled = true;
            btnCancelAll.Text = "Cancel";
            btnCancelAll.Enabled = true;
        }

        /// <summary>
        /// Check if user clicked Skip for current game
        /// </summary>
        public bool ShouldSkipCurrentGame() => skipCurrentGame;

        /// <summary>
        /// Check if user clicked Cancel All
        /// </summary>
        public bool ShouldCancelAll() => cancelAllRemaining;

        /// <summary>
        /// Updates the upload progress with speed and ETA
        /// </summary>
        public void UpdateUploadProgress(int percent, long uploadedBytes, long totalBytes, double bytesPerSecond)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => UpdateUploadProgress(percent, uploadedBytes, totalBytes, bytesPerSecond))); } catch { }
                return;
            }

            uploadProgressBar.Value = Math.Min(percent, 100);
            uploadProgressBar.Invalidate(); // Force repaint for custom drawing
            lblUploadSize.Text = $"{FormatBytes(uploadedBytes)} / {FormatBytes(totalBytes)}";

            if (bytesPerSecond > 0)
            {
                lblUploadSpeed.Text = $"{FormatBytes((long)bytesPerSecond)}/s";

                long remainingBytes = totalBytes - uploadedBytes;
                if (remainingBytes > 0)
                {
                    double secondsRemaining = remainingBytes / bytesPerSecond;
                    lblUploadEta.Text = FormatEta(secondsRemaining);
                }
                else
                {
                    lblUploadEta.Text = "";
                }
            }
        }

        /// <summary>
        /// Hides the upload details panel
        /// </summary>
        public void HideUploadDetails()
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => HideUploadDetails())); } catch { }
                return;
            }

            uploadDetailsPanel.Visible = false;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
            if (bytes >= 1024L * 1024)
                return $"{bytes / (1024.0 * 1024):F1} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F0} KB";
            return $"{bytes} B";
        }

        private string FormatEta(double seconds)
        {
            if (seconds < 60)
                return $"{seconds:F0}s";
            if (seconds < 3600)
                return $"{(int)(seconds / 60)}:{(int)(seconds % 60):D2}";
            return $"{(int)(seconds / 3600)}:{(int)((seconds % 3600) / 60):D2}:{(int)(seconds % 60):D2}";
        }

        private string allLinksPhpBB = "";
        private string allLinksMarkdown = "";
        private Button copyRinBtn;
        private Button copyDiscordBtn;

        /// <summary>
        /// Shows Copy All buttons after processing completes
        /// </summary>
        public void ShowCopyAllButton(string phpBBLinks)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => ShowCopyAllButton(phpBBLinks))); } catch { }
                return;
            }

            allLinksPhpBB = phpBBLinks;

            // Build markdown version from finalUrls
            var mdLinks = new List<string>();
            foreach (var kvp in finalUrls)
            {
                string gameName = Path.GetFileName(kvp.Key);
                mdLinks.Add($"[{gameName}]({kvp.Value})");
            }
            allLinksMarkdown = string.Join("\n", mdLinks);

            if (string.IsNullOrEmpty(allLinksPhpBB) && string.IsNullOrEmpty(allLinksMarkdown))
                return;

            // Create Copy for Rin button
            copyRinBtn = CreateStyledButton("Copy for Rin", new Point(15, 495), new Size(100, 40));
            copyRinBtn.BackColor = Color.FromArgb(70, 50, 0);
            copyRinBtn.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(allLinksPhpBB))
                {
                    try
                    {
                        Clipboard.SetText(allLinksPhpBB);
                        copyRinBtn.Text = "Copied! ✓";
                        var timer = new Timer { Interval = 2000 };
                        timer.Tick += (ts, te) => { timer.Stop(); timer.Dispose(); copyRinBtn.Text = "Copy for Rin"; };
                        timer.Start();
                    }
                    catch { }
                }
            };
            this.Controls.Add(copyRinBtn);

            // Create Copy for Discord button
            copyDiscordBtn = CreateStyledButton("Copy for Discord", new Point(125, 495), new Size(120, 40));
            copyDiscordBtn.BackColor = Color.FromArgb(88, 101, 242); // Discord blurple
            copyDiscordBtn.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(allLinksMarkdown))
                {
                    try
                    {
                        Clipboard.SetText(allLinksMarkdown);
                        copyDiscordBtn.Text = "Copied! ✓";
                        var timer = new Timer { Interval = 2000 };
                        timer.Tick += (ts, te) => { timer.Stop(); timer.Dispose(); copyDiscordBtn.Text = "Copy for Discord"; };
                        timer.Start();
                    }
                    catch { }
                }
            };
            this.Controls.Add(copyDiscordBtn);

            // Make sure they're on top
            copyRinBtn.BringToFront();
            copyDiscordBtn.BringToFront();
        }

        /// <summary>
        /// Track if batch processing is active
        /// </summary>
        public bool IsProcessing { get; private set; }

        /// <summary>
        /// Update the form title with progress percentage
        /// </summary>
        public void UpdateTitleProgress(int percent, string phase = null)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => UpdateTitleProgress(percent, phase))); } catch { }
                return;
            }

            string title = $"Batch Processing - {percent}%";
            if (!string.IsNullOrEmpty(phase))
                title += $" ({phase})";
            this.Text = title;
        }

        /// <summary>
        /// Disables checkboxes and buttons during processing
        /// </summary>
        public void SetProcessingMode(bool processing)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => SetProcessingMode(processing))); } catch { }
                return;
            }

            IsProcessing = processing;

            // Disable/enable grid interactions
            gameGrid.ReadOnly = processing;

            // Hide count label and buttons during processing
            var countLabel = this.Controls["countLabel"] as Label;
            if (countLabel != null) countLabel.Visible = !processing;
            if (compressionLabel != null) compressionLabel.Visible = !processing;

            // Find and disable/enable buttons
            foreach (Control c in this.Controls)
            {
                if (c is Button btn)
                {
                    btn.Visible = !processing;
                }
            }
        }

        private void SetAllCheckboxes(string columnName, bool value)
        {
            foreach (DataGridViewRow row in gameGrid.Rows)
            {
                row.Cells[columnName].Value = value;

                // If enabling Upload, also enable Zip (required for 1fichier)
                // Note: Crack is now independent - Zip/Upload don't require Crack
                if (value && columnName == "Upload")
                {
                    row.Cells["Zip"].Value = true;
                }
            }
            UpdateCountLabel();
        }

        private void UpdateCountLabel()
        {
            int crackCount = 0;
            int zipCount = 0;
            int uploadCount = 0;

            foreach (DataGridViewRow row in gameGrid.Rows)
            {
                if ((bool)(row.Cells["Crack"].Value ?? false)) crackCount++;
                if ((bool)(row.Cells["Zip"].Value ?? false)) zipCount++;
                if ((bool)(row.Cells["Upload"].Value ?? false)) uploadCount++;
            }

            var countLabel = this.Controls["countLabel"] as Label;
            if (countLabel != null)
            {
                string text = $"{crackCount} crack";
                if (zipCount > 0) text += $", {zipCount} zip";
                if (uploadCount > 0) text += $", {uploadCount} upload";
                countLabel.Text = text;
            }
        }

        private void OpenCompressionSettings()
        {
            using (var form = new CompressionSettingsForm())
            {
                form.Owner = this;
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    CompressionFormat = form.SelectedFormat;
                    CompressionLevel = form.SelectedLevel;
                    UseRinPassword = form.UseRinPassword;
                    UpdateCompressionLabel();
                }
            }
        }

        private void UpdateCompressionLabel()
        {
            string levelDesc;
            switch (CompressionLevel)
            {
                case "0": levelDesc = "No compression"; break;
                case "5": levelDesc = "Medium"; break;
                case "10": levelDesc = "Ultra"; break;
                default: levelDesc = $"Level {CompressionLevel}"; break;
            }
            compressionLabel.Text = $"{CompressionFormat} {levelDesc}";
        }

        private string GetFolderSizeString(string folderPath)
        {
            try
            {
                long size = GetFolderSize(folderPath);
                return FormatFileSize(size);
            }
            catch
            {
                return "?";
            }
        }

        private long GetFolderSize(string folderPath)
        {
            long size = 0;
            try
            {
                foreach (var file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        size += new FileInfo(file).Length;
                    }
                    catch { }
                }
            }
            catch { }
            return size;
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double size = bytes;
            while (size >= 1024 && i < suffixes.Length - 1)
            {
                size /= 1024;
                i++;
            }
            return $"{size:0.#} {suffixes[i]}";
        }

        private string DetectAppId(string gamePath)
        {
            try
            {
                // Normalize path
                gamePath = gamePath.TrimEnd('\\', '/');

                // Try manifest first
                var manifestInfo = SteamManifestParser.GetAppIdFromManifest(gamePath);
                if (manifestInfo.HasValue)
                {
                    return manifestInfo.Value.appId;
                }

                // Try steam_appid.txt in game folder
                var appIdFile = Path.Combine(gamePath, "steam_appid.txt");
                if (File.Exists(appIdFile))
                {
                    string content = File.ReadAllText(appIdFile).Trim();
                    if (!string.IsNullOrEmpty(content) && content.All(char.IsDigit))
                    {
                        return content;
                    }
                }

                // Try steam_settings folder
                var steamSettingsAppId = Path.Combine(gamePath, "steam_settings", "steam_appid.txt");
                if (File.Exists(steamSettingsAppId))
                {
                    string content = File.ReadAllText(steamSettingsAppId).Trim();
                    if (!string.IsNullOrEmpty(content) && content.All(char.IsDigit))
                    {
                        return content;
                    }
                }

                // Try to find appmanifest directly by scanning steamapps folder
                string steamappsPath = null;
                var dir = new DirectoryInfo(gamePath);
                while (dir != null)
                {
                    if (dir.Name.Equals("steamapps", StringComparison.OrdinalIgnoreCase))
                    {
                        steamappsPath = dir.FullName;
                        break;
                    }
                    if (dir.Name.Equals("common", StringComparison.OrdinalIgnoreCase) && dir.Parent != null)
                    {
                        steamappsPath = dir.Parent.FullName;
                        break;
                    }
                    dir = dir.Parent;
                }

                if (steamappsPath != null)
                {
                    string gameFolderName = Path.GetFileName(gamePath);
                    var acfFiles = Directory.GetFiles(steamappsPath, "appmanifest_*.acf");
                    foreach (var acf in acfFiles)
                    {
                        try
                        {
                            string content = File.ReadAllText(acf);
                            // Look for installdir that matches (case insensitive, partial match)
                            if (content.IndexOf($"\"{gameFolderName}\"", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                // Extract appid from this file
                                var match = System.Text.RegularExpressions.Regex.Match(content, @"""appid""\s+""(\d+)""");
                                if (match.Success)
                                {
                                    return match.Groups[1].Value;
                                }
                            }
                        }
                        catch { }
                    }
                }

                // Try Steam Store Search API first (better fuzzy matching)
                string folderName = Path.GetFileName(gamePath);
                string apiAppId = SearchSteamStoreApi(folderName);
                if (!string.IsNullOrEmpty(apiAppId))
                {
                    return apiAppId;
                }

                // Fall back to local database search
                string foundAppId = SearchSteamDbByFolderName(folderName);
                if (!string.IsNullOrEmpty(foundAppId))
                {
                    return foundAppId;
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Searches the Steam Store API for a game by name. Has excellent fuzzy matching.
        /// Returns the AppID of the best match, or null if not found.
        /// </summary>
        private string SearchSteamStoreApi(string gameName)
        {
            try
            {
                // Clean up the search term
                string searchTerm = gameName
                    .Replace("_", " ")
                    .Replace("-", " ")
                    .Replace(".", " ")
                    .Trim();

                // URL encode the search term
                string encodedTerm = Uri.EscapeDataString(searchTerm);
                string url = $"https://store.steampowered.com/api/storesearch?term={encodedTerm}&cc=us&l=en-us";

                using (var client = new System.Net.WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    string json = client.DownloadString(url);

                    // Parse JSON response
                    var response = Newtonsoft.Json.JsonConvert.DeserializeObject<SteamStoreSearchResponse>(json);

                    if (response?.items == null || response.items.Count == 0)
                        return null;

                    // Filter out unwanted results (soundtracks, DLC, demos, etc.)
                    var filteredItems = response.items.Where(item =>
                    {
                        if (item.type != "app") return false;

                        string nameLower = item.name?.ToLower() ?? "";

                        // Filter out common non-game content
                        if (nameLower.Contains("soundtrack")) return false;
                        if (nameLower.Contains("dlc")) return false;
                        if (nameLower.Contains("demo")) return false;
                        if (nameLower.Contains("beta")) return false;
                        if (nameLower.Contains("test")) return false;
                        if (nameLower.Contains("server")) return false;
                        if (nameLower.Contains("playtest")) return false;
                        if (nameLower.Contains("dedicated")) return false;
                        if (nameLower.Contains("sdk")) return false;
                        if (nameLower.Contains("editor")) return false;
                        if (nameLower.Contains("tool")) return false;
                        if (nameLower.Contains("artbook")) return false;
                        if (nameLower.Contains("art book")) return false;
                        if (nameLower.Contains("original score")) return false;
                        if (nameLower.Contains("ost")) return false;

                        return true;
                    }).ToList();

                    if (filteredItems.Count == 0)
                    {
                        // If all results were filtered, try the first app-type result
                        var firstApp = response.items.FirstOrDefault(i => i.type == "app");
                        return firstApp?.id.ToString();
                    }

                    // Return the first (best) match
                    return filteredItems[0].id.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STEAM API] Search error for '{gameName}': {ex.Message}");
            }
            return null;
        }

        // Classes for Steam Store API response
        private class SteamStoreSearchResponse
        {
            public int total { get; set; }
            public List<SteamStoreSearchItem> items { get; set; }
        }

        private class SteamStoreSearchItem
        {
            public string type { get; set; }
            public string name { get; set; }
            public int id { get; set; }
        }

        private string SearchSteamDbByFolderName(string folderName)
        {
            try
            {
                var dataGen = new APPID.DataTableGeneration();
                var table = dataGen.DataTableToGenerate;
                if (table == null) return null;

                // Clean folder name for matching
                string searchName = folderName.Replace("_", " ").Replace("-", " ").Trim();

                // Try exact match first (case insensitive)
                foreach (DataRow row in table.Rows)
                {
                    string gameName = row[0]?.ToString() ?? "";
                    if (gameName.Equals(searchName, StringComparison.OrdinalIgnoreCase) ||
                        gameName.Equals(folderName, StringComparison.OrdinalIgnoreCase))
                    {
                        return row[1]?.ToString();
                    }
                }

                // Try contains match - folder name in game name or vice versa
                foreach (DataRow row in table.Rows)
                {
                    string gameName = row[0]?.ToString() ?? "";
                    // Normalize both for comparison
                    string normalizedGameName = gameName.Replace(":", "").Replace("-", " ").Replace("_", " ").ToLower();
                    string normalizedFolderName = folderName.Replace("_", " ").Replace("-", " ").ToLower();

                    if (normalizedGameName.Equals(normalizedFolderName) ||
                        normalizedGameName.Replace(" ", "").Equals(normalizedFolderName.Replace(" ", "")))
                    {
                        return row[1]?.ToString();
                    }
                }
            }
            catch { }
            return null;
        }

        private string ShowAppIdSearchDialog(string gameName, string currentAppId)
        {
            using (var dialog = new AppIdSearchDialog(gameName, currentAppId))
            {
                dialog.Owner = this;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    return dialog.SelectedAppId;
                }
            }
            return null; // User cancelled
        }

        private Button CreateStyledButton(string text, Point location, Size size)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = Color.FromArgb(38, 38, 42),
                ForeColor = Color.FromArgb(220, 220, 225),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 60);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 48, 52);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(28, 28, 32);

            // Round the corners
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 8, 8));

            return btn;
        }

        /// <summary>
        /// Store crack details for a game path - called by Form1 after cracking
        /// </summary>
        public void StoreCrackDetails(string gamePath, APPID.SteamAppId.CrackDetails details)
        {
            if (string.IsNullOrEmpty(gamePath) || details == null) return;
            crackDetailsMap[gamePath] = details;
        }

        /// <summary>
        /// Update zip status for a game's crack details
        /// </summary>
        public void UpdateZipStatus(string gamePath, bool success, string zipPath = null, string error = null)
        {
            if (string.IsNullOrEmpty(gamePath)) return;

            if (!crackDetailsMap.ContainsKey(gamePath))
            {
                // Create a minimal CrackDetails if one doesn't exist
                crackDetailsMap[gamePath] = new APPID.SteamAppId.CrackDetails
                {
                    GameName = Path.GetFileName(gamePath),
                    GamePath = gamePath
                };
            }

            var details = crackDetailsMap[gamePath];
            details.ZipAttempted = true;
            details.ZipSuccess = success;
            details.ZipPath = zipPath;
            details.ZipError = error;
        }

        /// <summary>
        /// Update upload status for a game's crack details
        /// </summary>
        public void UpdateUploadStatus(string gamePath, bool success, string url = null, string error = null, int retryCount = 0)
        {
            if (string.IsNullOrEmpty(gamePath)) return;

            if (!crackDetailsMap.ContainsKey(gamePath))
            {
                // Create a minimal CrackDetails if one doesn't exist
                crackDetailsMap[gamePath] = new APPID.SteamAppId.CrackDetails
                {
                    GameName = Path.GetFileName(gamePath),
                    GamePath = gamePath
                };
            }

            var details = crackDetailsMap[gamePath];
            details.UploadAttempted = true;
            details.UploadSuccess = success;
            details.UploadUrl = url;
            details.UploadError = error;
            details.UploadRetryCount = retryCount;
        }

        /// <summary>
        /// Show crack details in a popup dialog
        /// </summary>
        private void ShowCrackDetails(APPID.SteamAppId.CrackDetails details)
        {
            if (details == null) return;

            var detailForm = new Form
            {
                Text = $"Crack Details - {details.GameName}",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(25, 28, 40),
                ForeColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var textBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 33, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10)
            };

            // Build colored text
            textBox.Text = "";
            textBox.SelectionFont = new Font("Segoe UI", 12, FontStyle.Bold);
            textBox.SelectionColor = Color.Cyan;
            textBox.AppendText($"Crack Details for {details.GameName}\n");
            textBox.SelectionFont = new Font("Consolas", 9);
            textBox.SelectionColor = Color.Gray;
            textBox.AppendText($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
            textBox.AppendText($"Path: {details.GamePath}\n");
            textBox.AppendText($"AppID: {details.AppId}\n");
            textBox.AppendText($"Time: {details.Timestamp:yyyy-MM-dd HH:mm:ss}\n");

            textBox.SelectionColor = details.Success ? Color.LightGreen : Color.Red;
            textBox.AppendText($"Success: {(details.Success ? "✓ Yes" : "✗ No")}\n\n");

            if (details.DllsBackedUp.Count > 0)
            {
                textBox.SelectionColor = Color.Yellow;
                textBox.AppendText($"DLLs Backed Up ({details.DllsBackedUp.Count}):\n");
                textBox.SelectionColor = Color.White;
                foreach (var dll in details.DllsBackedUp)
                    textBox.AppendText($"  • {dll}\n");
                textBox.AppendText("\n");
            }

            if (details.DllsReplaced.Count > 0)
            {
                textBox.SelectionColor = Color.LightGreen;
                textBox.AppendText($"DLLs Replaced ({details.DllsReplaced.Count}):\n");
                textBox.SelectionColor = Color.White;
                foreach (var dll in details.DllsReplaced)
                    textBox.AppendText($"  • {dll}\n");
                textBox.AppendText("\n");
            }

            if (details.ExesTried.Count > 0)
            {
                textBox.SelectionColor = Color.Cyan;
                textBox.AppendText($"EXEs Scanned by Steamless ({details.ExesTried.Count}):\n");
                foreach (var exe in details.ExesTried)
                {
                    bool wasUnpacked = details.ExesUnpacked.Any(u => u.EndsWith(exe));
                    if (wasUnpacked)
                    {
                        textBox.SelectionColor = Color.LightGreen;
                        textBox.AppendText($"  • {exe} [UNPACKED - Had Steam Stub]\n");
                    }
                    else
                    {
                        textBox.SelectionColor = Color.Gray;
                        textBox.AppendText($"  • {exe} [No Steam Stub]\n");
                    }
                }
                textBox.AppendText("\n");
            }

            if (details.Errors.Count > 0)
            {
                textBox.SelectionColor = Color.Red;
                textBox.AppendText($"Errors ({details.Errors.Count}):\n");
                textBox.SelectionColor = Color.Orange;
                foreach (var err in details.Errors)
                    textBox.AppendText($"  ! {err}\n");
                textBox.AppendText("\n");
            }

            // Zip status
            if (details.ZipAttempted)
            {
                textBox.SelectionColor = Color.Gray;
                textBox.AppendText("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
                textBox.SelectionColor = details.ZipSuccess ? Color.LightGreen : Color.Red;
                textBox.AppendText($"Zip: {(details.ZipSuccess ? "Success" : "Failed")}\n");
                if (!string.IsNullOrEmpty(details.ZipPath))
                {
                    textBox.SelectionColor = Color.Gray;
                    textBox.AppendText($"  Path: {details.ZipPath}\n");
                }
                if (!string.IsNullOrEmpty(details.ZipError))
                {
                    textBox.SelectionColor = Color.Orange;
                    textBox.AppendText($"  Error: {details.ZipError}\n");
                }
                textBox.AppendText("\n");
            }

            // Upload status
            if (details.UploadAttempted)
            {
                if (!details.ZipAttempted)
                {
                    textBox.SelectionColor = Color.Gray;
                    textBox.AppendText("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
                }
                textBox.SelectionColor = details.UploadSuccess ? Color.LightGreen : Color.Red;
                textBox.AppendText($"Upload: {(details.UploadSuccess ? "Success" : "Failed")}\n");
                if (details.UploadRetryCount > 0)
                {
                    textBox.SelectionColor = Color.Yellow;
                    textBox.AppendText($"  Retries: {details.UploadRetryCount}\n");
                }
                if (!string.IsNullOrEmpty(details.UploadUrl))
                {
                    textBox.SelectionColor = Color.Cyan;
                    textBox.AppendText($"  1fichier: {details.UploadUrl}\n");
                }
                if (!string.IsNullOrEmpty(details.PyDriveUrl))
                {
                    textBox.SelectionColor = Color.LightGreen;
                    textBox.AppendText($"  PyDrive: {details.PyDriveUrl}\n");
                }
                if (!string.IsNullOrEmpty(details.UploadError))
                {
                    textBox.SelectionColor = Color.Orange;
                    textBox.AppendText($"  Error: {details.UploadError}\n");
                }
            }

            if (!details.HasAnyChanges && details.Errors.Count == 0 && !details.ZipAttempted && !details.UploadAttempted)
            {
                textBox.SelectionColor = Color.Orange;
                textBox.AppendText("\nNo modifications were made to this game.\n");
                textBox.AppendText("This could mean:\n");
                textBox.AppendText("  - No steam_api.dll or steam_api64.dll was found\n");
                textBox.AppendText("  - No Steam-protected EXEs were found\n");
            }

            // Copy button
            var copyBtn = new Button
            {
                Text = "Copy to Clipboard",
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.FromArgb(40, 45, 55),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            copyBtn.FlatAppearance.BorderColor = Color.FromArgb(60, 65, 75);
            copyBtn.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(details.GetSummary());
                    MessageBox.Show("Details copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch { }
            };

            detailForm.Controls.Add(textBox);
            detailForm.Controls.Add(copyBtn);
            detailForm.ShowDialog(this);
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void ApplyAcrylicEffect()
        {
            try
            {
                // Use the shared AcrylicHelper for consistent styling
                APPID.AcrylicHelper.ApplyAcrylic(this);
            }
            catch { }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw border
            using (var pen = new Pen(Color.FromArgb(60, 60, 65), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    }

    /// <summary>
    /// Dialog for searching and selecting an AppID
    /// </summary>
    public class AppIdSearchDialog : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private TextBox searchBox;
        private DataGridView resultsGrid;
        private DataTable steamGamesTable;

        public string SelectedAppId { get; private set; }

        public AppIdSearchDialog(string gameName, string currentAppId)
        {
            InitializeForm(gameName, currentAppId);
            this.Load += (s, e) =>
            {
                ApplyAcrylicEffect();
                LoadSteamGamesData();
                if (!string.IsNullOrEmpty(gameName))
                {
                    searchBox.Text = gameName;
                    PerformSearch(gameName);
                }
            };
            this.MouseDown += Form_MouseDown;
        }

        private void InitializeForm(string gameName, string currentAppId)
        {
            this.Text = "Search AppID";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(5, 8, 20);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // Title
            var titleLabel = new Label
            {
                Text = "Search AppID",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(15, 12),
                Size = new Size(200, 25),
                BackColor = Color.Transparent
            };
            this.Controls.Add(titleLabel);

            // Game name label
            var gameLabel = new Label
            {
                Text = $"Game: {gameName}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 180, 185),
                Location = new Point(15, 40),
                Size = new Size(400, 20),
                BackColor = Color.Transparent
            };
            this.Controls.Add(gameLabel);

            // Search box
            searchBox = new TextBox
            {
                Location = new Point(15, 65),
                Size = new Size(330, 25),
                BackColor = Color.FromArgb(25, 28, 40),
                ForeColor = Color.FromArgb(220, 255, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10)
            };
            searchBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    PerformSearch(searchBox.Text);
                }
            };
            this.Controls.Add(searchBox);

            // Search button
            var searchBtn = new Button
            {
                Text = "Search",
                Location = new Point(355, 65),
                Size = new Size(75, 25),
                BackColor = Color.FromArgb(38, 38, 42),
                ForeColor = Color.FromArgb(220, 220, 225),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            searchBtn.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 60);
            searchBtn.Click += (s, e) => PerformSearch(searchBox.Text);
            this.Controls.Add(searchBtn);

            // Results grid
            resultsGrid = new DataGridView
            {
                Location = new Point(15, 100),
                Size = new Size(415, 210),
                BackgroundColor = Color.FromArgb(15, 18, 30),
                ForeColor = Color.FromArgb(220, 255, 255),
                GridColor = Color.FromArgb(40, 40, 45),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Font = new Font("Segoe UI", 9)
            };

            resultsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 28, 40);
            resultsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(150, 200, 255);
            resultsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            resultsGrid.ColumnHeadersHeight = 28;
            resultsGrid.DefaultCellStyle.BackColor = Color.FromArgb(15, 18, 30);
            resultsGrid.DefaultCellStyle.ForeColor = Color.FromArgb(220, 255, 255);
            resultsGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 80, 120);
            resultsGrid.RowTemplate.Height = 24;

            resultsGrid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    SelectCurrentRow();
                }
            };
            this.Controls.Add(resultsGrid);

            // Manual AppID entry
            var manualLabel = new Label
            {
                Text = "Or enter AppID manually:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(140, 140, 145),
                Location = new Point(15, 320),
                Size = new Size(160, 20),
                BackColor = Color.Transparent
            };
            this.Controls.Add(manualLabel);

            var manualBox = new TextBox
            {
                Location = new Point(175, 318),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(25, 28, 40),
                ForeColor = Color.FromArgb(220, 255, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                Text = currentAppId
            };
            this.Controls.Add(manualBox);

            // OK button
            var okBtn = new Button
            {
                Text = "OK",
                Location = new Point(290, 315),
                Size = new Size(65, 30),
                BackColor = Color.FromArgb(0, 100, 70),
                ForeColor = Color.FromArgb(220, 220, 225),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            okBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 80, 60);
            okBtn.Click += (s, e) =>
            {
                // Use manual entry if provided, otherwise use selected row
                if (!string.IsNullOrWhiteSpace(manualBox.Text) && manualBox.Text.All(char.IsDigit))
                {
                    SelectedAppId = manualBox.Text.Trim();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else if (resultsGrid.SelectedRows.Count > 0)
                {
                    SelectCurrentRow();
                }
                else
                {
                    MessageBox.Show("Please select a game from the results or enter an AppID manually.",
                        "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            this.Controls.Add(okBtn);

            // Cancel button
            var cancelBtn = new Button
            {
                Text = "Cancel",
                Location = new Point(365, 315),
                Size = new Size(65, 30),
                BackColor = Color.FromArgb(38, 38, 42),
                ForeColor = Color.FromArgb(220, 220, 225),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            cancelBtn.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 60);
            cancelBtn.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(cancelBtn);
        }

        private void LoadSteamGamesData()
        {
            try
            {
                // Try to load the Steam games table from the main form's data
                var dataGen = new APPID.DataTableGeneration();
                steamGamesTable = dataGen.DataTableToGenerate;
                resultsGrid.DataSource = steamGamesTable;

                // Configure columns
                if (resultsGrid.Columns.Count >= 2)
                {
                    resultsGrid.Columns[0].HeaderText = "Name";
                    resultsGrid.Columns[0].Width = 300;
                    resultsGrid.Columns[1].HeaderText = "AppID";
                    resultsGrid.Columns[1].Width = 80;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Steam games data: {ex.Message}");
            }
        }

        private void PerformSearch(string searchTerm)
        {
            if (steamGamesTable == null) return;

            try
            {
                string clean = searchTerm.Trim()
                    .Replace("'", "''")  // Escape single quotes
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace("*", "")
                    .Replace("%", "");

                if (string.IsNullOrEmpty(clean)) return;

                // Try exact match first
                steamGamesTable.DefaultView.RowFilter = $"Name LIKE '{clean}'";
                if (steamGamesTable.DefaultView.Count > 0) return;

                // Try contains search
                string[] words = clean.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 1)
                {
                    string filter = string.Join("%' AND Name LIKE '%", words);
                    steamGamesTable.DefaultView.RowFilter = $"Name LIKE '%{filter}%'";
                    if (steamGamesTable.DefaultView.Count > 0) return;
                }

                // Simple contains
                steamGamesTable.DefaultView.RowFilter = $"Name LIKE '%{clean}%'";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
                steamGamesTable.DefaultView.RowFilter = "";
            }
        }

        private void SelectCurrentRow()
        {
            if (resultsGrid.SelectedRows.Count > 0)
            {
                var row = resultsGrid.SelectedRows[0];
                if (row.Cells.Count >= 2)
                {
                    SelectedAppId = row.Cells[1].Value?.ToString();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void ApplyAcrylicEffect()
        {
            try
            {
                APPID.AcrylicHelper.ApplyAcrylic(this);
            }
            catch { }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(60, 60, 65), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    }
}
