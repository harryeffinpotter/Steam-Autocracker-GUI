
namespace APPID
{
    partial class SteamAppId
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel titleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnClose;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SteamAppId));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnSearch = new System.Windows.Forms.Button();
            this.resinstruccZip = new System.Windows.Forms.Label();
            this.tgDisc = new System.Windows.Forms.Label();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.pin = new System.Windows.Forms.PictureBox();
            this.unPin = new System.Windows.Forms.PictureBox();
            this.gitHub = new System.Windows.Forms.PictureBox();
            this.ShareButton = new System.Windows.Forms.Button();
            this.RequestButton = new System.Windows.Forms.Button();
            this.autoCrackOff = new System.Windows.Forms.PictureBox();
            this.autoCrackOn = new System.Windows.Forms.PictureBox();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.OpenDir = new System.Windows.Forms.Button();
            this.ZipToShare = new System.Windows.Forms.Button();
            this.UploadZipButton = new System.Windows.Forms.Button();
            this.currDIrText = new System.Windows.Forms.Label();
            this.dllSelect = new APPID.TransparentComboBox();
            this.lanMultiplayerCheckBox = new System.Windows.Forms.CheckBox();
            this.donePic = new System.Windows.Forms.PictureBox();
            this.startCrackPic = new System.Windows.Forms.PictureBox();
            this.selectDir = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.selectDirLabel = new System.Windows.Forms.Label();
            this.drgdropText = new System.Windows.Forms.Label();
            this.btnManualEntry = new System.Windows.Forms.Button();
            this.lanMultiplayerToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ManAppPanel = new System.Windows.Forms.Panel();
            this.ManAppBox = new System.Windows.Forms.TextBox();
            this.ManAppBtn = new System.Windows.Forms.Button();
            this.ManAppLbl = new System.Windows.Forms.Label();
            this.titleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.unPin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitHub)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.autoCrackOff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.autoCrackOn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.donePic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startCrackPic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selectDir)).BeginInit();
            this.ManAppPanel.SuspendLayout();
            this.titleBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // searchTextBox
            // 
            this.searchTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(12)))));
            resources.ApplyResources(this.searchTextBox, "searchTextBox");
            this.searchTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.searchTextBox_MouseClick);
            this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
            this.searchTextBox.Enter += new System.EventHandler(this.searchTextBox_Enter);
            this.searchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchTextBox_KeyDown);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(5)))), ((int)(((byte)(25)))));
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.SpringGreen;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.Black;
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(10)))), ((int)(((byte)(30)))));
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(25)))), ((int)(((byte)(75)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.Ivory;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(25)))), ((int)(((byte)(75)))));
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.Ivory;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            resources.ApplyResources(this.dataGridView1, "dataGridView1");
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(2)))), ((int)(((byte)(10)))));
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.SpringGreen;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.GridColor = System.Drawing.Color.White;
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(5)))), ((int)(((byte)(25)))));
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.Color.SteelBlue;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(10)))), ((int)(((byte)(35)))));
            dataGridViewCellStyle10.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(111)))), ((int)(((byte)(65)))));
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.Color.Black;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle10;
            this.dataGridView1.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(5)))), ((int)(((byte)(25)))));
            this.dataGridView1.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dataGridView1.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(70)))), ((int)(((byte)(100)))));
            this.dataGridView1.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            this.dataGridView1.RowTemplate.ReadOnly = true;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.StandardTab = true;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dataGridView1.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_RowHeaderMouseClick);
            this.dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyDown);
            // 
            // btnSearch
            // 
            resources.ApplyResources(this.btnSearch, "btnSearch");
            this.btnSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // resinstruccZip
            // 
            resources.ApplyResources(this.resinstruccZip, "resinstruccZip");
            this.resinstruccZip.BackColor = System.Drawing.Color.Transparent;
            this.resinstruccZip.ForeColor = System.Drawing.Color.White;
            this.resinstruccZip.Name = "resinstruccZip";
            this.resinstruccZip.Click += new System.EventHandler(this.resinstruccZip_Click);
            // 
            // tgDisc
            // 
            resources.ApplyResources(this.tgDisc, "tgDisc");
            this.tgDisc.BackColor = System.Drawing.Color.Transparent;
            this.tgDisc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tgDisc.Name = "tgDisc";
            this.tgDisc.Click += new System.EventHandler(this.label2_Click);
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.UploadZipButton);
            this.mainPanel.Controls.Add(this.pin);
            this.mainPanel.Controls.Add(this.unPin);
            this.mainPanel.Controls.Add(this.gitHub);
            this.mainPanel.Controls.Add(this.ShareButton);
            this.mainPanel.Controls.Add(this.RequestButton);
            this.mainPanel.Controls.Add(this.autoCrackOff);
            this.mainPanel.Controls.Add(this.autoCrackOn);
            this.mainPanel.Controls.Add(this.StatusLabel);
            this.mainPanel.Controls.Add(this.tgDisc);
            this.mainPanel.Controls.Add(this.OpenDir);
            this.mainPanel.Controls.Add(this.ZipToShare);
            this.mainPanel.Controls.Add(this.currDIrText);
            this.mainPanel.Controls.Add(this.dllSelect);
            this.mainPanel.Controls.Add(this.lanMultiplayerCheckBox);
            this.mainPanel.Controls.Add(this.donePic);
            this.mainPanel.Controls.Add(this.startCrackPic);
            this.mainPanel.Controls.Add(this.selectDir);
            this.mainPanel.Controls.Add(this.label5);
            this.mainPanel.Controls.Add(this.label1);
            this.mainPanel.Controls.Add(this.selectDirLabel);
            this.mainPanel.Controls.Add(this.drgdropText);
            resources.ApplyResources(this.mainPanel, "mainPanel");
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Click += new System.EventHandler(this.Form_Click);
            this.mainPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragDrop);
            this.mainPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragEnter);
            this.mainPanel.DragLeave += new System.EventHandler(this.mainPanel_DragLeave);
            // 
            // pin
            // 
            this.pin.BackColor = System.Drawing.Color.Transparent;
            this.pin.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.pin, "pin");
            this.pin.Name = "pin";
            this.lanMultiplayerToolTip.SetToolTip(this.pin, resources.GetString("pin.ToolTip"));
            this.pin.Click += new System.EventHandler(this.pin_Click);
            // 
            // unPin
            // 
            this.unPin.BackColor = System.Drawing.Color.Transparent;
            this.unPin.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.unPin, "unPin");
            this.unPin.Name = "unPin";
            this.lanMultiplayerToolTip.SetToolTip(this.unPin, resources.GetString("unPin.ToolTip"));
            this.unPin.Click += new System.EventHandler(this.unPin_Click);
            // 
            // gitHub
            // 
            this.gitHub.BackColor = System.Drawing.Color.Transparent;
            this.gitHub.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.gitHub, "gitHub");
            this.gitHub.Name = "gitHub";
            this.gitHub.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // ShareButton
            // 
            resources.ApplyResources(this.ShareButton, "ShareButton");
            this.ShareButton.BackColor = System.Drawing.Color.Transparent;
            this.ShareButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ShareButton.FlatAppearance.BorderSize = 0;
            this.ShareButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ShareButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ShareButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ShareButton.Name = "ShareButton";
            this.lanMultiplayerToolTip.SetToolTip(this.ShareButton, resources.GetString("ShareButton.ToolTip"));
            this.ShareButton.UseVisualStyleBackColor = false;
            this.ShareButton.Click += new System.EventHandler(this.ShareButton_Click);
            // 
            // RequestButton
            // 
            this.RequestButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.RequestButton.FlatAppearance.BorderSize = 0;
            this.RequestButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.RequestButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            resources.ApplyResources(this.RequestButton, "RequestButton");
            this.RequestButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.RequestButton.Name = "RequestButton";
            this.RequestButton.UseVisualStyleBackColor = false;
            // 
            // autoCrackOff
            // 
            this.autoCrackOff.BackColor = System.Drawing.Color.Transparent;
            this.autoCrackOff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.autoCrackOff.Image = global::APPID.Properties.Resources._1111;
            resources.ApplyResources(this.autoCrackOff, "autoCrackOff");
            this.autoCrackOff.Name = "autoCrackOff";
            this.lanMultiplayerToolTip.SetToolTip(this.autoCrackOff, resources.GetString("autoCrackOff.ToolTip"));
            this.autoCrackOff.Click += new System.EventHandler(this.autoCrackOff_Click);
            // 
            // autoCrackOn
            // 
            this.autoCrackOn.BackColor = System.Drawing.Color.Transparent;
            this.autoCrackOn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.autoCrackOn.Image = global::APPID.Properties.Resources._22222;
            resources.ApplyResources(this.autoCrackOn, "autoCrackOn");
            this.autoCrackOn.Name = "autoCrackOn";
            this.lanMultiplayerToolTip.SetToolTip(this.autoCrackOn, resources.GetString("autoCrackOn.ToolTip"));
            this.autoCrackOn.Click += new System.EventHandler(this.autoCrackOn_Click);
            // 
            // StatusLabel
            // 
            this.StatusLabel.AllowDrop = true;
            this.StatusLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.StatusLabel, "StatusLabel");
            this.StatusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.DragDrop += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragDrop);
            this.StatusLabel.DragEnter += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragEnter);
            this.StatusLabel.DragLeave += new System.EventHandler(this.mainPanel_DragLeave);
            // 
            // OpenDir
            // 
            this.OpenDir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(110)))), ((int)(((byte)(180)))));
            this.OpenDir.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(60)))));
            this.OpenDir.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(32)))));
            this.OpenDir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(52)))));
            resources.ApplyResources(this.OpenDir, "OpenDir");
            this.OpenDir.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(225)))));
            this.OpenDir.Name = "OpenDir";
            this.OpenDir.TabStop = false;
            this.OpenDir.UseVisualStyleBackColor = false;
            this.OpenDir.Click += new System.EventHandler(this.OpenDir_Click);
            // 
            // ZipToShare
            // 
            this.ZipToShare.BackColor = System.Drawing.Color.CadetBlue;
            this.ZipToShare.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(60)))));
            this.ZipToShare.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(32)))));
            this.ZipToShare.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(52)))));
            resources.ApplyResources(this.ZipToShare, "ZipToShare");
            this.ZipToShare.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(225)))));
            this.ZipToShare.Name = "ZipToShare";
            this.ZipToShare.TabStop = false;
            this.lanMultiplayerToolTip.SetToolTip(this.ZipToShare, resources.GetString("ZipToShare.ToolTip"));
            this.ZipToShare.UseVisualStyleBackColor = false;
            this.ZipToShare.Click += new System.EventHandler(this.ZipToShare_Click);
            // 
            // UploadZipButton
            // 
            this.UploadZipButton.BackColor = System.Drawing.Color.ForestGreen;
            this.UploadZipButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(60)))));
            this.UploadZipButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(32)))));
            this.UploadZipButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(52)))));
            resources.ApplyResources(this.UploadZipButton, "UploadZipButton");
            this.UploadZipButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(225)))));
            this.UploadZipButton.Name = "UploadZipButton";
            this.UploadZipButton.UseVisualStyleBackColor = false;
            // 
            // currDIrText
            // 
            resources.ApplyResources(this.currDIrText, "currDIrText");
            this.currDIrText.ForeColor = System.Drawing.Color.AliceBlue;
            this.currDIrText.Name = "currDIrText";
            // 
            // dllSelect
            // 
            this.dllSelect.AutoCompleteCustomSource.AddRange(new string[] {
            resources.GetString("dllSelect.AutoCompleteCustomSource"),
            resources.GetString("dllSelect.AutoCompleteCustomSource1")});
            this.dllSelect.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(150)))), ((int)(((byte)(200)))));
            this.dllSelect.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.dllSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.dllSelect, "dllSelect");
            this.dllSelect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.dllSelect.FormattingEnabled = true;
            this.dllSelect.Items.AddRange(new object[] {
            resources.GetString("dllSelect.Items"),
            resources.GetString("dllSelect.Items1")});
            this.dllSelect.Name = "dllSelect";
            this.dllSelect.SelectedIndexChanged += new System.EventHandler(this.dllSelect_SelectedIndexChanged);
            // 
            // lanMultiplayerCheckBox
            // 
            resources.ApplyResources(this.lanMultiplayerCheckBox, "lanMultiplayerCheckBox");
            this.lanMultiplayerCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.lanMultiplayerCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lanMultiplayerCheckBox.Name = "lanMultiplayerCheckBox";
            this.lanMultiplayerToolTip.SetToolTip(this.lanMultiplayerCheckBox, resources.GetString("lanMultiplayerCheckBox.ToolTip"));
            this.lanMultiplayerCheckBox.UseVisualStyleBackColor = false;
            this.lanMultiplayerCheckBox.CheckedChanged += new System.EventHandler(this.lanMultiplayerCheckBox_CheckedChanged);
            // 
            // donePic
            // 
            this.donePic.BackColor = System.Drawing.Color.Transparent;
            this.donePic.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.donePic, "donePic");
            this.donePic.Name = "donePic";
            this.donePic.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // startCrackPic
            // 
            this.startCrackPic.BackColor = System.Drawing.Color.Transparent;
            this.startCrackPic.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.startCrackPic, "startCrackPic");
            this.startCrackPic.Name = "startCrackPic";
            this.startCrackPic.Click += new System.EventHandler(this.startCrackPic_Click);
            this.startCrackPic.MouseDown += new System.Windows.Forms.MouseEventHandler(this.startCrackPic_MouseDown);
            this.startCrackPic.MouseEnter += new System.EventHandler(this.startCrackPic_MouseEnter);
            this.startCrackPic.MouseLeave += new System.EventHandler(this.startCrackPic_MouseLeave);
            this.startCrackPic.MouseHover += new System.EventHandler(this.startCrackPic_MouseHover);
            // 
            // selectDir
            // 
            this.selectDir.BackColor = System.Drawing.Color.Transparent;
            this.selectDir.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.selectDir, "selectDir");
            this.selectDir.Name = "selectDir";
            this.selectDir.Click += new System.EventHandler(this.pictureBox2_Click);
            this.selectDir.MouseDown += new System.Windows.Forms.MouseEventHandler(this.selectDir_MouseDown);
            this.selectDir.MouseEnter += new System.EventHandler(this.selectDir_MouseEnter);
            this.selectDir.MouseLeave += new System.EventHandler(this.selectDir_MouseLeave);
            this.selectDir.MouseHover += new System.EventHandler(this.selectDir_MouseHover);
            // 
            // label5
            // 
            this.label5.AllowDrop = true;
            resources.ApplyResources(this.label5, "label5");
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.label5.Name = "label5";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            this.label5.DragDrop += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragDrop);
            this.label5.DragEnter += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragEnter);
            this.label5.DragLeave += new System.EventHandler(this.mainPanel_DragLeave);
            // 
            // label1
            // 
            this.label1.AllowDrop = true;
            resources.ApplyResources(this.label1, "label1");
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Name = "label1";
            this.label1.DragDrop += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragDrop);
            this.label1.DragEnter += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragEnter);
            this.label1.DragLeave += new System.EventHandler(this.mainPanel_DragLeave);
            // 
            // selectDirLabel
            // 
            this.selectDirLabel.AllowDrop = true;
            resources.ApplyResources(this.selectDirLabel, "selectDirLabel");
            this.selectDirLabel.BackColor = System.Drawing.Color.Transparent;
            this.selectDirLabel.ForeColor = System.Drawing.Color.White;
            this.selectDirLabel.Name = "selectDirLabel";
            this.selectDirLabel.DragDrop += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragDrop);
            this.selectDirLabel.DragEnter += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragEnter);
            this.selectDirLabel.DragLeave += new System.EventHandler(this.mainPanel_DragLeave);
            // 
            // drgdropText
            // 
            this.drgdropText.AllowDrop = true;
            this.drgdropText.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.drgdropText, "drgdropText");
            this.drgdropText.ForeColor = System.Drawing.SystemColors.Control;
            this.drgdropText.Name = "drgdropText";
            this.drgdropText.DragDrop += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragDrop);
            this.drgdropText.DragEnter += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragEnter);
            this.drgdropText.DragLeave += new System.EventHandler(this.mainPanel_DragLeave);
            // 
            // btnManualEntry
            // 
            this.btnManualEntry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(42)))));
            this.btnManualEntry.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(60)))));
            this.btnManualEntry.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(32)))));
            this.btnManualEntry.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(52)))));
            resources.ApplyResources(this.btnManualEntry, "btnManualEntry");
            this.btnManualEntry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(225)))));
            this.btnManualEntry.Name = "btnManualEntry";
            this.btnManualEntry.UseVisualStyleBackColor = false;
            this.btnManualEntry.Click += new System.EventHandler(this.btnManualEntry_Click);
            // 
            // lanMultiplayerToolTip
            // 
            this.lanMultiplayerToolTip.AutoPopDelay = 15000;
            this.lanMultiplayerToolTip.InitialDelay = 500;
            this.lanMultiplayerToolTip.ReshowDelay = 100;
            this.lanMultiplayerToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.lanMultiplayerToolTip.ToolTipTitle = "Info";
            // 
            // ManAppPanel
            // 
            this.ManAppPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ManAppPanel.Controls.Add(this.ManAppBox);
            this.ManAppPanel.Controls.Add(this.ManAppBtn);
            this.ManAppPanel.Controls.Add(this.ManAppLbl);
            resources.ApplyResources(this.ManAppPanel, "ManAppPanel");
            this.ManAppPanel.Name = "ManAppPanel";
            this.ManAppPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.ManAppPanel_Paint);
            // 
            // ManAppBox
            // 
            this.ManAppBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(12)))));
            this.ManAppBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.ManAppBox, "ManAppBox");
            this.ManAppBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ManAppBox.Name = "ManAppBox";
            this.ManAppBox.TextChanged += new System.EventHandler(this.ManAppBox_TextChanged);
            this.ManAppBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ManAppBox_KeyDown);
            // 
            // ManAppBtn
            // 
            this.ManAppBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(15)))), ((int)(((byte)(25)))), ((int)(((byte)(45)))));
            this.ManAppBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(150)))), ((int)(((byte)(200)))));
            this.ManAppBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(30)))), ((int)(((byte)(50)))), ((int)(((byte)(80)))));
            resources.ApplyResources(this.ManAppBtn, "ManAppBtn");
            this.ManAppBtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ManAppBtn.Name = "ManAppBtn";
            this.ManAppBtn.UseVisualStyleBackColor = false;
            this.ManAppBtn.Click += new System.EventHandler(this.ManAppBtn_Click);
            // 
            // ManAppLbl
            // 
            this.ManAppLbl.AllowDrop = true;
            resources.ApplyResources(this.ManAppLbl, "ManAppLbl");
            this.ManAppLbl.BackColor = System.Drawing.Color.Transparent;
            this.ManAppLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(220)))), ((int)(((byte)(255)))));
            this.ManAppLbl.Name = "ManAppLbl";
            this.ManAppLbl.DragDrop += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragDrop);
            this.ManAppLbl.DragEnter += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragEnter);
            this.ManAppLbl.DragLeave += new System.EventHandler(this.mainPanel_DragLeave);
            // 
            // titleBar
            // 
            this.titleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.titleBar.Controls.Add(this.lblTitle);
            this.titleBar.Controls.Add(this.btnMinimize);
            this.titleBar.Controls.Add(this.btnClose);
            resources.ApplyResources(this.titleBar, "titleBar");
            this.titleBar.Name = "titleBar";
            this.titleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleBar_MouseDown);
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TitleBar_MouseDown);
            // 
            // btnMinimize
            // 
            resources.ApplyResources(this.btnMinimize, "btnMinimize");
            this.btnMinimize.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(45)))));
            this.btnMinimize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.UseVisualStyleBackColor = false;
            this.btnMinimize.Click += new System.EventHandler(this.BtnMinimize_Click);
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(150)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // SteamAppId
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(8)))), ((int)(((byte)(20)))));
            this.Controls.Add(this.titleBar);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.btnManualEntry);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.ManAppPanel);
            this.Controls.Add(this.searchTextBox);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.resinstruccZip);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SteamAppId";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SteamAppId_FormClosing);
            this.Load += new System.EventHandler(this.SteamAppId_Load);
            this.Click += new System.EventHandler(this.Form_Click);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.mainPanel_DragEnter);
            this.DragLeave += new System.EventHandler(this.mainPanel_DragLeave);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.unPin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitHub)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.autoCrackOff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.autoCrackOn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.donePic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startCrackPic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selectDir)).EndInit();
            this.ManAppPanel.ResumeLayout(false);
            this.ManAppPanel.PerformLayout();
            this.titleBar.ResumeLayout(false);
            this.titleBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Label resinstruccZip;
        private System.Windows.Forms.Label tgDisc;
        public System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.PictureBox gitHub;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.PictureBox selectDir;
        private System.Windows.Forms.Label selectDirLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox donePic;
        private System.Windows.Forms.PictureBox startCrackPic;
        private TransparentComboBox dllSelect;
        private System.Windows.Forms.CheckBox lanMultiplayerCheckBox;
        private System.Windows.Forms.ToolTip lanMultiplayerToolTip;
        private System.Windows.Forms.Label drgdropText;
        public System.Windows.Forms.Label currDIrText;
        private System.Windows.Forms.PictureBox unPin;
        private System.Windows.Forms.PictureBox pin;
        private System.Windows.Forms.Button OpenDir;
        private System.Windows.Forms.Button ZipToShare;
        private System.Windows.Forms.Button UploadZipButton;
        private System.Windows.Forms.Button ShareButton;
        private System.Windows.Forms.Button RequestButton;
        private System.Windows.Forms.Panel ManAppPanel;
        private System.Windows.Forms.Label ManAppLbl;
        private System.Windows.Forms.TextBox ManAppBox;
        private System.Windows.Forms.Button ManAppBtn;
        private System.Windows.Forms.Label StatusLabel;
        public System.Windows.Forms.Button btnManualEntry;
        private System.Windows.Forms.PictureBox autoCrackOff;
        private System.Windows.Forms.PictureBox autoCrackOn;
        private System.Windows.Forms.Label label1;
    }
}

