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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.gamesGrid = new System.Windows.Forms.DataGridView();
            this.GameName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstallPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GameSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BuildID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AppID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ShareClean = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ShareCracked = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblStatus = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.titleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
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
            this.mainPanel.Padding = new System.Windows.Forms.Padding(1);
            this.mainPanel.Size = new System.Drawing.Size(1000, 700);
            this.mainPanel.TabIndex = 0;
            // 
            // gamesGrid
            // 
            this.gamesGrid.AllowUserToAddRows = false;
            this.gamesGrid.AllowUserToDeleteRows = false;
            this.gamesGrid.AllowUserToOrderColumns = true;
            this.gamesGrid.AllowUserToResizeRows = false;
            this.gamesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gamesGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(5)))), ((int)(((byte)(8)))));
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
            this.GameName,
            this.InstallPath,
            this.GameSize,
            this.BuildID,
            this.AppID,
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
            this.gamesGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(35)))), ((int)(((byte)(45)))));
            this.gamesGrid.Location = new System.Drawing.Point(1, 36);
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
            this.gamesGrid.Size = new System.Drawing.Size(998, 613);
            this.gamesGrid.TabIndex = 1;
            this.gamesGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GamesGrid_CellClick);
            this.gamesGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gamesGrid_CellContentClick);
            this.gamesGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.GamesGrid_CellFormatting);
            this.gamesGrid.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.GamesGrid_CellMouseEnter);
            this.gamesGrid.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.GamesGrid_CellMouseLeave);
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
            // ShareClean
            // 
            this.ShareClean.HeaderText = "Share Clean";
            this.ShareClean.MinimumWidth = 8;
            this.ShareClean.Name = "ShareClean";
            this.ShareClean.ReadOnly = true;
            // 
            // ShareCracked
            // 
            this.ShareCracked.HeaderText = "Share Cracked";
            this.ShareCracked.MinimumWidth = 8;
            this.ShareCracked.Name = "ShareCracked";
            this.ShareCracked.ReadOnly = true;
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.Cyan;
            this.lblStatus.Location = new System.Drawing.Point(1, 649);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(998, 20);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Visible = false;
            // 
            // progressBar
            // 
            this.progressBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(1, 669);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(998, 30);
            this.progressBar.TabIndex = 3;
            this.progressBar.Visible = false;
            // 
            // titleBar
            // 
            this.titleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(50)))), ((int)(((byte)(60)))), ((int)(((byte)(80)))));
            this.titleBar.Controls.Add(this.lblTitle);
            this.titleBar.Controls.Add(this.btnMinimize);
            this.titleBar.Controls.Add(this.btnClose);
            this.titleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.titleBar.Location = new System.Drawing.Point(1, 1);
            this.titleBar.Name = "titleBar";
            this.titleBar.Size = new System.Drawing.Size(998, 35);
            this.titleBar.TabIndex = 4;
            this.titleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleBar_MouseDown);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
            this.lblTitle.Location = new System.Drawing.Point(10, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(131, 19);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Share Your Games";
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleBar_MouseDown);
            // 
            // btnMinimize
            // 
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(45)))));
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnMinimize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.btnMinimize.Location = new System.Drawing.Point(923, 0);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(35, 35);
            this.btnMinimize.TabIndex = 2;
            this.btnMinimize.Text = "−";
            this.btnMinimize.UseVisualStyleBackColor = false;
            this.btnMinimize.Click += new System.EventHandler(this.BtnMinimize_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(150)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.btnClose.Location = new System.Drawing.Point(963, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(35, 35);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "✕";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // EnhancedShareWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(8)))), ((int)(((byte)(15)))));
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.mainPanel);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "EnhancedShareWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Share Your Games";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EnhancedShareWindow_FormClosed);
            this.Load += new System.EventHandler(this.EnhancedShareWindow_Load);
            this.mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gamesGrid)).EndInit();
            this.titleBar.ResumeLayout(false);
            this.titleBar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridViewTextBoxColumn GameName;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstallPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn GameSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn BuildID;
        private System.Windows.Forms.DataGridViewTextBoxColumn AppID;
        private System.Windows.Forms.DataGridViewTextBoxColumn ShareClean;
        private System.Windows.Forms.DataGridViewTextBoxColumn ShareCracked;
    }
}