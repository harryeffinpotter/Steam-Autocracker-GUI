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

        private RadioButton zipRadioButton;
        private RadioButton sevenZipRadioButton;
        private TrackBar levelTrackBar;
        private Label levelDescriptionLabel;
        private CheckBox uploadCheckBox;
        private CheckBox rinCheckBox;
        private Button okButton;
        private Button cancelButton;
        private Panel sliderPanel;
        private int sliderValue = 0;
        private string gameName;
        private bool isCracked;

        public CompressionSettingsFormExtended(string gameName, bool isCracked)
        {
            this.gameName = gameName;
            this.isCracked = isCracked;
            InitializeComponent();
            InitializeTimers();
        }

        private void InitializeComponent()
        {
            this.Text = $"Compression Settings - {gameName}";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(0, 20, 50);

            // Title label
            var titleLabel = new Label();
            titleLabel.Text = $"{(isCracked ? "🎮 CRACKED" : "📦 CLEAN")} - {gameName}";
            titleLabel.Location = new Point(20, 15);
            titleLabel.Size = new Size(360, 25);
            titleLabel.ForeColor = isCracked ? Color.FromArgb(255, 200, 100) : Color.FromArgb(150, 255, 150);
            titleLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;

            // Format selection
            var formatLabel = new Label();
            formatLabel.Text = "Format:";
            formatLabel.Location = new Point(20, 50);
            formatLabel.Size = new Size(60, 25);
            formatLabel.ForeColor = Color.FromArgb(192, 255, 255);

            zipRadioButton = new RadioButton();
            zipRadioButton.Text = "ZIP (Universal)";
            zipRadioButton.Location = new Point(85, 48);
            zipRadioButton.Size = new Size(120, 20);
            zipRadioButton.ForeColor = Color.FromArgb(220, 255, 255);
            zipRadioButton.Checked = true;

            sevenZipRadioButton = new RadioButton();
            sevenZipRadioButton.Text = "7Z (Smaller)";
            sevenZipRadioButton.Location = new Point(210, 48);
            sevenZipRadioButton.Size = new Size(120, 20);
            sevenZipRadioButton.ForeColor = Color.FromArgb(220, 255, 255);

            // Compression level
            var levelLabel = new Label();
            levelLabel.Text = "Level:";
            levelLabel.Location = new Point(20, 85);
            levelLabel.Size = new Size(60, 25);
            levelLabel.ForeColor = Color.FromArgb(192, 255, 255);

            levelTrackBar = new TrackBar();
            levelTrackBar.Location = new Point(85, 83);
            levelTrackBar.Size = new Size(270, 45);
            levelTrackBar.Maximum = 10;
            levelTrackBar.Value = 0;
            levelTrackBar.TickStyle = TickStyle.Both;
            levelTrackBar.Visible = false; // Hidden, we use custom slider

            CreateCustomSlider();

            levelDescriptionLabel = new Label();
            levelDescriptionLabel.Location = new Point(85, 125);
            levelDescriptionLabel.Size = new Size(270, 20);
            levelDescriptionLabel.ForeColor = Color.FromArgb(192, 255, 255);
            levelDescriptionLabel.Text = "0 - No Compression (Instant)";
            levelDescriptionLabel.TextAlign = ContentAlignment.TopCenter;

            // Separator line
            var separator = new Panel();
            separator.Location = new Point(20, 160);
            separator.Size = new Size(350, 2);
            separator.BackColor = Color.FromArgb(50, 100, 150);

            // Upload checkbox
            uploadCheckBox = new CheckBox();
            uploadCheckBox.Text = "📤 Upload to YSG/HFP Backend (6 month expiry)";
            uploadCheckBox.Location = new Point(20, 175);
            uploadCheckBox.Size = new Size(350, 25);
            uploadCheckBox.ForeColor = Color.FromArgb(255, 200, 100);
            uploadCheckBox.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            // RIN encryption checkbox
            rinCheckBox = new CheckBox();
            rinCheckBox.Text = "🔒 This release is for RIN (encrypt with cs.rin.ru)";
            rinCheckBox.Location = new Point(20, 210);
            rinCheckBox.Size = new Size(350, 25);
            rinCheckBox.ForeColor = Color.FromArgb(100, 200, 255);
            rinCheckBox.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            // Info label
            var infoLabel = new Label();
            infoLabel.Text = "Upload creates shareable link • RIN encryption adds password";
            infoLabel.Location = new Point(20, 245);
            infoLabel.Size = new Size(350, 20);
            infoLabel.ForeColor = Color.Gray;
            infoLabel.Font = new Font("Segoe UI", 8);
            infoLabel.TextAlign = ContentAlignment.TopCenter;

            // Buttons
            okButton = new Button();
            okButton.Text = "✅ Process";
            okButton.Location = new Point(100, 280);
            okButton.Size = new Size(90, 30);
            okButton.DialogResult = DialogResult.OK;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.FlatAppearance.BorderColor = Color.FromArgb(100, 200, 100);
            okButton.BackColor = Color.FromArgb(0, 2, 10);
            okButton.ForeColor = Color.FromArgb(192, 255, 255);

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Location = new Point(210, 280);
            cancelButton.Size = new Size(90, 30);
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(200, 100, 100);
            cancelButton.BackColor = Color.FromArgb(0, 2, 10);
            cancelButton.ForeColor = Color.FromArgb(192, 255, 255);

            // Add controls
            this.Controls.Add(titleLabel);
            this.Controls.Add(formatLabel);
            this.Controls.Add(zipRadioButton);
            this.Controls.Add(sevenZipRadioButton);
            this.Controls.Add(levelLabel);
            this.Controls.Add(levelTrackBar);
            this.Controls.Add(levelDescriptionLabel);
            this.Controls.Add(separator);
            this.Controls.Add(uploadCheckBox);
            this.Controls.Add(rinCheckBox);
            this.Controls.Add(infoLabel);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
        }

        private void CreateCustomSlider()
        {
            sliderPanel = new Panel();
            sliderPanel.Location = new Point(85, 83);
            sliderPanel.Size = new Size(270, 40);
            sliderPanel.BackColor = Color.Transparent;

            sliderPanel.Paint += SliderPanel_Paint;
            sliderPanel.MouseDown += SliderPanel_MouseDown;
            sliderPanel.MouseMove += SliderPanel_MouseMove;
            this.Controls.Add(sliderPanel);
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