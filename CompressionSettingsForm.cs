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
        public string SelectedFormat { get; set; }
        public string SelectedLevel { get; set; }
        public bool RememberChoice { get; set; }
        public bool UseRinPassword { get; set; }
        private Panel sliderPanel;
        private int sliderValue = 0;

        public CompressionSettingsForm()
        {
            InitializeComponent();

            // Set icon to match main app
            try
            {
                this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            catch { }

            // Set defaults
            zipRadioButton.Checked = true;  // ZIP selected by default
            rememberCheckBox.Checked = false;

            // Load saved password setting and set checkbox state
            UseRinPassword = APPID.Properties.Settings.Default.UseRinPassword;
            rinPasswordCheckBox.Checked = UseRinPassword;

            // Hide the default trackbar and create custom slider
            levelTrackBar.Visible = false;
            CreateCustomSlider();
            UpdateLevelDescription();

            // Apply acrylic effect
            this.Load += (s, e) => ApplyAcrylicEffect();
        }

        private void ApplyAcrylicEffect()
        {
            try
            {
                // Apply rounded corners (Windows 11)
                int preference = 2; // DWMWCP_ROUND
                APPID.NativeMethods.DwmSetWindowAttribute(this.Handle, 33, ref preference, sizeof(int));
            }
            catch { }

            try
            {
                var accent = new APPID.AccentPolicy();
                accent.AccentState = 4; // ACCENT_ENABLE_ACRYLICBLURBEHIND
                accent.AccentFlags = APPID.ThemeConfig.BlurIntensity;
                accent.GradientColor = APPID.ThemeConfig.AcrylicBlurColor;

                int accentStructSize = Marshal.SizeOf(accent);
                IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new APPID.WindowCompositionAttribData();
                data.Attribute = 19; // WCA_ACCENT_POLICY
                data.SizeOfData = accentStructSize;
                data.Data = accentPtr;

                SetWindowCompositionAttribute(this.Handle, ref data);
                Marshal.FreeHGlobal(accentPtr);
            }
            catch { }
        }

        private void CreateCustomSlider()
        {
            sliderPanel = new Panel
            {
                Location = new Point(55, 95),
                Size = new Size(270, 40),
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
            int trackStart = 15;
            int trackEnd = 255;
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
            int value = (int)Math.Round((mouseX - 10) / 25.0);
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
            RememberChoice = rememberCheckBox.Checked;
            UseRinPassword = rinPasswordCheckBox.Checked;

            // Save password preference
            APPID.Properties.Settings.Default.UseRinPassword = UseRinPassword;
            APPID.Properties.Settings.Default.Save();

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


    }
}