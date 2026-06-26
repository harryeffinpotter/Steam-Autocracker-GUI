using System.Drawing;
using System.Windows.Forms;

namespace SteamAutocrackGUI
{
    partial class CompressionSettingsFormExtended
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Label titleLabel;
        private Label formatLabel;
        private RadioButton zipRadioButton;
        private RadioButton sevenZipRadioButton;
        private Label levelLabel;
        private TrackBar levelTrackBar;
        private Label levelDescriptionLabel;
        private Panel separator;
        private Panel sliderPanel;
        private CheckBox uploadCheckBox;
        private CheckBox rinCheckBox;
        private Label infoLabel;
        private Button okButton;
        private Button cancelButton;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.titleLabel = new Label();
            this.formatLabel = new Label();
            this.zipRadioButton = new RadioButton();
            this.sevenZipRadioButton = new RadioButton();
            this.levelLabel = new Label();
            this.levelTrackBar = new TrackBar();
            this.levelDescriptionLabel = new Label();
            this.separator = new Panel();
            this.sliderPanel = new Panel();
            this.uploadCheckBox = new CheckBox();
            this.rinCheckBox = new CheckBox();
            this.infoLabel = new Label();
            this.okButton = new Button();
            this.cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.levelTrackBar)).BeginInit();
            this.SuspendLayout();
            //
            // titleLabel
            //
            this.titleLabel.Location = new Point(20, 15);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new Size(360, 25);
            this.titleLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            //
            // formatLabel
            //
            this.formatLabel.Text = "Format:";
            this.formatLabel.Location = new Point(20, 50);
            this.formatLabel.Name = "formatLabel";
            this.formatLabel.Size = new Size(60, 25);
            this.formatLabel.ForeColor = Color.FromArgb(192, 255, 255);
            //
            // zipRadioButton
            //
            this.zipRadioButton.Text = "ZIP (Universal)";
            this.zipRadioButton.Location = new Point(85, 48);
            this.zipRadioButton.Name = "zipRadioButton";
            this.zipRadioButton.Size = new Size(120, 20);
            this.zipRadioButton.ForeColor = Color.FromArgb(220, 255, 255);
            this.zipRadioButton.Checked = true;
            //
            // sevenZipRadioButton
            //
            this.sevenZipRadioButton.Text = "7Z (Smaller)";
            this.sevenZipRadioButton.Location = new Point(210, 48);
            this.sevenZipRadioButton.Name = "sevenZipRadioButton";
            this.sevenZipRadioButton.Size = new Size(120, 20);
            this.sevenZipRadioButton.ForeColor = Color.FromArgb(220, 255, 255);
            //
            // levelLabel
            //
            this.levelLabel.Text = "Level:";
            this.levelLabel.Location = new Point(20, 85);
            this.levelLabel.Name = "levelLabel";
            this.levelLabel.Size = new Size(60, 25);
            this.levelLabel.ForeColor = Color.FromArgb(192, 255, 255);
            //
            // levelTrackBar
            //
            this.levelTrackBar.Location = new Point(85, 83);
            this.levelTrackBar.Name = "levelTrackBar";
            this.levelTrackBar.Size = new Size(270, 45);
            this.levelTrackBar.Maximum = 10;
            this.levelTrackBar.Value = 0;
            this.levelTrackBar.TickStyle = TickStyle.Both;
            this.levelTrackBar.Visible = false;
            //
            // sliderPanel
            //
            this.sliderPanel.Location = new Point(85, 83);
            this.sliderPanel.Name = "sliderPanel";
            this.sliderPanel.Size = new Size(270, 40);
            this.sliderPanel.BackColor = Color.Transparent;
            this.sliderPanel.Paint += this.SliderPanel_Paint;
            this.sliderPanel.MouseDown += this.SliderPanel_MouseDown;
            this.sliderPanel.MouseMove += this.SliderPanel_MouseMove;
            //
            // levelDescriptionLabel
            //
            this.levelDescriptionLabel.Location = new Point(85, 125);
            this.levelDescriptionLabel.Name = "levelDescriptionLabel";
            this.levelDescriptionLabel.Size = new Size(270, 20);
            this.levelDescriptionLabel.ForeColor = Color.FromArgb(192, 255, 255);
            this.levelDescriptionLabel.Text = "0 - No Compression (Instant)";
            this.levelDescriptionLabel.TextAlign = ContentAlignment.TopCenter;
            //
            // separator
            //
            this.separator.Location = new Point(20, 160);
            this.separator.Name = "separator";
            this.separator.Size = new Size(350, 2);
            this.separator.BackColor = Color.FromArgb(50, 100, 150);
            //
            // uploadCheckBox
            //
            this.uploadCheckBox.Text = "📤 Upload to YSG/HFP Backend (3 month expiry)";
            this.uploadCheckBox.Location = new Point(20, 175);
            this.uploadCheckBox.Name = "uploadCheckBox";
            this.uploadCheckBox.Size = new Size(350, 25);
            this.uploadCheckBox.ForeColor = Color.FromArgb(255, 200, 100);
            this.uploadCheckBox.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            //
            // rinCheckBox
            //
            this.rinCheckBox.Text = "🔒 This release is for RIN (encrypt with cs.rin.ru)";
            this.rinCheckBox.Location = new Point(20, 210);
            this.rinCheckBox.Name = "rinCheckBox";
            this.rinCheckBox.Size = new Size(350, 25);
            this.rinCheckBox.ForeColor = Color.FromArgb(100, 200, 255);
            this.rinCheckBox.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            //
            // infoLabel
            //
            this.infoLabel.Text = "Upload creates shareable link • RIN encryption adds password";
            this.infoLabel.Location = new Point(20, 245);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new Size(350, 20);
            this.infoLabel.ForeColor = Color.Gray;
            this.infoLabel.Font = new Font("Segoe UI", 8F);
            this.infoLabel.TextAlign = ContentAlignment.TopCenter;
            //
            // okButton
            //
            this.okButton.Text = "✅ Process";
            this.okButton.Location = new Point(100, 280);
            this.okButton.Name = "okButton";
            this.okButton.Size = new Size(90, 30);
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.FlatStyle = FlatStyle.Flat;
            this.okButton.FlatAppearance.BorderColor = Color.FromArgb(100, 200, 100);
            this.okButton.BackColor = Color.FromArgb(0, 2, 10);
            this.okButton.ForeColor = Color.FromArgb(192, 255, 255);
            //
            // cancelButton
            //
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Location = new Point(210, 280);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(90, 30);
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.FlatStyle = FlatStyle.Flat;
            this.cancelButton.FlatAppearance.BorderColor = Color.FromArgb(200, 100, 100);
            this.cancelButton.BackColor = Color.FromArgb(0, 2, 10);
            this.cancelButton.ForeColor = Color.FromArgb(192, 255, 255);
            //
            // CompressionSettingsFormExtended
            //
            this.Text = "Compression Settings";
            // Original used this.Size = 400x350 on a FixedDialog; the controls were
            // laid out against the resulting ~384x311 client area.
            this.ClientSize = new Size(384, 311);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(0, 20, 50);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.formatLabel);
            this.Controls.Add(this.zipRadioButton);
            this.Controls.Add(this.sevenZipRadioButton);
            this.Controls.Add(this.levelLabel);
            this.Controls.Add(this.levelTrackBar);
            this.Controls.Add(this.sliderPanel);
            this.Controls.Add(this.levelDescriptionLabel);
            this.Controls.Add(this.separator);
            this.Controls.Add(this.uploadCheckBox);
            this.Controls.Add(this.rinCheckBox);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            ((System.ComponentModel.ISupportInitialize)(this.levelTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
