namespace SteamAppIdIdentifier
{
    partial class EnhancedShareWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Panel titleBar;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.DataGridView gamesGrid;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ToolTip toolTip;

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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.gamesGrid = new System.Windows.Forms.DataGridView();
            this.SelectGame = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.GameName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstallPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GameSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BuildID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AppID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LastUpdated = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CrackOnly = new System.Windows.Forms.DataGridViewButtonColumn();
            this.ShareClean = new System.Windows.Forms.DataGridViewButtonColumn();
            this.ShareCracked = new System.Windows.Forms.DataGridViewButtonColumn();
            this.lblSelectedPrefix = new System.Windows.Forms.Label();
            this.lblSelectedCount = new System.Windows.Forms.Label();
            this.lblSelectedSuffix = new System.Windows.Forms.Label();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnProcessSelected = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.titleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gamesGrid)).BeginInit();
            this.titleBar.SuspendLayout();
            this.SuspendLayout();
            //
            // mainPanel
            //
            this.mainPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.mainPanel.Controls.Add(this.gamesGrid);
            this.mainPanel.Controls.Add(this.lblStatus);
            this.mainPanel.Controls.Add(this.progressBar);
            this.mainPanel.Controls.Add(this.titleBar);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new System.Windows.Forms.Padding(0);
            this.mainPanel.Size = new System.Drawing.Size(1200, 915);
            this.mainPanel.TabIndex = 0;
            //
            // gamesGrid
            // 
            this.gamesGrid.AllowUserToAddRows = false;
            this.gamesGrid.AllowUserToDeleteRows = false;
            this.gamesGrid.AllowUserToOrderColumns = true;
            this.gamesGrid.AllowUserToResizeRows = false;
            this.gamesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gamesGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(12)))));
            this.gamesGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gamesGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(12)))), ((int)(((byte)(18)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(18)))), ((int)(((byte)(25)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gamesGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gamesGrid.ColumnHeadersHeight = 35;
            this.gamesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gamesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SelectGame,
            this.GameName,
            this.InstallPath,
            this.GameSize,
            this.BuildID,
            this.AppID,
            this.LastUpdated,
            this.CrackOnly,
            this.ShareClean,
            this.ShareCracked});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(12)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(25)))), ((int)(((byte)(35)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gamesGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.gamesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gamesGrid.EnableHeadersVisualStyles = false;
            this.gamesGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(45)))));
            this.gamesGrid.Location = new System.Drawing.Point(1, 47);
            this.gamesGrid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gamesGrid.MultiSelect = false;
            this.gamesGrid.Name = "gamesGrid";
            this.gamesGrid.ReadOnly = true;
            this.gamesGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(12)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(25)))), ((int)(((byte)(35)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gamesGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.gamesGrid.RowHeadersVisible = false;
            this.gamesGrid.RowHeadersWidth = 62;
            this.gamesGrid.RowTemplate.Height = 30;
            this.gamesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gamesGrid.Size = new System.Drawing.Size(1165, 802);
            this.gamesGrid.TabIndex = 1;
            this.toolTip.SetToolTip(this.gamesGrid, "Click on game rows to perform actions. Click column headers to sort.");
            this.gamesGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GamesGrid_CellClick);
            this.gamesGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gamesGrid_CellContentClick);
            this.gamesGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.GamesGrid_CellFormatting);
            this.gamesGrid.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.GamesGrid_CellMouseEnter);
            this.gamesGrid.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.GamesGrid_CellMouseLeave);
            //
            // SelectGame
            //
            this.SelectGame.HeaderText = "☑";
            this.SelectGame.MinimumWidth = 30;
            this.SelectGame.Name = "SelectGame";
            this.SelectGame.Width = 35;
            this.SelectGame.FillWeight = 20;
            //
            // GameName
            //
            this.GameName.HeaderText = "Game";
            this.GameName.MinimumWidth = 8;
            this.GameName.Name = "GameName";
            this.GameName.ReadOnly = true;
            // 
            // InstallPath
            // 
            this.InstallPath.HeaderText = "Install Path";
            this.InstallPath.MinimumWidth = 8;
            this.InstallPath.Name = "InstallPath";
            this.InstallPath.ReadOnly = true;
            // 
            // GameSize
            // 
            this.GameSize.HeaderText = "Size";
            this.GameSize.MinimumWidth = 8;
            this.GameSize.Name = "GameSize";
            this.GameSize.ReadOnly = true;
            // 
            // BuildID
            // 
            this.BuildID.HeaderText = "Build";
            this.BuildID.MinimumWidth = 8;
            this.BuildID.Name = "BuildID";
            this.BuildID.ReadOnly = true;
            // 
            // AppID
            // 
            this.AppID.HeaderText = "AppID";
            this.AppID.MinimumWidth = 8;
            this.AppID.Name = "AppID";
            this.AppID.ReadOnly = true;
            //
            // LastUpdated
            //
            this.LastUpdated.HeaderText = "Last Updated";
            this.LastUpdated.MinimumWidth = 8;
            this.LastUpdated.Name = "LastUpdated";
            this.LastUpdated.ReadOnly = true;
            //
            // CrackOnly
            //
            this.CrackOnly.HeaderText = "Crack";
            this.CrackOnly.MinimumWidth = 70;
            this.CrackOnly.Name = "CrackOnly";
            this.CrackOnly.Text = "⚡ Crack";
            this.CrackOnly.UseColumnTextForButtonValue = false;
            this.CrackOnly.FillWeight = 45;
            this.CrackOnly.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            //
            // ShareClean
            //
            this.ShareClean.HeaderText = "Clean";
            this.ShareClean.MinimumWidth = 70;
            this.ShareClean.Name = "ShareClean";
            this.ShareClean.Text = "📦 Clean";
            this.ShareClean.UseColumnTextForButtonValue = false;
            this.ShareClean.FillWeight = 45;
            this.ShareClean.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            //
            // ShareCracked
            //
            this.ShareCracked.HeaderText = "Cracked";
            this.ShareCracked.MinimumWidth = 80;
            this.ShareCracked.Name = "ShareCracked";
            this.ShareCracked.Text = "🎮 Cracked";
            this.ShareCracked.UseColumnTextForButtonValue = false;
            this.ShareCracked.FillWeight = 50;
            this.ShareCracked.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            //
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.Cyan;
            this.lblStatus.Location = new System.Drawing.Point(1, 849);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1165, 26);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Visible = false;
            // 
            // progressBar
            // 
            this.progressBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(1, 875);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1165, 39);
            this.progressBar.TabIndex = 3;
            this.progressBar.Visible = false;
            // 
            // titleBar
            // 
            this.titleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(50)))), ((int)(((byte)(60)))), ((int)(((byte)(80)))));
            this.titleBar.Controls.Add(this.lblTitle);
            this.titleBar.Controls.Add(this.lblSelectedPrefix);
            this.titleBar.Controls.Add(this.lblSelectedCount);
            this.titleBar.Controls.Add(this.lblSelectedSuffix);
            this.titleBar.Controls.Add(this.btnSettings);
            this.titleBar.Controls.Add(this.btnProcessSelected);
            this.titleBar.Controls.Add(this.btnMinimize);
            this.titleBar.Controls.Add(this.btnClose);
            this.titleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.titleBar.Location = new System.Drawing.Point(1, 1);
            this.titleBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.titleBar.Name = "titleBar";
            this.titleBar.Size = new System.Drawing.Size(1165, 46);
            this.titleBar.TabIndex = 4;
            this.titleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleBar_MouseDown);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
            this.lblTitle.Location = new System.Drawing.Point(24, 14);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(155, 19);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Crack | Zip | Share";
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleBar_MouseDown);
            //
            // lblSelectedPrefix
            //
            this.lblSelectedPrefix.AutoSize = true;
            this.lblSelectedPrefix.BackColor = System.Drawing.Color.Transparent;
            this.lblSelectedPrefix.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSelectedPrefix.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(160)))));
            this.lblSelectedPrefix.Location = new System.Drawing.Point(185, 16);
            this.lblSelectedPrefix.Name = "lblSelectedPrefix";
            this.lblSelectedPrefix.Size = new System.Drawing.Size(52, 15);
            this.lblSelectedPrefix.Text = "Selected ";
            this.lblSelectedPrefix.Visible = false;
            //
            // lblSelectedCount
            //
            this.lblSelectedCount.AutoSize = true;
            this.lblSelectedCount.BackColor = System.Drawing.Color.Transparent;
            this.lblSelectedCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSelectedCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(255)))), ((int)(((byte)(150)))));
            this.lblSelectedCount.Location = new System.Drawing.Point(185, 16);
            this.lblSelectedCount.Name = "lblSelectedCount";
            this.lblSelectedCount.Size = new System.Drawing.Size(14, 15);
            this.lblSelectedCount.Text = "0";
            this.lblSelectedCount.Visible = false;
            //
            // lblSelectedSuffix
            //
            this.lblSelectedSuffix.AutoSize = true;
            this.lblSelectedSuffix.BackColor = System.Drawing.Color.Transparent;
            this.lblSelectedSuffix.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSelectedSuffix.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(160)))));
            this.lblSelectedSuffix.Location = new System.Drawing.Point(257, 16);
            this.lblSelectedSuffix.Name = "lblSelectedSuffix";
            this.lblSelectedSuffix.Size = new System.Drawing.Size(18, 15);
            this.lblSelectedSuffix.Text = " to";
            this.lblSelectedSuffix.Visible = false;
            //
            // btnSettings
            //
            this.btnSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
            this.btnSettings.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(75)))));
            this.btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
            this.btnSettings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(18)))), ((int)(((byte)(22)))));
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.Location = new System.Drawing.Point(388, 7);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(40, 33);
            this.btnSettings.TabIndex = 14;
            this.btnSettings.Text = "";
            this.toolTip.SetToolTip(this.btnSettings, "Compression settings");
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            //
            // btnProcessSelected
            //
            this.btnProcessSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(80)))), ((int)(((byte)(30)))));
            this.btnProcessSelected.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(140)))), ((int)(((byte)(60)))));
            this.btnProcessSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProcessSelected.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnProcessSelected.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(255)))), ((int)(((byte)(150)))));
            this.btnProcessSelected.Location = new System.Drawing.Point(290, 7);
            this.btnProcessSelected.Name = "btnProcessSelected";
            this.btnProcessSelected.Size = new System.Drawing.Size(90, 33);
            this.btnProcessSelected.TabIndex = 13;
            this.btnProcessSelected.Text = "Process";
            this.toolTip.SetToolTip(this.btnProcessSelected, "Process selected games with chosen actions");
            this.btnProcessSelected.UseVisualStyleBackColor = false;
            this.btnProcessSelected.Click += new System.EventHandler(this.BtnProcessSelected_Click);
            //
            // btnMinimize
            // 
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnMinimize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.btnMinimize.Location = new System.Drawing.Point(1078, 0);
            this.btnMinimize.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(41, 46);
            this.btnMinimize.TabIndex = 2;
            this.btnMinimize.Text = "−";
            this.toolTip.SetToolTip(this.btnMinimize, "Minimize this window");
            this.btnMinimize.UseVisualStyleBackColor = false;
            this.btnMinimize.Click += new System.EventHandler(this.BtnMinimize_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.btnClose.Location = new System.Drawing.Point(1125, -3);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(41, 46);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "✕";
            this.toolTip.SetToolTip(this.btnClose, "Close this window");
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // EnhancedShareWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(8)))), ((int)(((byte)(20)))));
            this.ClientSize = new System.Drawing.Size(1200, 915);
            this.Controls.Add(this.mainPanel);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "EnhancedShareWindow";
            this.Opacity = 0.95D;
            this.ShowInTaskbar = true;  // Show in taskbar since main form hides
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Game Manager - Crack, Share & Export";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EnhancedShareWindow_FormClosed);
            this.Load += new System.EventHandler(this.EnhancedShareWindow_Load);
            this.mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gamesGrid)).EndInit();
            this.titleBar.ResumeLayout(false);
            this.titleBar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridViewCheckBoxColumn SelectGame;
        private System.Windows.Forms.DataGridViewTextBoxColumn GameName;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstallPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn GameSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn BuildID;
        private System.Windows.Forms.DataGridViewTextBoxColumn AppID;
        private System.Windows.Forms.DataGridViewTextBoxColumn LastUpdated;
        private System.Windows.Forms.DataGridViewButtonColumn CrackOnly;
        private System.Windows.Forms.DataGridViewButtonColumn ShareClean;
        private System.Windows.Forms.DataGridViewButtonColumn ShareCracked;
        private System.Windows.Forms.Label lblSelectedPrefix;
        private System.Windows.Forms.Label lblSelectedCount;
        private System.Windows.Forms.Label lblSelectedSuffix;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnProcessSelected;
    }
}