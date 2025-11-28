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
            this.rinPasswordCheckBox = new System.Windows.Forms.CheckBox();
            this.ctrlSLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.levelTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.titleLabel.Location = new System.Drawing.Point(145, 17);
            this.titleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(153, 19);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Compression Settings";
            // 
            // zipRadioButton
            // 
            this.zipRadioButton.AutoSize = true;
            this.zipRadioButton.BackColor = System.Drawing.Color.Transparent;
            this.zipRadioButton.Checked = true;
            this.zipRadioButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(20)))));
            this.zipRadioButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.zipRadioButton.Location = new System.Drawing.Point(89, 82);
            this.zipRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.zipRadioButton.Name = "zipRadioButton";
            this.zipRadioButton.Size = new System.Drawing.Size(109, 21);
            this.zipRadioButton.TabIndex = 2;
            this.zipRadioButton.TabStop = true;
            this.zipRadioButton.Text = "ZIP (Universal)";
            this.zipRadioButton.UseVisualStyleBackColor = false;
            // 
            // sevenZipRadioButton
            // 
            this.sevenZipRadioButton.AutoSize = true;
            this.sevenZipRadioButton.BackColor = System.Drawing.Color.Transparent;
            this.sevenZipRadioButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(20)))));
            this.sevenZipRadioButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.sevenZipRadioButton.Location = new System.Drawing.Point(239, 82);
            this.sevenZipRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.sevenZipRadioButton.Name = "sevenZipRadioButton";
            this.sevenZipRadioButton.Size = new System.Drawing.Size(61, 21);
            this.sevenZipRadioButton.TabIndex = 3;
            this.sevenZipRadioButton.Text = "7Z file";
            this.sevenZipRadioButton.UseVisualStyleBackColor = false;
            // 
            // levelTrackBar
            // 
            this.levelTrackBar.Location = new System.Drawing.Point(64, 124);
            this.levelTrackBar.Margin = new System.Windows.Forms.Padding(4);
            this.levelTrackBar.Name = "levelTrackBar";
            this.levelTrackBar.Size = new System.Drawing.Size(350, 45);
            this.levelTrackBar.TabIndex = 5;
            this.levelTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.levelTrackBar.Scroll += new System.EventHandler(this.levelTrackBar_Scroll);
            // 
            // levelDescriptionLabel
            // 
            this.levelDescriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.levelDescriptionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.levelDescriptionLabel.Location = new System.Drawing.Point(64, 150);
            this.levelDescriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.levelDescriptionLabel.Name = "levelDescriptionLabel";
            this.levelDescriptionLabel.Size = new System.Drawing.Size(315, 26);
            this.levelDescriptionLabel.TabIndex = 10;
            this.levelDescriptionLabel.Text = "0 - No Compression (Instant)";
            this.levelDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // rememberCheckBox
            // 
            this.rememberCheckBox.AutoSize = true;
            this.rememberCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.rememberCheckBox.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(20)))));
            this.rememberCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.rememberCheckBox.Location = new System.Drawing.Point(239, 60);
            this.rememberCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.rememberCheckBox.Name = "rememberCheckBox";
            this.rememberCheckBox.Size = new System.Drawing.Size(115, 21);
            this.rememberCheckBox.TabIndex = 6;
            this.rememberCheckBox.Text = "Dont ask again";
            this.rememberCheckBox.UseVisualStyleBackColor = false;
            // 
            // rinPasswordCheckBox
            // 
            this.rinPasswordCheckBox.AutoSize = true;
            this.rinPasswordCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.rinPasswordCheckBox.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(20)))));
            this.rinPasswordCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.rinPasswordCheckBox.Location = new System.Drawing.Point(89, 60);
            this.rinPasswordCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.rinPasswordCheckBox.Name = "rinPasswordCheckBox";
            this.rinPasswordCheckBox.Size = new System.Drawing.Size(144, 21);
            this.rinPasswordCheckBox.TabIndex = 11;
            this.rinPasswordCheckBox.Text = "Use cs.rin password";
            this.rinPasswordCheckBox.UseVisualStyleBackColor = false;
            // 
            // ctrlSLabel
            // 
            this.ctrlSLabel.AutoSize = true;
            this.ctrlSLabel.BackColor = System.Drawing.Color.Transparent;
            this.ctrlSLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.ctrlSLabel.ForeColor = System.Drawing.Color.Gray;
            this.ctrlSLabel.Location = new System.Drawing.Point(125, 298);
            this.ctrlSLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ctrlSLabel.Name = "ctrlSLabel";
            this.ctrlSLabel.Size = new System.Drawing.Size(193, 13);
            this.ctrlSLabel.TabIndex = 7;
            this.ctrlSLabel.Text = "Ctrl+S to open this window anytime";
            // 
            // okButton
            // 
            this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(42)))));
            this.okButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(60)))));
            this.okButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(32)))));
            this.okButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(52)))));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.okButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(225)))));
            this.okButton.Location = new System.Drawing.Point(122, 234);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(93, 39);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "ZIP IT";
            this.okButton.UseVisualStyleBackColor = false;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(42)))));
            this.cancelButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(60)))));
            this.cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(32)))));
            this.cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(52)))));
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(225)))));
            this.cancelButton.Location = new System.Drawing.Point(227, 234);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(93, 39);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // CompressionSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(8)))), ((int)(((byte)(20)))));
            this.ClientSize = new System.Drawing.Size(443, 320);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.ctrlSLabel);
            this.Controls.Add(this.rinPasswordCheckBox);
            this.Controls.Add(this.rememberCheckBox);
            this.Controls.Add(this.levelDescriptionLabel);
            this.Controls.Add(this.levelTrackBar);
            this.Controls.Add(this.sevenZipRadioButton);
            this.Controls.Add(this.zipRadioButton);
            this.Controls.Add(this.titleLabel);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private System.Windows.Forms.CheckBox rinPasswordCheckBox;
        private System.Windows.Forms.Label ctrlSLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}