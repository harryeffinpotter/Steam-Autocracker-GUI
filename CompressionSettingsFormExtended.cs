using System;
using System.Drawing;
using System.Windows.Forms;

namespace SteamAutocrackGUI
{
    public partial class CompressionSettingsFormExtended : Form
    {
        public string SelectedFormat { get; set; }
        public string SelectedLevel { get; set; }
        public bool UploadToBackend { get; set; }
        public bool EncryptForRIN { get; set; }

        private int sliderValue = 0;
        private string gameName;
        private bool isCracked;

        public CompressionSettingsFormExtended(string gameName, bool isCracked)
        {
            this.gameName = gameName;
            this.isCracked = isCracked;
            InitializeComponent();
            ApplyDynamicContent();
            InitializeTimers();
        }

        /// <summary>
        /// Applies the runtime-dependent text/colors that can't live in the
        /// designer-serialized InitializeComponent.
        /// </summary>
        private void ApplyDynamicContent()
        {
            this.Text = $"Compression Settings - {gameName}";
            titleLabel.Text = $"{(isCracked ? "CRACKED" : "CLEAN")} - {gameName}";
            titleLabel.ForeColor = isCracked ? Color.FromArgb(255, 200, 100) : Color.FromArgb(150, 255, 150);
        }

        private void SliderPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int trackY = 20;
            int trackStart = 15;
            int trackEnd = 255;
            int trackWidth = trackEnd - trackStart;

            // Draw track
            using (var trackBrush = new SolidBrush(Color.FromArgb(20, 192, 255, 255)))
            {
                g.FillRectangle(trackBrush, trackStart, trackY - 2, trackWidth, 4);
            }

            // Draw fill
            int fillWidth = (int)(sliderValue / 10.0 * trackWidth);
            if (fillWidth > 0)
            {
                using (var fillBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(trackStart, 0),
                    new Point(trackEnd, 0),
                    Color.FromArgb(0, 200, 255),
                    Color.FromArgb(192, 255, 255)))
                {
                    g.FillRectangle(fillBrush, trackStart, trackY - 2, fillWidth, 4);
                }
            }

            // Draw handle
            int handleX = trackStart + (int)(sliderValue / 10.0 * trackWidth);
            using (var handleBrush = new SolidBrush(Color.FromArgb(220, 255, 255)))
            {
                g.FillEllipse(handleBrush, handleX - 6, trackY - 6, 12, 12);
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
            int value = (int)Math.Round((mouseX - 15) / 24.0);
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

        private void InitializeTimers()
        {
            UpdateLevelDescription();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            SelectedFormat = zipRadioButton.Checked ? "ZIP" : "7Z";
            SelectedLevel = sliderValue.ToString();
            UploadToBackend = uploadCheckBox.Checked;
            EncryptForRIN = rinCheckBox.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
