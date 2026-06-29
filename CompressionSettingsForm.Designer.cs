namespace SteamAutocrackGUI
{
    partial class CompressionSettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            titleLabel = new System.Windows.Forms.Label();
            zipRadioButton = new System.Windows.Forms.RadioButton();
            sevenZipRadioButton = new System.Windows.Forms.RadioButton();
            levelTrackBar = new System.Windows.Forms.TrackBar();
            levelDescriptionLabel = new System.Windows.Forms.Label();
            rinPasswordCheckBox = new System.Windows.Forms.CheckBox();
            ctrlSLabel = new System.Windows.Forms.Label();
            okButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            convertToDdlCheckBox = new System.Windows.Forms.CheckBox();
            deleteZipsCheckBox = new System.Windows.Forms.CheckBox();
            bandwidthLabel = new System.Windows.Forms.Label();
            bandwidthTextBox = new System.Windows.Forms.TextBox();
            bandwidthUnitCombo = new System.Windows.Forms.ComboBox();
            zipFolderLabel = new System.Windows.Forms.Label();
            zipOutputFolderTextBox = new System.Windows.Forms.TextBox();
            browseBtn = new System.Windows.Forms.Button();
            resetBtn = new System.Windows.Forms.Button();
            sliderPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)levelTrackBar).BeginInit();
            sliderPanel.SuspendLayout();
            SuspendLayout();
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.BackColor = System.Drawing.Color.Transparent;
            titleLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            titleLabel.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
            titleLabel.Location = new System.Drawing.Point(164, 13);
            titleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(114, 19);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Default Settings";
            // 
            // zipRadioButton
            // 
            zipRadioButton.AutoSize = true;
            zipRadioButton.BackColor = System.Drawing.Color.Transparent;
            zipRadioButton.Checked = true;
            zipRadioButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 0, 20);
            zipRadioButton.ForeColor = System.Drawing.Color.FromArgb(220, 255, 255);
            zipRadioButton.Location = new System.Drawing.Point(176, 152);
            zipRadioButton.Margin = new System.Windows.Forms.Padding(4);
            zipRadioButton.Name = "zipRadioButton";
            zipRadioButton.Size = new System.Drawing.Size(42, 19);
            zipRadioButton.TabIndex = 2;
            zipRadioButton.TabStop = true;
            zipRadioButton.Text = "ZIP";
            zipRadioButton.UseVisualStyleBackColor = false;
            // 
            // sevenZipRadioButton
            // 
            sevenZipRadioButton.AutoSize = true;
            sevenZipRadioButton.BackColor = System.Drawing.Color.Transparent;
            sevenZipRadioButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 0, 20);
            sevenZipRadioButton.ForeColor = System.Drawing.Color.FromArgb(220, 255, 255);
            sevenZipRadioButton.Location = new System.Drawing.Point(220, 152);
            sevenZipRadioButton.Margin = new System.Windows.Forms.Padding(4);
            sevenZipRadioButton.Name = "sevenZipRadioButton";
            sevenZipRadioButton.Size = new System.Drawing.Size(38, 19);
            sevenZipRadioButton.TabIndex = 3;
            sevenZipRadioButton.Text = "7Z";
            sevenZipRadioButton.UseVisualStyleBackColor = false;
            // 
            // levelTrackBar
            // 
            levelTrackBar.Location = new System.Drawing.Point(0, 1);
            levelTrackBar.Margin = new System.Windows.Forms.Padding(4);
            levelTrackBar.Name = "levelTrackBar";
            levelTrackBar.Size = new System.Drawing.Size(350, 45);
            levelTrackBar.TabIndex = 5;
            levelTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            levelTrackBar.Visible = false;
            levelTrackBar.Scroll += levelTrackBar_Scroll;
            // 
            // levelDescriptionLabel
            // 
            levelDescriptionLabel.BackColor = System.Drawing.Color.Transparent;
            levelDescriptionLabel.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
            levelDescriptionLabel.Location = new System.Drawing.Point(64, 130);
            levelDescriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            levelDescriptionLabel.Name = "levelDescriptionLabel";
            levelDescriptionLabel.Size = new System.Drawing.Size(315, 23);
            levelDescriptionLabel.TabIndex = 10;
            levelDescriptionLabel.Text = "0 - No Compression (Instant)";
            levelDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // rinPasswordCheckBox
            // 
            rinPasswordCheckBox.AutoSize = true;
            rinPasswordCheckBox.BackColor = System.Drawing.Color.Transparent;
            rinPasswordCheckBox.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 0, 20);
            rinPasswordCheckBox.ForeColor = System.Drawing.Color.FromArgb(220, 255, 255);
            rinPasswordCheckBox.Location = new System.Drawing.Point(319, 39);
            rinPasswordCheckBox.Margin = new System.Windows.Forms.Padding(4);
            rinPasswordCheckBox.Name = "rinPasswordCheckBox";
            rinPasswordCheckBox.Size = new System.Drawing.Size(115, 19);
            rinPasswordCheckBox.TabIndex = 11;
            rinPasswordCheckBox.Text = "Use rin password";
            rinPasswordCheckBox.UseVisualStyleBackColor = false;
            // 
            // ctrlSLabel
            // 
            ctrlSLabel.AutoSize = true;
            ctrlSLabel.BackColor = System.Drawing.Color.Transparent;
            ctrlSLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            ctrlSLabel.ForeColor = System.Drawing.Color.Gray;
            ctrlSLabel.Location = new System.Drawing.Point(125, 222);
            ctrlSLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            ctrlSLabel.Name = "ctrlSLabel";
            ctrlSLabel.Size = new System.Drawing.Size(193, 13);
            ctrlSLabel.TabIndex = 7;
            ctrlSLabel.Text = "Ctrl+S to open this window anytime";
            // 
            // okButton
            // 
            okButton.BackColor = System.Drawing.Color.FromArgb(38, 38, 42);
            okButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(55, 55, 60);
            okButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(28, 28, 32);
            okButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(38, 38, 42);
            okButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            okButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            okButton.ForeColor = System.Drawing.Color.FromArgb(220, 220, 225);
            okButton.Location = new System.Drawing.Point(125, 180);
            okButton.Margin = new System.Windows.Forms.Padding(4);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(93, 34);
            okButton.TabIndex = 8;
            okButton.Text = "Apply";
            okButton.UseVisualStyleBackColor = false;
            okButton.Click += okButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.BackColor = System.Drawing.Color.FromArgb(38, 38, 42);
            cancelButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(55, 55, 60);
            cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(28, 28, 32);
            cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(38, 38, 42);
            cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cancelButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            cancelButton.ForeColor = System.Drawing.Color.FromArgb(220, 220, 225);
            cancelButton.Location = new System.Drawing.Point(223, 180);
            cancelButton.Margin = new System.Windows.Forms.Padding(4);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(93, 34);
            cancelButton.TabIndex = 9;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = false;
            cancelButton.Click += cancelButton_Click;
            // 
            // convertToDdlCheckBox
            // 
            convertToDdlCheckBox.BackColor = System.Drawing.Color.Transparent;
            convertToDdlCheckBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            convertToDdlCheckBox.ForeColor = System.Drawing.Color.FromArgb(220, 255, 255);
            convertToDdlCheckBox.Location = new System.Drawing.Point(12, 39);
            convertToDdlCheckBox.Name = "convertToDdlCheckBox";
            convertToDdlCheckBox.Size = new System.Drawing.Size(153, 21);
            convertToDdlCheckBox.TabIndex = 12;
            convertToDdlCheckBox.Text = "Convert to DDL (debrid)";
            convertToDdlCheckBox.UseVisualStyleBackColor = false;
            // 
            // deleteZipsCheckBox
            // 
            deleteZipsCheckBox.BackColor = System.Drawing.Color.Transparent;
            deleteZipsCheckBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            deleteZipsCheckBox.ForeColor = System.Drawing.Color.FromArgb(220, 255, 255);
            deleteZipsCheckBox.Location = new System.Drawing.Point(165, 39);
            deleteZipsCheckBox.Name = "deleteZipsCheckBox";
            deleteZipsCheckBox.Size = new System.Drawing.Size(151, 21);
            deleteZipsCheckBox.TabIndex = 13;
            deleteZipsCheckBox.Text = "Delete zips after upload";
            deleteZipsCheckBox.UseVisualStyleBackColor = false;
            // 
            // bandwidthLabel
            // 
            bandwidthLabel.BackColor = System.Drawing.Color.Transparent;
            bandwidthLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            bandwidthLabel.ForeColor = System.Drawing.Color.FromArgb(150, 150, 155);
            bandwidthLabel.Location = new System.Drawing.Point(262, 67);
            bandwidthLabel.Name = "bandwidthLabel";
            bandwidthLabel.Size = new System.Drawing.Size(54, 21);
            bandwidthLabel.TabIndex = 14;
            bandwidthLabel.Text = "UL Limit:";
            // 
            // bandwidthTextBox
            // 
            bandwidthTextBox.BackColor = System.Drawing.Color.FromArgb(20, 22, 28);
            bandwidthTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            bandwidthTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            bandwidthTextBox.ForeColor = System.Drawing.Color.FromArgb(220, 255, 255);
            bandwidthTextBox.Location = new System.Drawing.Point(318, 64);
            bandwidthTextBox.Name = "bandwidthTextBox";
            bandwidthTextBox.Size = new System.Drawing.Size(55, 23);
            bandwidthTextBox.TabIndex = 15;
            // 
            // bandwidthUnitCombo
            // 
            bandwidthUnitCombo.BackColor = System.Drawing.Color.FromArgb(20, 22, 28);
            bandwidthUnitCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            bandwidthUnitCombo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            bandwidthUnitCombo.Font = new System.Drawing.Font("Segoe UI", 9F);
            bandwidthUnitCombo.ForeColor = System.Drawing.Color.FromArgb(220, 255, 255);
            bandwidthUnitCombo.Items.AddRange(new object[] { "Kbps", "Mbps", "Gbps" });
            bandwidthUnitCombo.Location = new System.Drawing.Point(376, 64);
            bandwidthUnitCombo.Name = "bandwidthUnitCombo";
            bandwidthUnitCombo.Size = new System.Drawing.Size(55, 23);
            bandwidthUnitCombo.TabIndex = 16;
            // 
            // zipFolderLabel
            // 
            zipFolderLabel.BackColor = System.Drawing.Color.Transparent;
            zipFolderLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            zipFolderLabel.ForeColor = System.Drawing.Color.FromArgb(150, 150, 155);
            zipFolderLabel.Location = new System.Drawing.Point(10, 67);
            zipFolderLabel.Name = "zipFolderLabel";
            zipFolderLabel.Size = new System.Drawing.Size(47, 21);
            zipFolderLabel.TabIndex = 17;
            zipFolderLabel.Text = "Zip Dir:";
            // 
            // zipOutputFolderTextBox
            // 
            zipOutputFolderTextBox.BackColor = System.Drawing.Color.FromArgb(20, 22, 28);
            zipOutputFolderTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            zipOutputFolderTextBox.Font = new System.Drawing.Font("Segoe UI", 8F);
            zipOutputFolderTextBox.ForeColor = System.Drawing.Color.FromArgb(180, 180, 185);
            zipOutputFolderTextBox.Location = new System.Drawing.Point(58, 64);
            zipOutputFolderTextBox.Name = "zipOutputFolderTextBox";
            zipOutputFolderTextBox.ReadOnly = true;
            zipOutputFolderTextBox.Size = new System.Drawing.Size(142, 22);
            zipOutputFolderTextBox.TabIndex = 18;
            // 
            // browseBtn
            // 
            browseBtn.BackColor = System.Drawing.Color.FromArgb(38, 38, 42);
            browseBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(55, 55, 60);
            browseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            browseBtn.Font = new System.Drawing.Font("Segoe UI", 8F);
            browseBtn.ForeColor = System.Drawing.Color.FromArgb(200, 200, 205);
            browseBtn.Location = new System.Drawing.Point(205, 64);
            browseBtn.Name = "browseBtn";
            browseBtn.Size = new System.Drawing.Size(30, 22);
            browseBtn.TabIndex = 19;
            browseBtn.Text = "...";
            browseBtn.UseVisualStyleBackColor = false;
            // 
            // resetBtn
            // 
            resetBtn.BackColor = System.Drawing.Color.FromArgb(38, 38, 42);
            resetBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(55, 55, 60);
            resetBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            resetBtn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            resetBtn.ForeColor = System.Drawing.Color.FromArgb(200, 100, 100);
            resetBtn.Location = new System.Drawing.Point(239, 64);
            resetBtn.Name = "resetBtn";
            resetBtn.Size = new System.Drawing.Size(20, 22);
            resetBtn.TabIndex = 20;
            resetBtn.Text = "×";
            resetBtn.UseVisualStyleBackColor = false;
            // 
            // sliderPanel
            // 
            sliderPanel.BackColor = System.Drawing.Color.Transparent;
            sliderPanel.Controls.Add(levelTrackBar);
            sliderPanel.Location = new System.Drawing.Point(45, 88);
            sliderPanel.Name = "sliderPanel";
            sliderPanel.Size = new System.Drawing.Size(350, 40);
            sliderPanel.TabIndex = 21;
            sliderPanel.Paint += SliderPanel_Paint;
            sliderPanel.MouseDown += SliderPanel_MouseDown;
            sliderPanel.MouseMove += SliderPanel_MouseMove;
            // 
            // CompressionSettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(5, 8, 20);
            ClientSize = new System.Drawing.Size(443, 246);
            Controls.Add(sliderPanel);
            Controls.Add(resetBtn);
            Controls.Add(browseBtn);
            Controls.Add(zipOutputFolderTextBox);
            Controls.Add(zipFolderLabel);
            Controls.Add(bandwidthUnitCombo);
            Controls.Add(bandwidthTextBox);
            Controls.Add(bandwidthLabel);
            Controls.Add(deleteZipsCheckBox);
            Controls.Add(convertToDdlCheckBox);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(ctrlSLabel);
            Controls.Add(rinPasswordCheckBox);
            Controls.Add(levelDescriptionLabel);
            Controls.Add(sevenZipRadioButton);
            Controls.Add(zipRadioButton);
            Controls.Add(titleLabel);
            ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CompressionSettingsForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Compression Settings";
            ((System.ComponentModel.ISupportInitialize)levelTrackBar).EndInit();
            sliderPanel.ResumeLayout(false);
            sliderPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.RadioButton zipRadioButton;
        private System.Windows.Forms.RadioButton sevenZipRadioButton;
        private System.Windows.Forms.TrackBar levelTrackBar;
        private System.Windows.Forms.Label levelDescriptionLabel;
        private System.Windows.Forms.CheckBox rinPasswordCheckBox;
        private System.Windows.Forms.Label ctrlSLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox convertToDdlCheckBox;
        private System.Windows.Forms.CheckBox deleteZipsCheckBox;
        private System.Windows.Forms.Label bandwidthLabel;
        private System.Windows.Forms.TextBox bandwidthTextBox;
        private System.Windows.Forms.ComboBox bandwidthUnitCombo;
        private System.Windows.Forms.Label zipFolderLabel;
        private System.Windows.Forms.TextBox zipOutputFolderTextBox;
        private System.Windows.Forms.Button browseBtn;
        private System.Windows.Forms.Button resetBtn;
        private System.Windows.Forms.Panel sliderPanel;
    }
}
