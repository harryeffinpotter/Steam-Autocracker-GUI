
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SteamAppId));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            searchTextBox = new System.Windows.Forms.TextBox();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            btnSearch = new System.Windows.Forms.Button();
            resinstruccZip = new System.Windows.Forms.Label();
            tgDisc = new System.Windows.Forms.Label();
            mainPanel = new System.Windows.Forms.Panel();
            UploadZipButton = new System.Windows.Forms.Button();
            pin = new System.Windows.Forms.PictureBox();
            unPin = new System.Windows.Forms.PictureBox();
            gitHub = new System.Windows.Forms.PictureBox();
            ShareButton = new System.Windows.Forms.Button();
            RequestButton = new System.Windows.Forms.Button();
            autoCrackOff = new System.Windows.Forms.PictureBox();
            autoCrackOn = new System.Windows.Forms.PictureBox();
            StatusLabel = new System.Windows.Forms.Label();
            OpenDir = new System.Windows.Forms.Button();
            ZipToShare = new System.Windows.Forms.Button();
            currDIrText = new System.Windows.Forms.Label();
            dllSelect = new TransparentComboBox();
            lanMultiplayerCheckBox = new System.Windows.Forms.CheckBox();
            donePic = new System.Windows.Forms.PictureBox();
            startCrackPic = new System.Windows.Forms.PictureBox();
            selectDir = new System.Windows.Forms.PictureBox();
            label5 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            selectDirLabel = new System.Windows.Forms.Label();
            drgdropText = new System.Windows.Forms.Label();
            btnManualEntry = new System.Windows.Forms.Button();
            lanMultiplayerToolTip = new System.Windows.Forms.ToolTip(components);
            ManAppPanel = new System.Windows.Forms.Panel();
            ManAppBox = new System.Windows.Forms.TextBox();
            ManAppBtn = new System.Windows.Forms.Button();
            ManAppLbl = new System.Windows.Forms.Label();
            titleBar = new System.Windows.Forms.Panel();
            lblTitle = new System.Windows.Forms.Label();
            btnMinimize = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)unPin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gitHub).BeginInit();
            ((System.ComponentModel.ISupportInitialize)autoCrackOff).BeginInit();
            ((System.ComponentModel.ISupportInitialize)autoCrackOn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)donePic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)startCrackPic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)selectDir).BeginInit();
            ManAppPanel.SuspendLayout();
            titleBar.SuspendLayout();
            SuspendLayout();
            // 
            // searchTextBox
            // 
            searchTextBox.BackColor = System.Drawing.Color.FromArgb(8, 8, 12);
            resources.ApplyResources(searchTextBox, "searchTextBox");
            searchTextBox.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.MouseClick += searchTextBox_MouseClick;
            searchTextBox.TextChanged += searchTextBox_TextChanged;
            searchTextBox.Enter += searchTextBox_Enter;
            searchTextBox.KeyDown += searchTextBox_KeyDown;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(0, 5, 25);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.SpringGreen;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(5, 10, 30);
            dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(0, 25, 75);
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Ivory;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(0, 25, 75);
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Ivory;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(dataGridView1, "dataGridView1");
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(0, 2, 10);
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SpringGreen;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            dataGridView1.GridColor = System.Drawing.Color.White;
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(0, 5, 25);
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.SteelBlue;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(0, 10, 35);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(20, 111, 65);
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.Black;
            dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle5;
            dataGridView1.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(0, 5, 25);
            dataGridView1.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridView1.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(30, 70, 100);
            dataGridView1.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            dataGridView1.RowTemplate.ReadOnly = true;
            dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.StandardTab = true;
            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            dataGridView1.RowHeaderMouseClick += dataGridView1_RowHeaderMouseClick;
            dataGridView1.KeyDown += dataGridView1_KeyDown;
            // 
            // btnSearch
            // 
            resources.ApplyResources(btnSearch, "btnSearch");
            btnSearch.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
            btnSearch.Name = "btnSearch";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // resinstruccZip
            // 
            resources.ApplyResources(resinstruccZip, "resinstruccZip");
            resinstruccZip.BackColor = System.Drawing.Color.Transparent;
            resinstruccZip.ForeColor = System.Drawing.Color.White;
            resinstruccZip.Name = "resinstruccZip";
            resinstruccZip.Click += resinstruccZip_Click;
            // 
            // tgDisc
            // 
            resources.ApplyResources(tgDisc, "tgDisc");
            tgDisc.BackColor = System.Drawing.Color.Transparent;
            tgDisc.ForeColor = System.Drawing.Color.FromArgb(128, 255, 255);
            tgDisc.Name = "tgDisc";
            tgDisc.Click += label2_Click;
            // 
            // mainPanel
            // 
            mainPanel.Controls.Add(UploadZipButton);
            mainPanel.Controls.Add(pin);
            mainPanel.Controls.Add(unPin);
            mainPanel.Controls.Add(gitHub);
            mainPanel.Controls.Add(ShareButton);
            mainPanel.Controls.Add(RequestButton);
            mainPanel.Controls.Add(autoCrackOff);
            mainPanel.Controls.Add(autoCrackOn);
            mainPanel.Controls.Add(StatusLabel);
            mainPanel.Controls.Add(tgDisc);
            mainPanel.Controls.Add(OpenDir);
            mainPanel.Controls.Add(ZipToShare);
            mainPanel.Controls.Add(currDIrText);
            mainPanel.Controls.Add(dllSelect);
            mainPanel.Controls.Add(lanMultiplayerCheckBox);
            mainPanel.Controls.Add(donePic);
            mainPanel.Controls.Add(startCrackPic);
            mainPanel.Controls.Add(selectDir);
            mainPanel.Controls.Add(label5);
            mainPanel.Controls.Add(label1);
            mainPanel.Controls.Add(selectDirLabel);
            mainPanel.Controls.Add(drgdropText);
            resources.ApplyResources(mainPanel, "mainPanel");
            mainPanel.Name = "mainPanel";
            mainPanel.Click += Form_Click;
            mainPanel.DragDrop += mainPanel_DragDrop;
            mainPanel.DragEnter += mainPanel_DragEnter;
            mainPanel.DragLeave += mainPanel_DragLeave;
            // 
            // UploadZipButton
            // 
            UploadZipButton.BackColor = System.Drawing.Color.ForestGreen;
            UploadZipButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(55, 55, 60);
            UploadZipButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(28, 28, 32);
            UploadZipButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(48, 48, 52);
            resources.ApplyResources(UploadZipButton, "UploadZipButton");
            UploadZipButton.ForeColor = System.Drawing.Color.FromArgb(220, 220, 225);
            UploadZipButton.Name = "UploadZipButton";
            UploadZipButton.UseVisualStyleBackColor = false;
            // 
            // pin
            // 
            pin.BackColor = System.Drawing.Color.Transparent;
            pin.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(pin, "pin");
            pin.Name = "pin";
            pin.TabStop = false;
            lanMultiplayerToolTip.SetToolTip(pin, resources.GetString("pin.ToolTip"));
            pin.Click += pin_Click;
            // 
            // unPin
            // 
            unPin.BackColor = System.Drawing.Color.Transparent;
            unPin.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(unPin, "unPin");
            unPin.Name = "unPin";
            unPin.TabStop = false;
            lanMultiplayerToolTip.SetToolTip(unPin, resources.GetString("unPin.ToolTip"));
            unPin.Click += unPin_Click;
            // 
            // gitHub
            // 
            gitHub.BackColor = System.Drawing.Color.Transparent;
            gitHub.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(gitHub, "gitHub");
            gitHub.Name = "gitHub";
            gitHub.TabStop = false;
            gitHub.Click += pictureBox1_Click;
            // 
            // ShareButton
            // 
            resources.ApplyResources(ShareButton, "ShareButton");
            ShareButton.BackColor = System.Drawing.Color.Transparent;
            ShareButton.Cursor = System.Windows.Forms.Cursors.Hand;
            ShareButton.FlatAppearance.BorderSize = 0;
            ShareButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(30, 255, 255, 255);
            ShareButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(20, 255, 255, 255);
            ShareButton.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
            ShareButton.Name = "ShareButton";
            lanMultiplayerToolTip.SetToolTip(ShareButton, resources.GetString("ShareButton.ToolTip"));
            ShareButton.UseVisualStyleBackColor = false;
            ShareButton.Click += ShareButton_Click;
            // 
            // RequestButton
            // 
            RequestButton.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
            RequestButton.FlatAppearance.BorderSize = 0;
            RequestButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            RequestButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            resources.ApplyResources(RequestButton, "RequestButton");
            RequestButton.ForeColor = System.Drawing.Color.FromArgb(255, 150, 150);
            RequestButton.Name = "RequestButton";
            RequestButton.UseVisualStyleBackColor = false;
            // 
            // autoCrackOff
            // 
            autoCrackOff.BackColor = System.Drawing.Color.Transparent;
            autoCrackOff.Cursor = System.Windows.Forms.Cursors.Hand;
            autoCrackOff.Image = APPID.Properties.Resources._1111;
            resources.ApplyResources(autoCrackOff, "autoCrackOff");
            autoCrackOff.Name = "autoCrackOff";
            autoCrackOff.TabStop = false;
            lanMultiplayerToolTip.SetToolTip(autoCrackOff, resources.GetString("autoCrackOff.ToolTip"));
            autoCrackOff.Click += autoCrackOff_Click;
            // 
            // autoCrackOn
            // 
            autoCrackOn.BackColor = System.Drawing.Color.Transparent;
            autoCrackOn.Cursor = System.Windows.Forms.Cursors.Hand;
            autoCrackOn.Image = APPID.Properties.Resources._22222;
            resources.ApplyResources(autoCrackOn, "autoCrackOn");
            autoCrackOn.Name = "autoCrackOn";
            autoCrackOn.TabStop = false;
            lanMultiplayerToolTip.SetToolTip(autoCrackOn, resources.GetString("autoCrackOn.ToolTip"));
            autoCrackOn.Click += autoCrackOn_Click;
            // 
            // StatusLabel
            // 
            StatusLabel.AllowDrop = true;
            StatusLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(StatusLabel, "StatusLabel");
            StatusLabel.ForeColor = System.Drawing.Color.FromArgb(100, 200, 255);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.DragDrop += mainPanel_DragDrop;
            StatusLabel.DragEnter += mainPanel_DragEnter;
            StatusLabel.DragLeave += mainPanel_DragLeave;
            // 
            // OpenDir
            // 
            OpenDir.BackColor = System.Drawing.Color.FromArgb(180, 110, 180);
            OpenDir.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(55, 55, 60);
            OpenDir.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(28, 28, 32);
            OpenDir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(48, 48, 52);
            resources.ApplyResources(OpenDir, "OpenDir");
            OpenDir.ForeColor = System.Drawing.Color.FromArgb(220, 220, 225);
            OpenDir.Name = "OpenDir";
            OpenDir.TabStop = false;
            OpenDir.UseVisualStyleBackColor = false;
            OpenDir.Click += OpenDir_Click;
            // 
            // ZipToShare
            // 
            ZipToShare.BackColor = System.Drawing.Color.CadetBlue;
            ZipToShare.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(55, 55, 60);
            ZipToShare.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(28, 28, 32);
            ZipToShare.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(48, 48, 52);
            resources.ApplyResources(ZipToShare, "ZipToShare");
            ZipToShare.ForeColor = System.Drawing.Color.FromArgb(220, 220, 225);
            ZipToShare.Name = "ZipToShare";
            ZipToShare.TabStop = false;
            lanMultiplayerToolTip.SetToolTip(ZipToShare, resources.GetString("ZipToShare.ToolTip"));
            ZipToShare.UseVisualStyleBackColor = false;
            ZipToShare.Click += ZipToShare_Click;
            // 
            // currDIrText
            // 
            resources.ApplyResources(currDIrText, "currDIrText");
            currDIrText.ForeColor = System.Drawing.Color.AliceBlue;
            currDIrText.Name = "currDIrText";
            // 
            // dllSelect
            // 
            dllSelect.AutoCompleteCustomSource.AddRange(new string[] { resources.GetString("dllSelect.AutoCompleteCustomSource"), resources.GetString("dllSelect.AutoCompleteCustomSource1") });
            dllSelect.BorderColor = System.Drawing.Color.FromArgb(100, 150, 200);
            dllSelect.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            dllSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(dllSelect, "dllSelect");
            dllSelect.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
            dllSelect.FormattingEnabled = true;
            dllSelect.Items.AddRange(new object[] { resources.GetString("dllSelect.Items"), resources.GetString("dllSelect.Items1") });
            dllSelect.Name = "dllSelect";
            dllSelect.SelectedIndexChanged += dllSelect_SelectedIndexChanged;
            // 
            // lanMultiplayerCheckBox
            // 
            resources.ApplyResources(lanMultiplayerCheckBox, "lanMultiplayerCheckBox");
            lanMultiplayerCheckBox.BackColor = System.Drawing.Color.Transparent;
            lanMultiplayerCheckBox.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
            lanMultiplayerCheckBox.Name = "lanMultiplayerCheckBox";
            lanMultiplayerToolTip.SetToolTip(lanMultiplayerCheckBox, resources.GetString("lanMultiplayerCheckBox.ToolTip"));
            lanMultiplayerCheckBox.UseVisualStyleBackColor = false;
            lanMultiplayerCheckBox.CheckedChanged += lanMultiplayerCheckBox_CheckedChanged;
            // 
            // donePic
            // 
            donePic.BackColor = System.Drawing.Color.Transparent;
            donePic.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(donePic, "donePic");
            donePic.Name = "donePic";
            donePic.TabStop = false;
            donePic.Click += pictureBox2_Click;
            // 
            // startCrackPic
            // 
            startCrackPic.BackColor = System.Drawing.Color.Transparent;
            startCrackPic.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(startCrackPic, "startCrackPic");
            startCrackPic.Name = "startCrackPic";
            startCrackPic.TabStop = false;
            startCrackPic.Click += startCrackPic_Click;
            startCrackPic.MouseDown += startCrackPic_MouseDown;
            startCrackPic.MouseEnter += startCrackPic_MouseEnter;
            startCrackPic.MouseLeave += startCrackPic_MouseLeave;
            startCrackPic.MouseHover += startCrackPic_MouseHover;
            // 
            // selectDir
            // 
            selectDir.BackColor = System.Drawing.Color.Transparent;
            selectDir.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(selectDir, "selectDir");
            selectDir.Name = "selectDir";
            selectDir.TabStop = false;
            selectDir.Click += pictureBox2_Click;
            selectDir.MouseDown += selectDir_MouseDown;
            selectDir.MouseEnter += selectDir_MouseEnter;
            selectDir.MouseLeave += selectDir_MouseLeave;
            selectDir.MouseHover += selectDir_MouseHover;
            // 
            // label5
            // 
            label5.AllowDrop = true;
            label5.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(label5, "label5");
            label5.ForeColor = System.Drawing.Color.LightSteelBlue;
            label5.Name = "label5";
            label5.Click += label5_Click;
            label5.DragDrop += mainPanel_DragDrop;
            label5.DragEnter += mainPanel_DragEnter;
            label5.DragLeave += mainPanel_DragLeave;
            // 
            // label1
            // 
            label1.AllowDrop = true;
            resources.ApplyResources(label1, "label1");
            label1.BackColor = System.Drawing.Color.Transparent;
            label1.Cursor = System.Windows.Forms.Cursors.Hand;
            label1.ForeColor = System.Drawing.Color.White;
            label1.Name = "label1";
            label1.DragDrop += mainPanel_DragDrop;
            label1.DragEnter += mainPanel_DragEnter;
            label1.DragLeave += mainPanel_DragLeave;
            // 
            // selectDirLabel
            // 
            selectDirLabel.AllowDrop = true;
            resources.ApplyResources(selectDirLabel, "selectDirLabel");
            selectDirLabel.BackColor = System.Drawing.Color.Transparent;
            selectDirLabel.ForeColor = System.Drawing.Color.White;
            selectDirLabel.Name = "selectDirLabel";
            selectDirLabel.DragDrop += mainPanel_DragDrop;
            selectDirLabel.DragEnter += mainPanel_DragEnter;
            selectDirLabel.DragLeave += mainPanel_DragLeave;
            // 
            // drgdropText
            // 
            drgdropText.AllowDrop = true;
            drgdropText.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(drgdropText, "drgdropText");
            drgdropText.ForeColor = System.Drawing.SystemColors.Control;
            drgdropText.Name = "drgdropText";
            drgdropText.DragDrop += mainPanel_DragDrop;
            drgdropText.DragEnter += mainPanel_DragEnter;
            drgdropText.DragLeave += mainPanel_DragLeave;
            // 
            // btnManualEntry
            // 
            btnManualEntry.BackColor = System.Drawing.Color.FromArgb(38, 38, 42);
            btnManualEntry.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(55, 55, 60);
            btnManualEntry.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(28, 28, 32);
            btnManualEntry.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(48, 48, 52);
            resources.ApplyResources(btnManualEntry, "btnManualEntry");
            btnManualEntry.ForeColor = System.Drawing.Color.FromArgb(220, 220, 225);
            btnManualEntry.Name = "btnManualEntry";
            btnManualEntry.UseVisualStyleBackColor = false;
            btnManualEntry.Click += btnManualEntry_Click;
            // 
            // lanMultiplayerToolTip
            // 
            lanMultiplayerToolTip.AutoPopDelay = 15000;
            lanMultiplayerToolTip.InitialDelay = 500;
            lanMultiplayerToolTip.ReshowDelay = 100;
            lanMultiplayerToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            lanMultiplayerToolTip.ToolTipTitle = "Info";
            // 
            // ManAppPanel
            // 
            ManAppPanel.BackColor = System.Drawing.Color.FromArgb(200, 0, 0, 0);
            ManAppPanel.Controls.Add(ManAppBox);
            ManAppPanel.Controls.Add(ManAppBtn);
            ManAppPanel.Controls.Add(ManAppLbl);
            resources.ApplyResources(ManAppPanel, "ManAppPanel");
            ManAppPanel.Name = "ManAppPanel";
            ManAppPanel.Paint += ManAppPanel_Paint;
            // 
            // ManAppBox
            // 
            ManAppBox.BackColor = System.Drawing.Color.FromArgb(8, 8, 12);
            ManAppBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(ManAppBox, "ManAppBox");
            ManAppBox.ForeColor = System.Drawing.Color.FromArgb(220, 255, 255);
            ManAppBox.Name = "ManAppBox";
            ManAppBox.TextChanged += ManAppBox_TextChanged;
            ManAppBox.KeyDown += ManAppBox_KeyDown;
            // 
            // ManAppBtn
            // 
            ManAppBtn.BackColor = System.Drawing.Color.FromArgb(80, 15, 25, 45);
            ManAppBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(100, 150, 200);
            ManAppBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(120, 30, 50, 80);
            resources.ApplyResources(ManAppBtn, "ManAppBtn");
            ManAppBtn.ForeColor = System.Drawing.Color.FromArgb(192, 255, 255);
            ManAppBtn.Name = "ManAppBtn";
            ManAppBtn.UseVisualStyleBackColor = false;
            ManAppBtn.Click += ManAppBtn_Click;
            // 
            // ManAppLbl
            // 
            ManAppLbl.AllowDrop = true;
            resources.ApplyResources(ManAppLbl, "ManAppLbl");
            ManAppLbl.BackColor = System.Drawing.Color.Transparent;
            ManAppLbl.ForeColor = System.Drawing.Color.FromArgb(200, 220, 255);
            ManAppLbl.Name = "ManAppLbl";
            ManAppLbl.DragDrop += mainPanel_DragDrop;
            ManAppLbl.DragEnter += mainPanel_DragEnter;
            ManAppLbl.DragLeave += mainPanel_DragLeave;
            // 
            // titleBar
            // 
            titleBar.BackColor = System.Drawing.Color.FromArgb(23, 0, 0, 0);
            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnMinimize);
            titleBar.Controls.Add(btnClose);
            resources.ApplyResources(titleBar, "titleBar");
            titleBar.Name = "titleBar";
            titleBar.MouseDown += TitleBar_MouseDown;
            // 
            // lblTitle
            // 
            resources.ApplyResources(lblTitle, "lblTitle");
            lblTitle.BackColor = System.Drawing.Color.Transparent;
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(200, 200, 210);
            lblTitle.Name = "lblTitle";
            lblTitle.MouseDown += TitleBar_MouseDown;
            // 
            // btnMinimize
            // 
            resources.ApplyResources(btnMinimize, "btnMinimize");
            btnMinimize.BackColor = System.Drawing.Color.Transparent;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(120, 40, 40, 45);
            btnMinimize.ForeColor = System.Drawing.Color.FromArgb(200, 200, 210);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.UseVisualStyleBackColor = false;
            btnMinimize.Click += BtnMinimize_Click;
            // 
            // btnClose
            // 
            resources.ApplyResources(btnClose, "btnClose");
            btnClose.BackColor = System.Drawing.Color.Transparent;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(180, 150, 0, 0);
            btnClose.ForeColor = System.Drawing.Color.FromArgb(200, 200, 210);
            btnClose.Name = "btnClose";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += BtnClose_Click;
            // 
            // SteamAppId
            // 
            AllowDrop = true;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(5, 8, 20);
            Controls.Add(titleBar);
            Controls.Add(mainPanel);
            Controls.Add(btnManualEntry);
            Controls.Add(dataGridView1);
            Controls.Add(ManAppPanel);
            Controls.Add(searchTextBox);
            Controls.Add(btnSearch);
            Controls.Add(resinstruccZip);
            ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "SteamAppId";
            FormClosing += SteamAppId_FormClosing;
            Load += SteamAppId_Load;
            Click += Form_Click;
            DragDrop += mainPanel_DragDrop;
            DragEnter += mainPanel_DragEnter;
            DragLeave += mainPanel_DragLeave;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pin).EndInit();
            ((System.ComponentModel.ISupportInitialize)unPin).EndInit();
            ((System.ComponentModel.ISupportInitialize)gitHub).EndInit();
            ((System.ComponentModel.ISupportInitialize)autoCrackOff).EndInit();
            ((System.ComponentModel.ISupportInitialize)autoCrackOn).EndInit();
            ((System.ComponentModel.ISupportInitialize)donePic).EndInit();
            ((System.ComponentModel.ISupportInitialize)startCrackPic).EndInit();
            ((System.ComponentModel.ISupportInitialize)selectDir).EndInit();
            ManAppPanel.ResumeLayout(false);
            ManAppPanel.PerformLayout();
            titleBar.ResumeLayout(false);
            titleBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

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

