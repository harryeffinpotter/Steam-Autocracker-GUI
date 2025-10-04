namespace SteamAppIdIdentifier
{
    partial class RequestsWindow
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.searchResultsGrid = new System.Windows.Forms.DataGridView();
            this.communityRequestsGrid = new System.Windows.Forms.DataGridView();
            this.myRequestsGrid = new System.Windows.Forms.DataGridView();
            this.communityRequestsLabel = new System.Windows.Forms.Label();
            this.myRequestsLabel = new System.Windows.Forms.Label();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.searchResultsGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.communityRequestsGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.myRequestsGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // searchBox
            // 
            this.searchBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(10)))), ((int)(((byte)(35)))));
            this.searchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.searchBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.searchBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.searchBox.Location = new System.Drawing.Point(12, 10);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(400, 26);
            this.searchBox.TabIndex = 0;
            this.searchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchBox_KeyDown);
            // 
            // searchButton
            // 
            this.searchButton.BackColor = System.Drawing.Color.Transparent;
            this.searchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.searchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.searchButton.Location = new System.Drawing.Point(420, 10);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(80, 26);
            this.searchButton.TabIndex = 1;
            this.searchButton.Text = "Search";
            this.searchButton.UseVisualStyleBackColor = false;
            this.searchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.statusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.statusLabel.Location = new System.Drawing.Point(510, 11);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(400, 25);
            this.statusLabel.TabIndex = 2;
            this.statusLabel.Text = "Ready...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // searchResultsGrid
            // 
            this.searchResultsGrid.AllowUserToAddRows = false;
            this.searchResultsGrid.AllowUserToDeleteRows = false;
            this.searchResultsGrid.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(5)))), ((int)(((byte)(25)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.SpringGreen;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            this.searchResultsGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.searchResultsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.searchResultsGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(5)))), ((int)(((byte)(25)))));
            this.searchResultsGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(25)))), ((int)(((byte)(75)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Ivory;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(25)))), ((int)(((byte)(75)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Ivory;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.searchResultsGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.searchResultsGrid.ColumnHeadersHeight = 34;
            this.searchResultsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(2)))), ((int)(((byte)(10)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SpringGreen;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.searchResultsGrid.DefaultCellStyle = dataGridViewCellStyle3;
            this.searchResultsGrid.EnableHeadersVisualStyles = false;
            this.searchResultsGrid.GridColor = System.Drawing.Color.White;
            this.searchResultsGrid.Location = new System.Drawing.Point(12, 45);
            this.searchResultsGrid.MultiSelect = false;
            this.searchResultsGrid.Name = "searchResultsGrid";
            this.searchResultsGrid.ReadOnly = true;
            this.searchResultsGrid.RowHeadersVisible = false;
            this.searchResultsGrid.RowHeadersWidth = 62;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(10)))), ((int)(((byte)(35)))));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(111)))), ((int)(((byte)(65)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black;
            this.searchResultsGrid.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.searchResultsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.searchResultsGrid.Size = new System.Drawing.Size(1176, 250);
            this.searchResultsGrid.TabIndex = 3;
            this.searchResultsGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.SearchResultsGrid_CellContentClick);
            this.searchResultsGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.SearchResultsGrid_CellFormatting);
            // 
            // communityRequestsGrid
            // 
            this.communityRequestsGrid.AllowUserToAddRows = false;
            this.communityRequestsGrid.AllowUserToDeleteRows = false;
            this.communityRequestsGrid.AllowUserToResizeRows = false;
            this.communityRequestsGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.communityRequestsGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(5)))), ((int)(((byte)(25)))));
            this.communityRequestsGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.communityRequestsGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.communityRequestsGrid.ColumnHeadersHeight = 34;
            this.communityRequestsGrid.DefaultCellStyle = dataGridViewCellStyle3;
            this.communityRequestsGrid.EnableHeadersVisualStyles = false;
            this.communityRequestsGrid.GridColor = System.Drawing.Color.White;
            this.communityRequestsGrid.Location = new System.Drawing.Point(12, 330);
            this.communityRequestsGrid.MultiSelect = false;
            this.communityRequestsGrid.Name = "communityRequestsGrid";
            this.communityRequestsGrid.ReadOnly = true;
            this.communityRequestsGrid.RowHeadersVisible = false;
            this.communityRequestsGrid.RowHeadersWidth = 62;
            this.communityRequestsGrid.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.communityRequestsGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.communityRequestsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.communityRequestsGrid.Size = new System.Drawing.Size(1176, 207);
            this.communityRequestsGrid.TabIndex = 5;
            // 
            // myRequestsGrid
            // 
            this.myRequestsGrid.AllowUserToAddRows = false;
            this.myRequestsGrid.AllowUserToDeleteRows = false;
            this.myRequestsGrid.AllowUserToResizeRows = false;
            this.myRequestsGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.myRequestsGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(5)))), ((int)(((byte)(25)))));
            this.myRequestsGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.myRequestsGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.myRequestsGrid.ColumnHeadersHeight = 34;
            this.myRequestsGrid.DefaultCellStyle = dataGridViewCellStyle3;
            this.myRequestsGrid.EnableHeadersVisualStyles = false;
            this.myRequestsGrid.GridColor = System.Drawing.Color.White;
            this.myRequestsGrid.Location = new System.Drawing.Point(12, 565);
            this.myRequestsGrid.MultiSelect = false;
            this.myRequestsGrid.Name = "myRequestsGrid";
            this.myRequestsGrid.ReadOnly = true;
            this.myRequestsGrid.RowHeadersVisible = false;
            this.myRequestsGrid.RowHeadersWidth = 62;
            this.myRequestsGrid.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.myRequestsGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.myRequestsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.myRequestsGrid.Size = new System.Drawing.Size(1176, 223);
            this.myRequestsGrid.TabIndex = 7;
            this.myRequestsGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.MyRequestsGrid_CellContentClick);
            // 
            // communityRequestsLabel
            // 
            this.communityRequestsLabel.AutoSize = true;
            this.communityRequestsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.communityRequestsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.communityRequestsLabel.Location = new System.Drawing.Point(12, 305);
            this.communityRequestsLabel.Name = "communityRequestsLabel";
            this.communityRequestsLabel.Size = new System.Drawing.Size(169, 18);
            this.communityRequestsLabel.TabIndex = 4;
            this.communityRequestsLabel.Text = "Community Requests";
            // 
            // myRequestsLabel
            // 
            this.myRequestsLabel.AutoSize = true;
            this.myRequestsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.myRequestsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.myRequestsLabel.Location = new System.Drawing.Point(12, 540);
            this.myRequestsLabel.Name = "myRequestsLabel";
            this.myRequestsLabel.Size = new System.Drawing.Size(106, 18);
            this.myRequestsLabel.TabIndex = 6;
            this.myRequestsLabel.Text = "My Requests";
            // 
            // refreshTimer
            // 
            this.refreshTimer.Enabled = true;
            this.refreshTimer.Interval = 60000;
            // 
            // RequestsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(5)))), ((int)(((byte)(25)))));
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.myRequestsGrid);
            this.Controls.Add(this.myRequestsLabel);
            this.Controls.Add(this.communityRequestsGrid);
            this.Controls.Add(this.communityRequestsLabel);
            this.Controls.Add(this.searchResultsGrid);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.searchBox);
            this.Name = "RequestsWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Request Games";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RequestsWindow_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.searchResultsGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.communityRequestsGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.myRequestsGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.DataGridView searchResultsGrid;
        private System.Windows.Forms.DataGridView communityRequestsGrid;
        private System.Windows.Forms.DataGridView myRequestsGrid;
        private System.Windows.Forms.Label communityRequestsLabel;
        private System.Windows.Forms.Label myRequestsLabel;
        private System.Windows.Forms.Timer refreshTimer;
    }
}