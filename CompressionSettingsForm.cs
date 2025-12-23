using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SteamAutocrackGUI
{
    public partial class CompressionSettingsForm : Form
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref APPID.WindowCompositionAttribData data);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        public string SelectedFormat { get; set; }
        public string SelectedLevel { get; set; }
        public bool RememberChoice { get; set; }
        public bool UseRinPassword { get; set; }
        public static long UploadBandwidthLimitBytesPerSecond { get; private set; } = 0; // 0 = unlimited
        private Panel sliderPanel;
        private int sliderValue = 0;
        private TextBox bandwidthTextBox;
        private ComboBox bandwidthUnitCombo;

        public CompressionSettingsForm()
        {
            InitializeComponent();

            // Enable dragging the form
            this.MouseDown += CompressionSettingsForm_MouseDown;

            // Set icon from resources
            try
            {
                this.Icon = APPID.Properties.Resources.sac_icon;
            }
            catch { }

            // Load saved format
            string savedFormat = APPID.Properties.Settings.Default.CompressionFormat ?? "ZIP";
            zipRadioButton.Checked = savedFormat == "ZIP";
            sevenZipRadioButton.Checked = savedFormat == "7Z";

            // Load saved password setting and set checkbox state
            UseRinPassword = APPID.AppSettings.Default.UseRinPassword;
            rinPasswordCheckBox.Checked = UseRinPassword;

            // Add Convert to DDL checkbox
            var convertToDdlCheckBox = new CheckBox
            {
                Location = new Point(89, 92),
                Size = new Size(265, 21),
                Text = "Convert to DDL (debrid backend)",
                ForeColor = Color.FromArgb(150, 150, 155),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9),
                Checked = !APPID.Properties.Settings.Default.SkipPyDriveConversion
            };
            convertToDdlCheckBox.CheckedChanged += (s, e) =>
            {
                APPID.Properties.Settings.Default.SkipPyDriveConversion = !convertToDdlCheckBox.Checked;
                APPID.Properties.Settings.Default.Save();
            };
            var tooltip = new ToolTip();
            tooltip.SetToolTip(convertToDdlCheckBox, "Convert 1fichier links to direct download links via debrid proxy");
            this.Controls.Add(convertToDdlCheckBox);

            // Hide the default trackbar and create custom slider
            levelTrackBar.Visible = false;

            // Load saved compression level
            sliderValue = APPID.Properties.Settings.Default.CompressionLevel;
            if (sliderValue < 0) sliderValue = 0;
            if (sliderValue > 10) sliderValue = 10;

            CreateCustomSlider();
            UpdateLevelDescription();

            // Add bandwidth limiter UI - below checkboxes, above slider
            var bandwidthLabel = new Label
            {
                Location = new Point(89, 118),
                Size = new Size(90, 21),
                Text = "Upload Limit:",
                ForeColor = Color.FromArgb(150, 150, 155),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(bandwidthLabel);

            bandwidthTextBox = new TextBox
            {
                Location = new Point(180, 115),
                Size = new Size(55, 23),
                BackColor = Color.FromArgb(20, 22, 28),
                ForeColor = Color.FromArgb(220, 255, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                Text = ""
            };
            var bwTooltip = new ToolTip();
            bwTooltip.SetToolTip(bandwidthTextBox, "Enter number or leave empty for unlimited");
            this.Controls.Add(bandwidthTextBox);

            bandwidthUnitCombo = new ComboBox
            {
                Location = new Point(240, 114),
                Size = new Size(70, 23),
                BackColor = Color.FromArgb(20, 22, 28),
                ForeColor = Color.FromArgb(220, 255, 255),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            bandwidthUnitCombo.Items.AddRange(new[] { "Kbps", "Mbps", "Gbps" });
            bandwidthUnitCombo.SelectedIndex = 1; // Default to Mbps
            this.Controls.Add(bandwidthUnitCombo);

            // Load saved bandwidth setting
            LoadBandwidthSetting();

            // Apply rounded corners to buttons
            ApplyRoundedCornersToButton(okButton);
            ApplyRoundedCornersToButton(cancelButton);

            // Apply acrylic effect
            this.Load += (s, e) =>
            {
                ApplyAcrylicEffect();
            };

            // Auto-close when clicking back to parent form only
            this.Deactivate += (s, e) =>
            {
                // Only close if the parent form is being activated, not other windows
                if (this.Owner != null && Form.ActiveForm == this.Owner)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };

            // ESC key closes form and returns to caller
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    e.Handled = true;
                }
            };
        }

        private void ApplyAcrylicEffect()
        {
            APPID.AcrylicHelper.ApplyAcrylic(this, roundedCorners: true, disableShadow: true);
        }

        private void ApplyRoundedCornersToButton(Button btn)
        {
            // Make button background transparent so we can draw custom
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            // Enable transparency support
            typeof(Button).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, btn, new object[] { true });

            // Disable focus cues to prevent orange highlighting
            typeof(Button).InvokeMember("SetStyle",
                System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, btn, new object[] { System.Windows.Forms.ControlStyles.Selectable, false });

            // Paint event for rounded corners
            btn.Paint += (sender, e) =>
            {
                Button b = sender as Button;

                // DISABLED - This causes deadlocks when updating buttons from background threads
                // Draw the parent's background in the button area first for true transparency
                /*if (b.Parent != null)
                {
                    using (var bmp = new Bitmap(b.Parent.Width, b.Parent.Height))
                    {
                        b.Parent.DrawToBitmap(bmp, new Rectangle(0, 0, b.Parent.Width, b.Parent.Height));
                        e.Graphics.DrawImage(bmp, new Rectangle(0, 0, b.Width, b.Height),
                            new Rectangle(b.Left, b.Top, b.Width, b.Height), GraphicsUnit.Pixel);
                    }
                }*/

                // Enable maximum quality anti-aliasing
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Create rounded rectangle that fits perfectly - subtle modern look
                int radius = 8;
                Rectangle rect = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

                int diameter = radius * 2;
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                // Determine colors based on state
                Color bgColor = Color.FromArgb(38, 38, 42);
                if (b.ClientRectangle.Contains(b.PointToClient(Cursor.Position)))
                {
                    bgColor = Color.FromArgb(50, 50, 55); // Lighter on hover
                }

                // Draw background
                using (var brush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Draw border
                using (var pen = new Pen(Color.FromArgb(55, 55, 60), 1.5f))
                {
                    e.Graphics.DrawPath(pen, path);
                }

                // Draw text
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle,
                    Color.FromArgb(220, 220, 225),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            // Refresh on mouse enter/leave for hover effect
            btn.MouseEnter += (s, e) => btn.Invalidate();
            btn.MouseLeave += (s, e) => btn.Invalidate();
        }

        private void CreateCustomSlider()
        {
            sliderPanel = new Panel
            {
                Location = new Point(49, 155),
                Size = new Size(350, 40),
                BackColor = Color.Transparent
            };

            sliderPanel.Paint += SliderPanel_Paint;
            sliderPanel.MouseDown += SliderPanel_MouseDown;
            sliderPanel.MouseMove += SliderPanel_MouseMove;
            this.Controls.Add(sliderPanel);
        }

        private void SliderPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            int trackY = 20;
            int trackStart = 10;
            int trackEnd = 340;
            int trackWidth = trackEnd - trackStart;

            // Glass track background with blur effect
            Rectangle trackRect = new Rectangle(trackStart - 3, trackY - 3, trackWidth + 6, 6);
            using (var glassPath = new GraphicsPath())
            {
                glassPath.AddRectangle(trackRect);

                // Frosted glass background
                using (var glassBrush = new LinearGradientBrush(
                    trackRect,
                    Color.FromArgb(15, 192, 255, 255),
                    Color.FromArgb(5, 192, 255, 255),
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(glassBrush, glassPath);
                }

                // Glass border
                using (var borderPen = new Pen(Color.FromArgb(30, 192, 255, 255), 1))
                {
                    g.DrawPath(borderPen, glassPath);
                }
            }

            // Progress fill with glass effect
            int fillWidth = (int)(sliderValue / 10.0 * trackWidth);
            if (fillWidth > 0)
            {
                Rectangle fillRect = new Rectangle(trackStart, trackY - 2, fillWidth, 4);

                // Neon glow beneath
                using (var glowBrush = new LinearGradientBrush(
                    new Rectangle(trackStart, trackY - 8, fillWidth, 16),
                    Color.Transparent,
                    Color.FromArgb(20, 0, 255, 255),
                    LinearGradientMode.Vertical))
                {
                    glowBrush.SetSigmaBellShape(0.5f);
                    g.FillRectangle(glowBrush, trackStart, trackY - 8, fillWidth, 16);
                }

                // Semi-transparent gradient fill
                using (var fillPath = new GraphicsPath())
                {
                    fillPath.AddRectangle(fillRect);

                    using (var fillBrush = new LinearGradientBrush(
                        new Point(trackStart, 0),
                        new Point(trackStart + fillWidth, 0),
                        Color.FromArgb(80, 0, 200, 255),
                        Color.FromArgb(120, 192, 255, 255)))
                    {
                        g.FillPath(fillBrush, fillPath);
                    }

                    // Glass shine overlay
                    using (var shineBrush = new LinearGradientBrush(
                        fillRect,
                        Color.FromArgb(40, 255, 255, 255),
                        Color.Transparent,
                        LinearGradientMode.Vertical))
                    {
                        shineBrush.SetSigmaBellShape(0.3f);
                        g.FillPath(shineBrush, fillPath);
                    }
                }
            }

            // Subtle tick marks with transparency
            for (int i = 0; i <= 10; i++)
            {
                int x = trackStart + (i * trackWidth / 10);
                int alpha = (i == 0 || i == 5 || i == 10) ? 60 : 25;
                using (var tickBrush = new SolidBrush(Color.FromArgb(alpha, 192, 255, 255)))
                {
                    g.FillRectangle(tickBrush, x - 1, trackY + 8, 2, 4);
                }
            }

            // Glass handle
            int handleX = trackStart + (int)(sliderValue / 10.0 * trackWidth);

            // Soft outer glow
            for (int i = 20; i > 0; i--)
            {
                using (var glowBrush = new SolidBrush(Color.FromArgb(2, 192, 255, 255)))
                {
                    g.FillEllipse(glowBrush, handleX - i/2, trackY - i/2, i, i);
                }
            }

            // Glass handle body
            Rectangle handleRect = new Rectangle(handleX - 7, trackY - 7, 14, 14);
            using (var handlePath = new GraphicsPath())
            {
                handlePath.AddEllipse(handleRect);

                // Frosted glass effect
                using (var handleBrush = new LinearGradientBrush(
                    handleRect,
                    Color.FromArgb(100, 220, 255, 255),
                    Color.FromArgb(60, 192, 255, 255),
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(handleBrush, handlePath);
                }

                // Glass rim
                using (var rimPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1))
                {
                    g.DrawPath(rimPen, handlePath);
                }

                // Inner glass highlight
                using (var highlightBrush = new PathGradientBrush(handlePath))
                {
                    highlightBrush.CenterColor = Color.FromArgb(60, 255, 255, 255);
                    highlightBrush.SurroundColors = new[] { Color.Transparent };
                    highlightBrush.FocusScales = new PointF(0.3f, 0.3f);
                    g.FillEllipse(highlightBrush, handleX - 5, trackY - 6, 10, 10);
                }
            }
        }

        private void SliderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            UpdateSliderValue(e.X);
        }

        private void SliderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                UpdateSliderValue(e.X);
            }
        }

        private void UpdateSliderValue(int mouseX)
        {
            int trackStart = 10;
            int trackWidth = 330;
            int value = (int)Math.Round(((mouseX - trackStart) / (double)trackWidth) * 10);
            sliderValue = Math.Max(0, Math.Min(10, value));
            sliderPanel.Invalidate();
            UpdateLevelDescription();
        }


        private void UpdateLevelDescription()
        {
            string description;
            switch (sliderValue)
            {
                case 0:
                    description = "0 - No Compression (Instant)";
                    break;
                case 5:
                    description = "5 - Medium Compression (Average)";
                    break;
                case 10:
                    description = "10 - Ultra Compression";
                    break;
                default:
                    description = sliderValue.ToString();
                    break;
            }
            levelDescriptionLabel.Text = description;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            SelectedFormat = zipRadioButton.Checked ? "ZIP" : "7Z";
            SelectedLevel = sliderValue.ToString();
            RememberChoice = true;
            UseRinPassword = rinPasswordCheckBox.Checked;

            // Save password preference
            APPID.AppSettings.Default.UseRinPassword = UseRinPassword;
            APPID.AppSettings.Default.Save();

            // Save compression level and format
            APPID.Properties.Settings.Default.CompressionLevel = sliderValue;
            APPID.Properties.Settings.Default.CompressionFormat = SelectedFormat;
            APPID.Properties.Settings.Default.Save();

            // Save bandwidth limit
            SaveBandwidthSetting();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void levelTrackBar_Scroll(object sender, EventArgs e)
        {
            sliderValue = levelTrackBar.Value;
            UpdateLevelDescription();
            sliderPanel.Invalidate();
        }

        private void CompressionSettingsForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void LoadBandwidthSetting()
        {
            try
            {
                string saved = APPID.Properties.Settings.Default.UploadBandwidthLimit ?? "";
                ParseBandwidthLimit(saved); // Initialize the static property

                if (string.IsNullOrWhiteSpace(saved))
                {
                    bandwidthTextBox.Text = "";
                    bandwidthUnitCombo.SelectedIndex = 1; // Mbps default
                    return;
                }

                // Parse the saved value to extract number and unit
                var match = System.Text.RegularExpressions.Regex.Match(saved.Trim().ToLower(), @"^(\d+(?:\.\d+)?)\s*(kb|kbit|kbps|mb|mbit|mbps|gb|gbit|gbps)?$");
                if (match.Success)
                {
                    bandwidthTextBox.Text = match.Groups[1].Value;
                    string unit = match.Groups[2].Value;
                    if (unit.StartsWith("k"))
                        bandwidthUnitCombo.SelectedIndex = 0; // Kbps
                    else if (unit.StartsWith("g"))
                        bandwidthUnitCombo.SelectedIndex = 2; // Gbps
                    else
                        bandwidthUnitCombo.SelectedIndex = 1; // Mbps (default)
                }
            }
            catch
            {
                bandwidthTextBox.Text = "";
                bandwidthUnitCombo.SelectedIndex = 1;
            }
        }

        private void SaveBandwidthSetting()
        {
            try
            {
                string value = "";
                if (!string.IsNullOrWhiteSpace(bandwidthTextBox.Text))
                {
                    string unit = bandwidthUnitCombo.SelectedItem?.ToString() ?? "Mbps";
                    value = bandwidthTextBox.Text.Trim() + unit;
                }
                APPID.Properties.Settings.Default.UploadBandwidthLimit = value;
                APPID.Properties.Settings.Default.Save();
                ParseBandwidthLimit(value);
            }
            catch { }
        }

        /// <summary>
        /// Parse bandwidth limit string like "100Mb", "5Gb", "500Mbit", "1Gbit" to bytes per second
        /// </summary>
        public static long ParseBandwidthLimit(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                UploadBandwidthLimitBytesPerSecond = 0;
                return 0;
            }

            input = input.Trim().ToLower();

            // Extract number and unit
            var match = System.Text.RegularExpressions.Regex.Match(input, @"^(\d+(?:\.\d+)?)\s*(mb|mbit|gb|gbit|kb|kbit|mbps|gbps|kbps)?$");
            if (!match.Success)
            {
                UploadBandwidthLimitBytesPerSecond = 0;
                return 0;
            }

            double value = double.Parse(match.Groups[1].Value);
            string unit = match.Groups[2].Value;

            // Convert to bits per second first, then to bytes per second
            double bitsPerSecond;
            switch (unit)
            {
                case "kb":
                case "kbit":
                case "kbps":
                    bitsPerSecond = value * 1000;
                    break;
                case "mb":
                case "mbit":
                case "mbps":
                case "": // Default to Mbit if no unit
                    bitsPerSecond = value * 1000 * 1000;
                    break;
                case "gb":
                case "gbit":
                case "gbps":
                    bitsPerSecond = value * 1000 * 1000 * 1000;
                    break;
                default:
                    bitsPerSecond = value * 1000 * 1000; // Default to Mbit
                    break;
            }

            // Convert bits to bytes (divide by 8)
            UploadBandwidthLimitBytesPerSecond = (long)(bitsPerSecond / 8);
            return UploadBandwidthLimitBytesPerSecond;
        }

        /// <summary>
        /// Get the per-upload limit based on number of concurrent uploads
        /// </summary>
        public static long GetPerUploadLimit(int concurrentUploads)
        {
            if (UploadBandwidthLimitBytesPerSecond <= 0 || concurrentUploads <= 0)
                return 0; // Unlimited

            return UploadBandwidthLimitBytesPerSecond / concurrentUploads;
        }
    }
}