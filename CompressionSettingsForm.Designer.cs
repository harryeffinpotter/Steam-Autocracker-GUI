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
            this.titleLabel = new System.Windows.Forms.Label();
            this.zipRadioButton = new System.Windows.Forms.RadioButton();
            this.sevenZipRadioButton = new System.Windows.Forms.RadioButton();
            this.levelTrackBar = new System.Windows.Forms.TrackBar();
            this.levelDescriptionLabel = new System.Windows.Forms.Label();
            this.rememberCheckBox = new System.Windows.Forms.CheckBox();
            this.ctrlSLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.levelTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.titleLabel.Location = new System.Drawing.Point(114, 15);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(153, 19);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Compression Settings";
            // 
            // zipRadioButton
            // 
            this.zipRadioButton.AutoSize = true;
            this.zipRadioButton.Checked = true;
            this.zipRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.zipRadioButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.zipRadioButton.Location = new System.Drawing.Point(52, 53);
            this.zipRadioButton.Name = "zipRadioButton";
            this.zipRadioButton.Size = new System.Drawing.Size(94, 17);
            this.zipRadioButton.TabIndex = 2;
            this.zipRadioButton.TabStop = true;
            this.zipRadioButton.Text = "ZIP (Universal)";
            this.zipRadioButton.UseVisualStyleBackColor = true;
            // 
            // sevenZipRadioButton
            // 
            this.sevenZipRadioButton.AutoSize = true;
            this.sevenZipRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sevenZipRadioButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.sevenZipRadioButton.Location = new System.Drawing.Point(171, 53);
            this.sevenZipRadioButton.Name = "sevenZipRadioButton";
            this.sevenZipRadioButton.Size = new System.Drawing.Size(158, 17);
            this.sevenZipRadioButton.TabIndex = 3;
            this.sevenZipRadioButton.Text = "7Z (Smaller file, needs 7-Zip)";
            this.sevenZipRadioButton.UseVisualStyleBackColor = true;
            // 
            // levelTrackBar
            // 
            this.levelTrackBar.Location = new System.Drawing.Point(23, 91);
            this.levelTrackBar.Name = "levelTrackBar";
            this.levelTrackBar.Size = new System.Drawing.Size(334, 45);
            this.levelTrackBar.TabIndex = 5;
            this.levelTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.levelTrackBar.Scroll += new System.EventHandler(this.levelTrackBar_Scroll);
            // 
            // levelDescriptionLabel
            // 
            this.levelDescriptionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.levelDescriptionLabel.Location = new System.Drawing.Point(55, 139);
            this.levelDescriptionLabel.Name = "levelDescriptionLabel";
            this.levelDescriptionLabel.Size = new System.Drawing.Size(270, 20);
            this.levelDescriptionLabel.TabIndex = 10;
            this.levelDescriptionLabel.Text = "0 - No Compression (Instant)";
            this.levelDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // rememberCheckBox
            // 
            this.rememberCheckBox.AutoSize = true;
            this.rememberCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.rememberCheckBox.Location = new System.Drawing.Point(255, 227);
            this.rememberCheckBox.Name = "rememberCheckBox";
            this.rememberCheckBox.Size = new System.Drawing.Size(128, 17);
            this.rememberCheckBox.TabIndex = 6;
            this.rememberCheckBox.Text = "Remember my choice";
            this.rememberCheckBox.UseVisualStyleBackColor = true;
            // 
            // ctrlSLabel
            // 
            this.ctrlSLabel.AutoSize = true;
            this.ctrlSLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.ctrlSLabel.ForeColor = System.Drawing.Color.Gray;
            this.ctrlSLabel.Location = new System.Drawing.Point(0, 228);
            this.ctrlSLabel.Name = "ctrlSLabel";
            this.ctrlSLabel.Size = new System.Drawing.Size(193, 13);
            this.ctrlSLabel.TabIndex = 7;
            this.ctrlSLabel.Text = "Ctrl+S to open this window anytime";
            // 
            // okButton
            // 
            this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(2)))), ((int)(((byte)(10)))));
            this.okButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.okButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(10)))), ((int)(((byte)(35)))));
            this.okButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(50)))));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.okButton.Location = new System.Drawing.Point(105, 175);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(80, 30);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "ZIP IT";
            this.okButton.UseVisualStyleBackColor = false;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(2)))), ((int)(((byte)(10)))));
            this.cancelButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(10)))), ((int)(((byte)(35)))));
            this.cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(20)))), ((int)(((byte)(50)))));
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cancelButton.Location = new System.Drawing.Point(195, 175);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(80, 30);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // CompressionSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(2)))), ((int)(((byte)(10)))));
            this.ClientSize = new System.Drawing.Size(380, 245);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.ctrlSLabel);
            this.Controls.Add(this.rememberCheckBox);
            this.Controls.Add(this.levelDescriptionLabel);
            this.Controls.Add(this.levelTrackBar);
            this.Controls.Add(this.sevenZipRadioButton);
            this.Controls.Add(this.zipRadioButton);
            this.Controls.Add(this.titleLabel);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CompressionSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Compression Settings";
            ((System.ComponentModel.ISupportInitialize)(this.levelTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.RadioButton zipRadioButton;
        private System.Windows.Forms.RadioButton sevenZipRadioButton;
        private System.Windows.Forms.TrackBar levelTrackBar;
        private System.Windows.Forms.Label levelDescriptionLabel;
        private System.Windows.Forms.CheckBox rememberCheckBox;
        private System.Windows.Forms.Label ctrlSLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}