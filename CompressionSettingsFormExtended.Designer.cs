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
            titleLabel = new Label();
            formatLabel = new Label();
            zipRadioButton = new RadioButton();
            sevenZipRadioButton = new RadioButton();
            levelTrackBar = new TrackBar();
            levelDescriptionLabel = new Label();
            separator = new Panel();
            sliderPanel = new Panel();
            uploadCheckBox = new CheckBox();
            rinCheckBox = new CheckBox();
            infoLabel = new Label();
            okButton = new Button();
            cancelButton = new Button();
            ((System.ComponentModel.ISupportInitialize)levelTrackBar).BeginInit();
            SuspendLayout();
            // 
            // titleLabel
            // 
            titleLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            titleLabel.Location = new Point(13, 15);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(360, 25);
            titleLabel.TabIndex = 0;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // formatLabel
            // 
            formatLabel.ForeColor = Color.FromArgb(192, 255, 255);
            formatLabel.Location = new Point(38, 50);
            formatLabel.Name = "formatLabel";
            formatLabel.Size = new Size(60, 25);
            formatLabel.TabIndex = 1;
            formatLabel.Text = "Format:";
            // 
            // zipRadioButton
            // 
            zipRadioButton.Checked = true;
            zipRadioButton.ForeColor = Color.FromArgb(220, 255, 255);
            zipRadioButton.Location = new Point(103, 48);
            zipRadioButton.Name = "zipRadioButton";
            zipRadioButton.Size = new Size(120, 20);
            zipRadioButton.TabIndex = 2;
            zipRadioButton.TabStop = true;
            zipRadioButton.Text = "ZIP (Universal)";
            // 
            // sevenZipRadioButton
            // 
            sevenZipRadioButton.ForeColor = Color.FromArgb(220, 255, 255);
            sevenZipRadioButton.Location = new Point(228, 48);
            sevenZipRadioButton.Name = "sevenZipRadioButton";
            sevenZipRadioButton.Size = new Size(120, 20);
            sevenZipRadioButton.TabIndex = 3;
            sevenZipRadioButton.Text = "7Z (Smaller)";
            // 
            // levelTrackBar
            // 
            levelTrackBar.Location = new Point(58, 83);
            levelTrackBar.Name = "levelTrackBar";
            levelTrackBar.Size = new Size(270, 45);
            levelTrackBar.TabIndex = 5;
            levelTrackBar.TickStyle = TickStyle.Both;
            levelTrackBar.Visible = false;
            // 
            // levelDescriptionLabel
            // 
            levelDescriptionLabel.ForeColor = Color.FromArgb(192, 255, 255);
            levelDescriptionLabel.Location = new Point(58, 125);
            levelDescriptionLabel.Name = "levelDescriptionLabel";
            levelDescriptionLabel.Size = new Size(270, 20);
            levelDescriptionLabel.TabIndex = 7;
            levelDescriptionLabel.Text = "0 - No Compression (Instant)";
            levelDescriptionLabel.TextAlign = ContentAlignment.TopCenter;
            // 
            // separator
            // 
            separator.BackColor = Color.FromArgb(50, 100, 150);
            separator.Location = new Point(18, 160);
            separator.Name = "separator";
            separator.Size = new Size(350, 2);
            separator.TabIndex = 8;
            // 
            // sliderPanel
            // 
            sliderPanel.BackColor = Color.Transparent;
            sliderPanel.Location = new Point(58, 83);
            sliderPanel.Name = "sliderPanel";
            sliderPanel.Size = new Size(270, 40);
            sliderPanel.TabIndex = 6;
            sliderPanel.Paint += SliderPanel_Paint;
            sliderPanel.MouseDown += SliderPanel_MouseDown;
            sliderPanel.MouseMove += SliderPanel_MouseMove;
            // 
            // uploadCheckBox
            // 
            uploadCheckBox.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            uploadCheckBox.ForeColor = Color.FromArgb(255, 200, 100);
            uploadCheckBox.Location = new Point(50, 175);
            uploadCheckBox.Name = "uploadCheckBox";
            uploadCheckBox.Size = new Size(314, 25);
            uploadCheckBox.TabIndex = 9;
            uploadCheckBox.Text = "Upload to YSG/HFP Backend (3 month expiry)";
            // 
            // rinCheckBox
            // 
            rinCheckBox.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rinCheckBox.ForeColor = Color.FromArgb(100, 200, 255);
            rinCheckBox.Location = new Point(50, 200);
            rinCheckBox.Name = "rinCheckBox";
            rinCheckBox.Size = new Size(294, 25);
            rinCheckBox.TabIndex = 10;
            rinCheckBox.Text = "This release is for RIN (encrypt with cs.rin.ru)";
            // 
            // infoLabel
            // 
            infoLabel.Font = new Font("Segoe UI", 8F);
            infoLabel.ForeColor = Color.Gray;
            infoLabel.Location = new Point(18, 233);
            infoLabel.Name = "infoLabel";
            infoLabel.Size = new Size(350, 20);
            infoLabel.TabIndex = 11;
            infoLabel.Text = "Upload creates shareable link • RIN encryption adds password";
            infoLabel.TextAlign = ContentAlignment.TopCenter;
            // 
            // okButton
            // 
            okButton.BackColor = Color.FromArgb(0, 2, 10);
            okButton.DialogResult = DialogResult.OK;
            okButton.FlatAppearance.BorderColor = Color.FromArgb(100, 200, 100);
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.ForeColor = Color.FromArgb(192, 255, 255);
            okButton.Location = new Point(93, 280);
            okButton.Name = "okButton";
            okButton.Size = new Size(90, 30);
            okButton.TabIndex = 12;
            okButton.Text = "✓ Process";
            okButton.UseVisualStyleBackColor = false;
            // 
            // cancelButton
            // 
            cancelButton.BackColor = Color.FromArgb(0, 2, 10);
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(200, 100, 100);
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.ForeColor = Color.FromArgb(192, 255, 255);
            cancelButton.Location = new Point(203, 280);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(90, 30);
            cancelButton.TabIndex = 13;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = false;
            // 
            // CompressionSettingsFormExtended
            // 
            BackColor = Color.FromArgb(0, 20, 50);
            ClientSize = new Size(386, 322);
            Controls.Add(titleLabel);
            Controls.Add(formatLabel);
            Controls.Add(zipRadioButton);
            Controls.Add(sevenZipRadioButton);
            Controls.Add(levelTrackBar);
            Controls.Add(sliderPanel);
            Controls.Add(levelDescriptionLabel);
            Controls.Add(separator);
            Controls.Add(uploadCheckBox);
            Controls.Add(rinCheckBox);
            Controls.Add(infoLabel);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CompressionSettingsFormExtended";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Compression Settings";
            ((System.ComponentModel.ISupportInitialize)levelTrackBar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
