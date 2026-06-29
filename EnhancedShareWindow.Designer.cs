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
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            mainPanel = new System.Windows.Forms.Panel();
            gamesGrid = new System.Windows.Forms.DataGridView();
            SelectGame = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            GameName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            InstallPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            GameSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            BuildID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            AppID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            LastUpdated = new System.Windows.Forms.DataGridViewTextBoxColumn();
            CrackOnly = new System.Windows.Forms.DataGridViewButtonColumn();
            ShareClean = new System.Windows.Forms.DataGridViewButtonColumn();
            ShareCracked = new System.Windows.Forms.DataGridViewButtonColumn();
            lblStatus = new System.Windows.Forms.Label();
            progressBar = new System.Windows.Forms.ProgressBar();
            titleBar = new System.Windows.Forms.Panel();
            lblTitle = new System.Windows.Forms.Label();
            lblSelectedPrefix = new System.Windows.Forms.Label();
            lblSelectedCount = new System.Windows.Forms.Label();
            lblSelectedSuffix = new System.Windows.Forms.Label();
            btnSettings = new System.Windows.Forms.Button();
            btnProcessSelected = new System.Windows.Forms.Button();
            btnMinimize = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            toolTip = new System.Windows.Forms.ToolTip(components);
            mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gamesGrid).BeginInit();
            titleBar.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = System.Drawing.Color.FromArgb(220, 0, 0, 0);
            mainPanel.Controls.Add(gamesGrid);
            mainPanel.Controls.Add(lblStatus);
            mainPanel.Controls.Add(progressBar);
            mainPanel.Controls.Add(titleBar);
            mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainPanel.Location = new System.Drawing.Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new System.Drawing.Size(1200, 807);
            mainPanel.TabIndex = 0;
            // 
            // gamesGrid
            // 
            gamesGrid.AllowUserToAddRows = false;
            gamesGrid.AllowUserToDeleteRows = false;
            gamesGrid.AllowUserToOrderColumns = true;
            gamesGrid.AllowUserToResizeRows = false;
            gamesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            gamesGrid.BackgroundColor = System.Drawing.Color.FromArgb(8, 8, 12);
            gamesGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            gamesGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(10, 12, 18);
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(100, 200, 255);
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(15, 18, 25);
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(100, 200, 255);
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            gamesGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            gamesGrid.ColumnHeadersHeight = 35;
            gamesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gamesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { SelectGame, GameName, InstallPath, GameSize, BuildID, AppID, LastUpdated, CrackOnly, ShareClean, ShareCracked });
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(8, 8, 12);
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(20, 25, 35);
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            gamesGrid.DefaultCellStyle = dataGridViewCellStyle2;
            gamesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            gamesGrid.EnableHeadersVisualStyles = false;
            gamesGrid.GridColor = System.Drawing.Color.FromArgb(40, 40, 45);
            gamesGrid.Location = new System.Drawing.Point(0, 41);
            gamesGrid.Margin = new System.Windows.Forms.Padding(4);
            gamesGrid.MultiSelect = false;
            gamesGrid.Name = "gamesGrid";
            gamesGrid.ReadOnly = true;
            gamesGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(8, 8, 12);
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(20, 25, 35);
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            gamesGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            gamesGrid.RowHeadersVisible = false;
            gamesGrid.RowHeadersWidth = 62;
            gamesGrid.RowTemplate.Height = 30;
            gamesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            gamesGrid.Size = new System.Drawing.Size(1200, 709);
            gamesGrid.TabIndex = 1;
            toolTip.SetToolTip(gamesGrid, "Click on game rows to perform actions. Click column headers to sort.");
            gamesGrid.CellClick += GamesGrid_CellClick;
            gamesGrid.CellContentClick += gamesGrid_CellContentClick;
            gamesGrid.CellFormatting += GamesGrid_CellFormatting;
            gamesGrid.CellMouseEnter += GamesGrid_CellMouseEnter;
            gamesGrid.CellMouseLeave += GamesGrid_CellMouseLeave;
            // 
            // SelectGame
            // 
            SelectGame.FillWeight = 20F;
            SelectGame.HeaderText = "☑";
            SelectGame.MinimumWidth = 30;
            SelectGame.Name = "SelectGame";
            SelectGame.ReadOnly = true;
            // 
            // GameName
            // 
            GameName.HeaderText = "Game";
            GameName.MinimumWidth = 8;
            GameName.Name = "GameName";
            GameName.ReadOnly = true;
            // 
            // InstallPath
            // 
            InstallPath.HeaderText = "Install Path";
            InstallPath.MinimumWidth = 8;
            InstallPath.Name = "InstallPath";
            InstallPath.ReadOnly = true;
            // 
            // GameSize
            // 
            GameSize.HeaderText = "Size";
            GameSize.MinimumWidth = 8;
            GameSize.Name = "GameSize";
            GameSize.ReadOnly = true;
            // 
            // BuildID
            // 
            BuildID.HeaderText = "Build";
            BuildID.MinimumWidth = 8;
            BuildID.Name = "BuildID";
            BuildID.ReadOnly = true;
            // 
            // AppID
            // 
            AppID.HeaderText = "AppID";
            AppID.MinimumWidth = 8;
            AppID.Name = "AppID";
            AppID.ReadOnly = true;
            // 
            // LastUpdated
            // 
            LastUpdated.HeaderText = "Last Updated";
            LastUpdated.MinimumWidth = 8;
            LastUpdated.Name = "LastUpdated";
            LastUpdated.ReadOnly = true;
            // 
            // CrackOnly
            // 
            CrackOnly.FillWeight = 45F;
            CrackOnly.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            CrackOnly.HeaderText = "Crack";
            CrackOnly.MinimumWidth = 70;
            CrackOnly.Name = "CrackOnly";
            CrackOnly.ReadOnly = true;
            CrackOnly.Text = "Crack";
            // 
            // ShareClean
            // 
            ShareClean.FillWeight = 45F;
            ShareClean.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ShareClean.HeaderText = "Clean";
            ShareClean.MinimumWidth = 70;
            ShareClean.Name = "ShareClean";
            ShareClean.ReadOnly = true;
            ShareClean.Text = "Clean";
            // 
            // ShareCracked
            // 
            ShareCracked.FillWeight = 50F;
            ShareCracked.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ShareCracked.HeaderText = "Cracked";
            ShareCracked.MinimumWidth = 80;
            ShareCracked.Name = "ShareCracked";
            ShareCracked.ReadOnly = true;
            ShareCracked.Text = "Crack + Share";
            // 
            // lblStatus
            // 
            lblStatus.BackColor = System.Drawing.Color.Transparent;
            lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.Cyan;
            lblStatus.Location = new System.Drawing.Point(0, 750);
            lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(1200, 23);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Ready";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblStatus.Visible = false;
            // 
            // progressBar
            // 
            progressBar.BackColor = System.Drawing.Color.FromArgb(15, 15, 15);
            progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            progressBar.Location = new System.Drawing.Point(0, 773);
            progressBar.Margin = new System.Windows.Forms.Padding(4);
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(1200, 34);
            progressBar.TabIndex = 3;
            progressBar.Visible = false;
            // 
            // titleBar
            // 
            titleBar.BackColor = System.Drawing.Color.FromArgb(60, 50, 60, 80);
            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(lblSelectedPrefix);
            titleBar.Controls.Add(lblSelectedCount);
            titleBar.Controls.Add(lblSelectedSuffix);
            titleBar.Controls.Add(btnSettings);
            titleBar.Controls.Add(btnProcessSelected);
            titleBar.Controls.Add(btnMinimize);
            titleBar.Controls.Add(btnClose);
            titleBar.Dock = System.Windows.Forms.DockStyle.Top;
            titleBar.Location = new System.Drawing.Point(0, 0);
            titleBar.Margin = new System.Windows.Forms.Padding(4);
            titleBar.Name = "titleBar";
            titleBar.Size = new System.Drawing.Size(1200, 41);
            titleBar.TabIndex = 4;
            titleBar.MouseDown += TitleBar_MouseDown;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = System.Drawing.Color.Transparent;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(200, 200, 210);
            lblTitle.Location = new System.Drawing.Point(13, 9);
            lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(133, 19);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Crack | Zip | Share";
            lblTitle.MouseDown += TitleBar_MouseDown;
            // 
            // lblSelectedPrefix
            // 
            lblSelectedPrefix.AutoSize = true;
            lblSelectedPrefix.BackColor = System.Drawing.Color.Transparent;
            lblSelectedPrefix.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSelectedPrefix.ForeColor = System.Drawing.Color.FromArgb(150, 150, 160);
            lblSelectedPrefix.Location = new System.Drawing.Point(185, 13);
            lblSelectedPrefix.Name = "lblSelectedPrefix";
            lblSelectedPrefix.Size = new System.Drawing.Size(54, 15);
            lblSelectedPrefix.TabIndex = 1;
            lblSelectedPrefix.Text = "Selected ";
            lblSelectedPrefix.Visible = false;
            // 
            // lblSelectedCount
            // 
            lblSelectedCount.AutoSize = true;
            lblSelectedCount.BackColor = System.Drawing.Color.Transparent;
            lblSelectedCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblSelectedCount.ForeColor = System.Drawing.Color.FromArgb(100, 255, 150);
            lblSelectedCount.Location = new System.Drawing.Point(185, 14);
            lblSelectedCount.Name = "lblSelectedCount";
            lblSelectedCount.Size = new System.Drawing.Size(14, 15);
            lblSelectedCount.TabIndex = 2;
            lblSelectedCount.Text = "0";
            lblSelectedCount.Visible = false;
            // 
            // lblSelectedSuffix
            // 
            lblSelectedSuffix.AutoSize = true;
            lblSelectedSuffix.BackColor = System.Drawing.Color.Transparent;
            lblSelectedSuffix.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSelectedSuffix.ForeColor = System.Drawing.Color.FromArgb(150, 150, 160);
            lblSelectedSuffix.Location = new System.Drawing.Point(257, 13);
            lblSelectedSuffix.Name = "lblSelectedSuffix";
            lblSelectedSuffix.Size = new System.Drawing.Size(21, 15);
            lblSelectedSuffix.TabIndex = 3;
            lblSelectedSuffix.Text = " to";
            lblSelectedSuffix.Visible = false;
            // 
            // btnSettings
            // 
            btnSettings.BackColor = System.Drawing.Color.FromArgb(20, 22, 28);
            btnSettings.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 65, 75);
            btnSettings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(15, 18, 22);
            btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(20, 22, 28);
            btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSettings.Font = new System.Drawing.Font("Segoe UI", 11F);
            btnSettings.ForeColor = System.Drawing.Color.White;
            btnSettings.Location = new System.Drawing.Point(388, 6);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new System.Drawing.Size(40, 29);
            btnSettings.TabIndex = 14;
            toolTip.SetToolTip(btnSettings, "Compression settings");
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += BtnSettings_Click;
            // 
            // btnProcessSelected
            // 
            btnProcessSelected.BackColor = System.Drawing.Color.FromArgb(30, 80, 30);
            btnProcessSelected.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 140, 60);
            btnProcessSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnProcessSelected.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnProcessSelected.ForeColor = System.Drawing.Color.FromArgb(150, 255, 150);
            btnProcessSelected.Location = new System.Drawing.Point(290, 6);
            btnProcessSelected.Name = "btnProcessSelected";
            btnProcessSelected.Size = new System.Drawing.Size(90, 29);
            btnProcessSelected.TabIndex = 13;
            btnProcessSelected.Text = "Process";
            toolTip.SetToolTip(btnProcessSelected, "Process selected games with chosen actions");
            btnProcessSelected.UseVisualStyleBackColor = false;
            btnProcessSelected.Click += BtnProcessSelected_Click;
            // 
            // btnMinimize
            // 
            btnMinimize.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnMinimize.BackColor = System.Drawing.Color.Transparent;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnMinimize.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            btnMinimize.ForeColor = System.Drawing.Color.FromArgb(150, 200, 255);
            btnMinimize.Location = new System.Drawing.Point(1118, -1);
            btnMinimize.Margin = new System.Windows.Forms.Padding(4);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new System.Drawing.Size(41, 41);
            btnMinimize.TabIndex = 2;
            btnMinimize.Text = "−";
            toolTip.SetToolTip(btnMinimize, "Minimize this window");
            btnMinimize.UseVisualStyleBackColor = false;
            btnMinimize.Click += BtnMinimize_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnClose.BackColor = System.Drawing.Color.Transparent;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnClose.ForeColor = System.Drawing.Color.FromArgb(150, 200, 255);
            btnClose.Location = new System.Drawing.Point(1160, -3);
            btnClose.Margin = new System.Windows.Forms.Padding(4);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(41, 41);
            btnClose.TabIndex = 1;
            btnClose.Text = "✕";
            toolTip.SetToolTip(btnClose, "Close this window");
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += BtnClose_Click;
            // 
            // toolTip
            // 
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 100;
            // 
            // EnhancedShareWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(5, 8, 20);
            ClientSize = new System.Drawing.Size(1200, 807);
            Controls.Add(mainPanel);
            ForeColor = System.Drawing.Color.White;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "EnhancedShareWindow";
            Opacity = 0.95D;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Game Manager - Crack, Share & Export";
            FormClosed += EnhancedShareWindow_FormClosed;
            Load += EnhancedShareWindow_Load;
            mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gamesGrid).EndInit();
            titleBar.ResumeLayout(false);
            titleBar.PerformLayout();
            ResumeLayout(false);

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