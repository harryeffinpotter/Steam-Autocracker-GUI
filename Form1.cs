using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CliWrap;
using CliWrap.Buffered;
using SAC_GUI;
using SteamAppIdIdentifier;
using SteamAutocrackGUI;
using Clipboard = System.Windows.Forms.Clipboard;
using DataFormats = System.Windows.Forms.DataFormats;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using Timer = System.Windows.Forms.Timer;

namespace APPID
{
    public partial class SteamAppId : Form
    {
        #region Windows API for Acrylic Blur
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttribData data);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttribData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        private const int WCA_ACCENT_POLICY = 19;
        private const int ACCENT_ENABLE_ACRYLICBLURBEHIND = 4;
        #endregion

        protected DataTableGeneration dataTableGeneration;
        public static int CurrentCell = 0;
        public static string APPNAME = "";
        public static string APPDATA = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public SteamAppId()
        {
            ServicePointManager.ServerCertificateValidationCallback =
            (s, certificate, chain, sslPolicyErrors) => true;
            dataTableGeneration = new DataTableGeneration();
            Task.Run(async () => await dataTableGeneration.GetDataTableAsync(dataTableGeneration)).Wait();
            InitializeComponent();
            InitializeTimers();

            // Apply acrylic blur and rounded corners
            this.Load += (s, e) => ApplyAcrylicEffect();

            // Initialize HWID and user tracking
            _ = InitializeUserTracking();
        }

        private void ApplyAcrylicEffect()
        {
            // Apply rounded corners (Windows 11)
            try
            {
                int preference = DWMWCP_ROUND;
                DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
            }
            catch { }

            // Apply acrylic blur effect
            try
            {
                var accent = new AccentPolicy();
                accent.AccentState = ACCENT_ENABLE_ACRYLICBLURBEHIND;
                accent.AccentFlags = 2;
                // Dark tinted glass: ABGR format (Alpha, Blue, Green, Red)
                // 0xBB = ~73% opacity, balanced blur and darkness
                accent.GradientColor = unchecked((int)0xBB0A0A0F); // Dark with good blur

                int accentStructSize = Marshal.SizeOf(accent);
                IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttribData();
                data.Attribute = WCA_ACCENT_POLICY;
                data.SizeOfData = accentStructSize;
                data.Data = accentPtr;

                SetWindowCompositionAttribute(this.Handle, ref data);

                Marshal.FreeHGlobal(accentPtr);
            }
            catch { }
        }

        // Custom title bar drag functionality
        private Point mouseDownPoint = Point.Empty;

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDownPoint = new Point(e.X, e.Y);
                this.Cursor = Cursors.SizeAll;

                var titleBarControl = sender as Control;
                titleBarControl.MouseMove += TitleBar_MouseMove;
                titleBarControl.MouseUp += TitleBar_MouseUp;
            }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && mouseDownPoint != Point.Empty)
            {
                this.Location = new Point(
                    this.Location.X + e.X - mouseDownPoint.X,
                    this.Location.Y + e.Y - mouseDownPoint.Y
                );
            }
        }

        private void TitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Default;
            mouseDownPoint = Point.Empty;

            var titleBarControl = sender as Control;
            titleBarControl.MouseMove -= TitleBar_MouseMove;
            titleBarControl.MouseUp -= TitleBar_MouseUp;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private async Task InitializeUserTracking()
        {
            try
            {
                var userId = await HWIDManager.InitializeHWID();
                System.Diagnostics.Debug.WriteLine($"[USER] Initialized with ID: {userId}");
                System.Diagnostics.Debug.WriteLine($"[USER] Current Honor Score: {HWIDManager.GetHonorScore()}");

                // Update UI with honor score
                this.Invoke(new Action(() =>
                {
                    UpdateHonorDisplay();
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[USER] Failed to initialize tracking: {ex.Message}");
            }
        }

        private void UpdateHonorDisplay()
        {
            var honorScore = HWIDManager.GetHonorScore();
            if (honorScore > 0)
            {
                // Update the window title with honor score
                this.Text = $"{this.Text.Split('-')[0].Trim()} - ⭐Honor: {honorScore}";
            }
        }

        private void InitializeTimers()
        {
            // Timer for label5 - visible for 10 seconds
            label5Timer = new Timer();
            label5Timer.Interval = 10000; // 10 seconds
            label5Timer.Tick += (s, e) => {
                label5.Visible = false;
                label5Timer.Stop();
            };

            // Timer for resinstruccZip - disappear after 30 seconds
            resinstruccZipTimer = new Timer();
            resinstruccZipTimer.Interval = 30000; // 30 seconds
            resinstruccZipTimer.Tick += (s, e) => {
                resinstruccZip.Visible = false;
                resinstruccZipTimer.Stop();
            };

            // Start label5 timer on form load
            label5.Visible = true;
            label5Timer.Start();
        }
        public static void Tit(string Message, Color color)
        {
            // Handle cross-thread calls
            if (SteamAppIdIdentifier.Program.form.InvokeRequired)
            {
                SteamAppIdIdentifier.Program.form.Invoke(new Action(() => Tit(Message, color)));
                return;
            }

            string MessageLow = Message.ToLower();
            if (MessageLow.Contains("READY TO CRACK!".ToLower()))
            {
                SteamAppIdIdentifier.Program.form.StatusLabel.ForeColor = Color.HotPink;
            }
            else if (MessageLow.Contains("Complete".ToLower()))
            {
                SteamAppIdIdentifier.Program.form.StatusLabel.ForeColor = Color.MediumSpringGreen;
            }
            else if (MessageLow.Contains("no steam".ToLower()))
            {
                SteamAppIdIdentifier.Program.form.StatusLabel.ForeColor = Color.Crimson;
            }
            else
            {
                SteamAppIdIdentifier.Program.form.StatusLabel.ForeColor = color;
            }
            SteamAppIdIdentifier.Program.form.StatusLabel.Text = Message;
            SteamAppIdIdentifier.Program.form.Text = Message.Replace("&&", "&");
        }
        public static void Tat(string Message)
        {
            // Handle cross-thread calls
            if (SteamAppIdIdentifier.Program.form.InvokeRequired)
            {
                SteamAppIdIdentifier.Program.form.Invoke(new Action(() => Tat(Message)));
                return;
            }
            SteamAppIdIdentifier.Program.form.currDIrText.Text = $"{Message}";
        }
        public static bool VRLExists = false;

        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9._0-]+", " ", RegexOptions.Compiled);
        }

        public static string ImprovedGameNameCleaning(string gameName)
        {
            string cleaned = gameName.ToLower().Trim();

            // Remove common gaming suffixes/prefixes that can cause mismatches
            string[] removeParts = {
                "definitive edition", "deluxe edition", "gold edition", "complete edition",
                "enhanced edition", "directors cut", "game of the year", "goty", "remastered",
                "redux", "ultimate", "premium", "collectors edition", "special edition",
                "anniversary edition", "standard edition", "base game", "digital deluxe",
                "legendary edition", "epic edition", "expanded edition"
            };

            foreach (string part in removeParts)
            {
                cleaned = cleaned.Replace(part, "");
            }

            // Remove years (4 digits)
            cleaned = Regex.Replace(cleaned, @"\b(19|20)\d{2}\b", "", RegexOptions.Compiled);

            // Remove platform indicators
            cleaned = Regex.Replace(cleaned, @"\b(pc|steam|windows|x64|x86)\b", "", RegexOptions.Compiled);

            // Normalize spacing and remove extra characters
            cleaned = RemoveSpecialCharacters(cleaned);
            cleaned = Regex.Replace(cleaned, @"\s+", " ", RegexOptions.Compiled).Trim();

            return cleaned;
        }

        private void SetSelectedGame(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count)
            {
                APPID = dataGridView1[1, rowIndex].Value.ToString();
                APPNAME = dataGridView1[0, rowIndex].Value.ToString().Trim();
                Tat($"{APPNAME} ({APPID})");
                searchTextBox.Clear();
                btnManualEntry.Visible = false;
                mainPanel.Visible = true;
                startCrackPic.Visible = true;
                Tit("READY! Click skull folder above to perform crack!", Color.HotPink);
                resinstruccZip.Visible = false;

                // Auto-crack if enabled
                if (autoCrackEnabled && !string.IsNullOrEmpty(gameDir))
                {
                    // Trigger crack just like clicking the button
                    startCrackPic_Click(null, null);
                }
            }
        }

        // Removed duplicate SetSelectedGame - using only the rowIndex version

        private bool PerformImprovedFuzzySearch(string gameFolderName)
        {
            // Try original search method first
            string Search = RemoveSpecialCharacters(gameFolderName.ToLower()).Trim();
            string SplitSearch = SplitCamelCase(RemoveSpecialCharacters(gameFolderName)).Trim();

            // Level 1: Exact match attempts
            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + Search.Replace("_", "").Trim() + "'");
            if (dataGridView1.Rows.Count > 0) return true;

            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + SplitSearch.Replace("_", "").Trim() + "'");
            if (dataGridView1.Rows.Count > 0) return true;

            // Level 2: Remove common gaming terms
            string improvedClean = ImprovedGameNameCleaning(gameFolderName);
            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + improvedClean.Replace(" ", "%' AND Name LIKE '%") + "%'");
            if (dataGridView1.Rows.Count > 0) return true;

            // Level 3: Original fallback patterns
            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + SplitSearch.ToLower().Replace("_", "").Replace("vr", "").Replace("vrs", "").Trim() + "'");
            if (dataGridView1.Rows.Count > 0) return true;

            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + SplitSearch.ToLower().Replace("'", "").Replace("-", "").Replace(";", "").Trim() + "'");
            if (dataGridView1.Rows.Count > 0) return true;

            // Level 4: Partial word matching
            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + Search.Replace("_", "").Replace(" ", "%' AND Name LIKE '%").Replace(" and ", " ").Replace(" the ", " ").Replace(":", "") + "%'");
            if (dataGridView1.Rows.Count > 0) return true;

            // Level 5: Try individual words for partial matches
            string[] words = improvedClean.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1)
            {
                // Try matching with the longest word first
                var sortedWords = words.OrderByDescending(w => w.Length).Where(w => w.Length > 3).Take(3);
                foreach (string word in sortedWords)
                {
                    ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + word + "%'");
                    if (dataGridView1.Rows.Count > 0) return true;
                }
            }

            // Level 6: Last resort - refresh data and try again
            dataTableGeneration = new DataTableGeneration();
            Task.Run(async () => await dataTableGeneration.GetDataTableAsync(dataTableGeneration)).Wait();
            dataGridView1.DataSource = dataTableGeneration.DataTableToGenerate;

            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + improvedClean.Replace(" ", "%' AND Name LIKE '%") + "%'");
            if (dataGridView1.Rows.Count > 0) return true;

            // No good match found
            return false;
        }
        public async void SteamAppId_Load(object sender, EventArgs e)
        {
            lanMultiplayerCheckBox.Checked = enableLanMultiplayer;
            if (Properties.Settings.Default.Pinned)
            {
                this.TopMost = true;
                unPin.BringToFront();
            }

            // Load auto-crack setting
            // Check saved preference, but default to ON if not saved
            if (!Properties.Settings.Default.AutoCrack)
            {
                autoCrackEnabled = false;
                autoCrackOff.BringToFront();
            }
            else
            {
                // Default to ON
                autoCrackEnabled = true;
                autoCrackOn.BringToFront();
                Properties.Settings.Default.AutoCrack = true;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.Goldy)
            {
                dllSelect.SelectedIndex = 0;
                lanMultiplayerCheckBox.Visible = true;
            }
            else
            {
                dllSelect.SelectedIndex = 1;
                lanMultiplayerCheckBox.Visible = false;
            }
            Tit("Checking for Internet... ", Color.LightSkyBlue);
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            bool check = Updater.CheckForNet();
            if (check)
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                Tit("Checking for updates", Color.LightSkyBlue);
                await Updater.CheckGitHubNewerVersion("atom0s", "Steamless", "https://api.github.com/repos");
                await Updater.UpdateGoldBergAsync();
                await Task.Delay(1500);
                Tit("Click folder && select game's parent directory.", Color.Cyan);
            }

            t1 = new Timer();
            t1.Tick += new EventHandler(t1_Tick);
            t1.Interval = 1000;
            if (Directory.Exists($"{APPDATA}\\VRL"))
            {
                VRLExists = true;
            }
            this.TopMost = true;
            string args3;
            dataGridView1.DataSource = dataTableGeneration.DataTableToGenerate;
            dataGridView1.MultiSelect = false;
            string args2 = "";
            dataGridView1.Columns[0].Width = 540;




            if (args2 != null)
            {
                foreach (string args1 in SteamAppIdIdentifier.Program.args2)
                {
                    args3 = RemoveSpecialCharacters(args1);
                    args2 += args3 + " ";
                }
                try
                {
                    string Search = RemoveSpecialCharacters(args2.ToLower()).Trim();
                    string SplitSearch = SplitCamelCase(RemoveSpecialCharacters(args2)).Trim();
                    ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + Search.Replace("_", "").Trim() + "'");

                    if (dataGridView1.Rows.Count == 0)
                        ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + SplitSearch.Replace("_", "").Trim() + "'");

                    if (dataGridView1.Rows.Count == 0)
                        ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + SplitSearch.ToLower().Replace("_", "").Replace("vr", "").Replace("vrs", "").Trim() + "'");

                    if (dataGridView1.Rows.Count == 0)
                        ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + SplitSearch.ToLower().Replace("'", "").Replace("-", "").Replace(";", "").Trim() + "'");

                    if (dataGridView1.Rows.Count == 0)
                    {

                        ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + Search.Replace("_", "").Replace(" ", "%' AND Name LIKE '%").Replace(" and ", " ").Replace(" the ", " ").Replace(":", "") + "%'");
                        if (dataGridView1.Rows.Count > 0)
                        {
                            // Don't auto-select here - let it fall through to SearchComplete
                        }
                        else
                        {
                            dataTableGeneration = new DataTableGeneration();
                            Task.Run(async () => await dataTableGeneration.GetDataTableAsync(dataTableGeneration)).Wait();
                            dataGridView1.DataSource = dataTableGeneration.DataTableToGenerate;
                            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + SplitSearch.Replace("_", "").Replace(" ", "%' AND Name LIKE '%").Replace(" and ", " ").Replace(" the ", " ").Replace(":", "") + "%'").Trim();
                            if (dataGridView1.Rows.Count > 0)
                            {
                                Clipboard.SetText($"{dataGridView1.Rows[0].Cells[1].Value.ToString()}");
                            }
                            else
                            {
                                dataTableGeneration = new DataTableGeneration();
                                Task.Run(async () => await dataTableGeneration.GetDataTableAsync(dataTableGeneration)).Wait();
                                dataGridView1.DataSource = dataTableGeneration.DataTableToGenerate;

                            }

                        }
                    }
                }
                catch { }
                searchTextBox.Text = "";

            }
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.ClearSelection();
            dataGridView1.DefaultCellStyle.SelectionBackColor = dataGridView1.DefaultCellStyle.BackColor;
            dataGridView1.DefaultCellStyle.SelectionForeColor = dataGridView1.DefaultCellStyle.ForeColor;
        }

        public static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            searchTextBox.Clear();
            searchTextBox.Focus();
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dataGridView1[1, e.RowIndex].Value == null)
                {
                    return;
                }
                if (dataGridView1.SelectedCells.Count > 0)
                {
                    CurrentCell = e.RowIndex;
                }
            }
            catch { }


        }
        public static String APPID;
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1[1, e.RowIndex].Value == null)
            {
                return;
            }
            try
            {
                if (VRLExists)
                {
                    string PropName = RemoveSpecialCharacters(dataGridView1[0, e.RowIndex].Value.ToString());
                    File.WriteAllText($"{APPDATA}\\VRL\\ProperName.txt", PropName);
                }
                APPID = dataGridView1[1, e.RowIndex].Value.ToString();
                APPNAME = dataGridView1[0, e.RowIndex].Value.ToString().Trim();
                Tat($"{APPNAME} ({APPID})");
                searchTextBox.Clear();
                mainPanel.Visible = true;
                btnManualEntry.Visible = false;
                startCrackPic.Visible = true;
                Tit("READY! Click skull folder above to perform crack!", Color.HotPink);

                resinstruccZip.Visible = false;

                // Auto-crack if enabled
                if (autoCrackEnabled && !string.IsNullOrEmpty(gameDir))
                {
                    // Trigger crack just like clicking the button
                    startCrackPic_Click(null, null);
                }
            }
            catch { }

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (VRLExists)
                {
                    string PropName = RemoveSpecialCharacters(dataGridView1[0, e.RowIndex].Value.ToString());
                    File.WriteAllText($"{APPDATA}\\VRL\\ProperName.txt", PropName);
                }
                SetSelectedGame(e.RowIndex);
            }
            catch
            {
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Up)
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        if (dataGridView1.Rows[0].Selected)
                        {
                            searchTextBox.Focus();
                        }
                    }
                }
                if (e.KeyCode == Keys.Escape)
                {
                    searchTextBox.Clear();
                    searchTextBox.Focus();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    if (dataGridView1[1, CurrentCell].Value == null)
                    {
                        return;
                    }
                    if (VRLExists)
                    {
                        string PropName = RemoveSpecialCharacters(dataGridView1[0, CurrentCell].Value.ToString());
                        File.WriteAllText($"{APPDATA}\\VRL\\ProperName.txt", PropName);
                    }
                    APPID = dataGridView1[1, CurrentCell].Value.ToString();
                    APPNAME = dataGridView1[0, CurrentCell].Value.ToString().Trim();
                    Tat($"{APPNAME} ({APPID})");
                    searchTextBox.Clear();
                    mainPanel.Visible = true;
                    btnManualEntry.Visible = false;
                    startCrackPic.Visible = true;
                    Tit("READY! Click skull folder above to perform crack!", Color.HotPink);


                }
                else if (e.KeyCode == Keys.Back)
                {
                    string s = searchTextBox.Text;

                    if (s.Length > 1)
                    {
                        s = s.Substring(0, s.Length - 1);
                    }
                    else
                    {
                        s = "0";
                    }

                    searchTextBox.Text = s;
                    searchTextBox.Focus();
                    dataGridView1.ClearSelection();
                    searchTextBox.SelectionStart = searchTextBox.Text.Length;
                }
                else if (e.KeyCode != Keys.Up && e.KeyCode != Keys.Down && char.IsLetterOrDigit((char)e.KeyCode) && e.KeyCode.ToString().Length == 1 || e.KeyCode == Keys.Space)
                {
                    bool isShiftKeyPressed = (e.Modifiers == Keys.Shift);
                    bool isCapsLockOn = System.Windows.Forms.Control
                         .IsKeyLocked(System.Windows.Forms.Keys.CapsLock);
                    if (e.KeyCode == Keys.Space)
                    {
                        searchTextBox.Focus();
                        searchTextBox.Text += " ";
                        dataGridView1.ClearSelection();
                        searchTextBox.SelectionStart = searchTextBox.Text.Length;
                    }
                    else if (!isShiftKeyPressed && !isCapsLockOn)
                    {
                        searchTextBox.Focus();
                        searchTextBox.Text += e.KeyData.ToString().ToLower();
                        searchTextBox.SelectionStart = searchTextBox.Text.Length;
                    }
                    else
                    {
                        searchTextBox.Focus();
                        searchTextBox.Text += e.KeyData.ToString().Replace(", Shift", "").Replace("Oem", "").Replace("numpad", "");
                        searchTextBox.SelectionStart = searchTextBox.Text.Length;
                    }

                }
                else if (e.KeyCode == Keys.F1)
                {
                    ManAppPanel.Visible = true;
                    ManAppBox.Focus();
                    ManAppPanel.BringToFront();
                    ManAppBtn.Visible = true;
            
                }
                else if (e.KeyCode == Keys.Control | e.KeyCode == Keys.I)
                {
                    ManAppPanel.Visible = true;
                    ManAppBox.Focus();
                    ManAppPanel.BringToFront();
                    ManAppBtn.Visible = true;

                }
            }
            catch { }
                
        }


        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {            
                if (e.Modifiers == Keys.LShiftKey && e.KeyCode == Keys.Oem4)
                {
                    searchTextBox.Text = $"${searchTextBox.Text}";
                    searchTextBox.SelectionStart = searchTextBox.Text.Length + 1;
                }
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
                {
                    searchTextBox.SelectAll();
                    e.SuppressKeyPress = true;
                }
                if (dataGridView1.Rows.Count == 0)
                {
                    return;
                }
                if (e.KeyCode == Keys.Down)
                {
                    dataGridView1.Focus();
                    dataGridView1.Rows[0].Selected = true;
                    dataGridView1.SelectedCells[0].Selected = true;

                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Tab)
                {
                    dataGridView1.Focus();
                    dataGridView1.Rows[0].Selected = true;
                    dataGridView1.SelectedCells[0].Selected = true;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    // ENTER PRESSED - GO FUCKING NUTS!
                    if (dataGridView1.Rows.Count == 1)
                    {
                        // ONE MATCH - AUTO SELECT AND NUT GALLONS!
                        SetSelectedGame(0);
                        e.SuppressKeyPress = true;
                    }
                    else if (dataGridView1.Rows.Count > 1)
                    {
                        // Multiple matches - select the first or highlighted one
                        if (dataGridView1.CurrentRow != null)
                        {
                            SetSelectedGame(dataGridView1.CurrentRow.Index);
                        }
                        else
                        {
                            SetSelectedGame(0);
                        }
                        e.SuppressKeyPress = true;
                    }
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    searchTextBox.Clear();
                    searchTextBox.Focus();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Back)
                {
                    BackPressed = true;
                }
                else if (e.KeyCode == Keys.Control && e.KeyCode == Keys.I)
                {
                    ManAppPanel.Visible = true;
                    ManAppBox.Focus();
                    ManAppPanel.BringToFront();
                    ManAppBtn.Visible = true;
                }
                else if (e.KeyCode == Keys.F1)
                {
                    ManAppPanel.Visible = true;
                    ManAppBox.Focus();
                    ManAppPanel.BringToFront();
                    ManAppBtn.Visible = true;
                }
                else
                {
                    BackPressed = false;
                    SearchPause = false;
                    t1.Stop();
                }
          
            }
            catch { }
        }
        public static bool SearchPause = false;
        public static bool BackPressed = false;
        public static void t1_Tick(object sender, EventArgs e)
        {
            BackPressed = false;
            SearchPause = false;
            t1.Stop();
        }
        public static Timer t1;
        private Timer label5Timer;
        private Timer resinstruccZipTimer;
        async Task PutTaskDelay()
        {
            await Task.Delay(1000);
        }
        bool textChanged = false;
        bool isFirstClickAfterSelection = false;  // Track first click after folder/file selection
        bool isInitialFolderSearch = false;  // Track if this is the FIRST search after folder selection

        private async void searchTextBox_TextChanged(object sender, EventArgs e)
        {

            if (SearchPause && searchTextBox.Text.Length > 0)
            {
                return;
            }
            try
            {
                if (textChanged)
                {
                    await PutTaskDelay();
                    textChanged = false;  // Reset the flag
                    return;
                }
                textChanged = true;
                string searchText = searchTextBox.Text.Trim();

                // BLIND SPOT DETECTION: Try to catch exact matches that are being missed
                if (searchText.Length > 2)
                {
                    // First, let's see if there's an exact match hiding in the full dataset
                    DataTable dt = (DataTable)dataGridView1.DataSource;

                    // Check for exact matches manually (case-insensitive)
                    var exactMatches = dt.AsEnumerable().Where(row =>
                        string.Equals(row.Field<string>("Name"), searchText, StringComparison.OrdinalIgnoreCase)).ToList();

                    // JACKBOX DEBUG: Special handling for numbered games that might have filtering issues
                    if (!exactMatches.Any() && searchText.ToLower().Contains("party pack"))
                    {
                        // Look for all party pack entries and see what's different
                        var allPartyPacks = dt.AsEnumerable().Where(row =>
                            row.Field<string>("Name")?.ToLower().Contains("party pack") == true).ToList();

                        // Try to find the exact match by checking each one manually
                        foreach (var pack in allPartyPacks)
                        {
                            string packName = pack.Field<string>("Name");
                            if (string.Equals(packName, searchText, StringComparison.OrdinalIgnoreCase))
                            {
                                exactMatches.Add(pack);
                                break;
                            }
                        }
                    }

                    if (exactMatches.Any())
                    {
                        // Found exact match! Apply filter to show only this
                        string exactName = exactMatches.First().Field<string>("Name");
                        ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name = '{0}'", exactName.Replace("'", "''"));
                        goto SearchComplete;
                    }

                    // SOUNDTRACK/DLC PRIORITY FIX: Check if we're finding longer titles when shorter exact ones exist
                    if (searchText.Length > 3)
                    {
                        var partialMatches = dt.AsEnumerable().Where(row =>
                            row.Field<string>("Name")?.ToLower().Contains(searchText.ToLower()) == true).ToList();

                        if (partialMatches.Any())
                        {
                            // Prioritize shorter matches (base games) over longer ones (soundtracks, DLC, etc.)
                            var sortedMatches = partialMatches.OrderBy(row => row.Field<string>("Name")?.Length ?? int.MaxValue).ToList();

                            // Check if the shortest match is actually an exact match or very close
                            var bestMatch = sortedMatches.First();
                            string bestMatchName = bestMatch.Field<string>("Name");

                            // If the shortest match equals our search text, use it
                            if (string.Equals(bestMatchName, searchText, StringComparison.OrdinalIgnoreCase))
                            {
                                ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name = '{0}'", bestMatchName.Replace("'", "''"));
                                goto SearchComplete;
                            }

                            // If we have multiple matches, prefer the one without extra words like "Soundtrack", "DLC", etc.
                            if (partialMatches.Count > 1)
                            {
                                var baseGameMatches = partialMatches.Where(row => {
                                    string name = row.Field<string>("Name")?.ToLower() ?? "";
                                    return !name.Contains("soundtrack") && !name.Contains("dlc") &&
                                           !name.Contains("season pass") && !name.Contains("expansion") &&
                                           !name.Contains("demo") && !name.Contains("beta");
                                }).OrderBy(row => row.Field<string>("Name")?.Length ?? int.MaxValue).ToList();

                                if (baseGameMatches.Any())
                                {
                                    string baseGameName = baseGameMatches.First().Field<string>("Name");
                                    ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name = '{0}'", baseGameName.Replace("'", "''"));
                                    goto SearchComplete;
                                }
                            }
                        }
                    }

                    // Check for matches with different Unicode normalization
                    var normalizedSearch = searchText.Normalize(NormalizationForm.FormKC);
                    var normalizedMatches = dt.AsEnumerable().Where(row =>
                        string.Equals(row.Field<string>("Name")?.Normalize(NormalizationForm.FormKC), normalizedSearch, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (normalizedMatches.Any())
                    {
                        string exactName = normalizedMatches.First().Field<string>("Name");
                        ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name = '{0}'", exactName.Replace("'", "''"));
                        goto SearchComplete;
                    }

                    // Check for matches ignoring certain problematic characters
                    string cleanSearch = new string(searchText.Where(c =>
                        !char.IsControl(c) &&
                        c != '\u00A0' && // Non-breaking space
                        c != '\u2009' && // Thin space
                        c != '\u200B' && // Zero-width space
                        c != '\uFEFF'    // Byte order mark
                    ).ToArray()).Trim();

                    if (cleanSearch != searchText)
                    {
                        var cleanMatches = dt.AsEnumerable().Where(row => {
                            string rowName = row.Field<string>("Name");
                            if (rowName == null) return false;
                            string cleanRowName = new string(rowName.Where(c =>
                                !char.IsControl(c) && c != '\u00A0' && c != '\u2009' && c != '\u200B' && c != '\uFEFF'
                            ).ToArray()).Trim();
                            return string.Equals(cleanRowName, cleanSearch, StringComparison.OrdinalIgnoreCase);
                        }).ToList();

                        if (cleanMatches.Any())
                        {
                            string exactName = cleanMatches.First().Field<string>("Name");
                            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name = '{0}'", exactName.Replace("'", "''"));
                            goto SearchComplete;
                        }
                    }
                }

                string Search = RemoveSpecialCharacters(searchTextBox.Text.ToLower()).Trim();
                string SplitSearch = SplitCamelCase(RemoveSpecialCharacters(searchTextBox.Text)).Trim();
                ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + Search.Replace("_", "").Trim() + "'");

                if (dataGridView1.Rows.Count == 0)
                    ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + SplitSearch.Replace("_", "").Trim() + "'");

                if (dataGridView1.Rows.Count == 0)
                    ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + SplitSearch.ToLower().Replace("_", "").Replace("vr", "").Replace("vrs", "").Trim() + "'");

                if (dataGridView1.Rows.Count == 0)
                    ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + SplitSearch.ToLower().Replace("'", "").Replace("-", "").Replace(";", "").Trim() + "'");

                if (dataGridView1.Rows.Count == 0)
                {

                    ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + Search.Replace("_", "").Replace(" ", "%' AND Name LIKE '%").Replace(" and ", " ").Replace(" the ", " ").Replace(":", "") + "%'");
                    if (dataGridView1.Rows.Count > 0)
                    {
                        Clipboard.SetText($"{dataGridView1.Rows[0].Cells[1].Value.ToString()}");
                    }
                    else
                    {

                        ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + SplitSearch.Replace("_", "").Replace(" ", "%' AND Name LIKE '%").Replace(" and ", " ").Replace(" the ", " ").Replace(":", "") + "%'").Trim();
                        if (dataGridView1.Rows.Count > 0)
                        {
                            // Don't auto-select here - let it fall through to SearchComplete
                        }
                        else
                        {



                            Search = searchTextBox.Text.ToLower();
                            if (Search.StartsWith("$"))
                            {
                                ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '" + Search.Replace("$", "").Replace("&", "and").Replace(":", " -") + "'");
                            }
                            else
                            {
                                Search = RemoveSpecialCharacters(searchTextBox.Text.ToLower());
                                ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + Search.Replace(" ", "%' AND Name LIKE '%").Replace(" and ", "").Replace(" & ", "").Replace(":", "") + "%'");
                            }

                            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                            dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                            if (BackPressed)
                            {
                                SearchPause = true;
                                t1.Start();
                            }

                            dataGridView1.ClearSelection();
                            dataGridView1.DefaultCellStyle.SelectionBackColor = dataGridView1.DefaultCellStyle.BackColor;
                            dataGridView1.DefaultCellStyle.SelectionForeColor = dataGridView1.DefaultCellStyle.ForeColor;

                            // ENHANCED FALLBACK: If still no results, try improved cleaning and offer manual input
                            if (dataGridView1.Rows.Count == 0 && searchTextBox.Text.Length > 2)
                            {
                                string improvedClean = ImprovedGameNameCleaning(searchTextBox.Text);

                                // Try with improved cleaning
                                ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + improvedClean.Replace(" ", "%' AND Name LIKE '%") + "%'");

                                // If still no match, try individual significant words
                                if (dataGridView1.Rows.Count == 0)
                                {
                                    string[] words = improvedClean.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (words.Length > 1)
                                    {
                                        var significantWords = words.Where(w => w.Length > 3).OrderByDescending(w => w.Length).Take(2);
                                        foreach (string word in significantWords)
                                        {
                                            ((DataTable)dataGridView1.DataSource).DefaultView.RowFilter = String.Format("Name like '%" + word + "%'");
                                            if (dataGridView1.Rows.Count > 0) break;
                                        }
                                    }
                                }

                                // If STILL no match, just show a status message - NO POPUP
                                if (dataGridView1.Rows.Count == 0)
                                {
                                    Tit($"No matches found for '{searchTextBox.Text}'. Try a different name or use Manual Entry button.", Color.Orange);
                                    btnManualEntry.Visible = true;  // Make sure manual entry button is visible
                                    resinstruccZip.Visible = true;
                                    resinstruccZipTimer.Stop();
                                    resinstruccZipTimer.Start();  // Start 30 second timer
                                    mainPanel.Visible = false;      // Hide main panel so dataGridView is visible
                                    // Don't show any popup - user can click Manual Entry if they want
                                }
                            }

                        }

                    }



                }

                // Always go to SearchComplete to handle the results
                goto SearchComplete;

                SearchComplete:
                // Only auto-select if this is the INITIAL folder search AND exactly 1 match
                if (dataGridView1.Rows.Count == 1)
                {
                    if (isInitialFolderSearch)
                    {
                        // This is THE initial folder search with 1 match - auto-select it
                        SetSelectedGame(0);
                    }
                    else
                    {
                        // User is manually searching - don't auto-select
                        Tit($"1 match: {dataGridView1[0, 0].Value} - Press Enter to select", Color.LightGreen);
                    }
                }
                else if (dataGridView1.Rows.Count > 1)
                {
                    Tit($"{dataGridView1.Rows.Count} matches found", Color.LightSkyBlue);
                    // More than 1 match = no perfect match, show manual entry option
                    btnManualEntry.Visible = true;
                    resinstruccZip.Visible = true;
                    resinstruccZipTimer.Stop();
                    resinstruccZipTimer.Start();  // Start 30 second timer
                }

                // Clear the initial folder search flag - any further typing is manual
                isInitialFolderSearch = false;

                // Fallback: If we still have no game selected but we do have search results, ensure buttons are visible
                if (string.IsNullOrEmpty(APPID) && dataGridView1.Rows.Count > 1)
                {
                    btnManualEntry.Visible = true;
                    resinstruccZip.Visible = true;
                    resinstruccZipTimer.Stop();
                    resinstruccZipTimer.Start();  // Start 30 second timer
                }

                textChanged = false;
            }
            catch { }
            // Remove duplicate auto-selection code that was causing issues
        }
        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (VRLExists)
            {
                string PropName = RemoveSpecialCharacters(dataGridView1[0, e.RowIndex].Value.ToString());
                File.WriteAllText($"{APPDATA}\\VRL\\ProperName.txt", PropName);
            }
            Tit("READY! Click skull folder above to perform crack!", Color.LightSkyBlue);
            APPID = dataGridView1[1, CurrentCell].Value.ToString();
            APPNAME = dataGridView1[0, CurrentCell].Value.ToString().Trim();
            Tat($"{APPNAME} ({APPID})");
            searchTextBox.Clear();
            mainPanel.Visible = true;
            btnManualEntry.Visible = false;
            startCrackPic.Visible = true;
            resinstruccZip.Visible = false;
            Tit("READY! Click skull folder above to perform crack!", Color.LightSkyBlue);

            // Auto-crack if enabled
            if (autoCrackEnabled && !string.IsNullOrEmpty(gameDir))
            {
                // Trigger crack just like clicking the button
                startCrackPic_Click(null, null);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/harryeffinpotter/SteamAPPIDFinder");
        }


        private void searchTextBox_Enter(object sender, EventArgs e)
        {

        }

        public async Task<bool> CrackAsync()  // Return true if something was cracked
        {
            // Run the cracking process on a background thread to prevent UI freeze
            return await Task.Run(() => CrackCoreAsync());
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            // Ensure paths end with separator for proper replacement
            if (!sourceDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                sourceDir += Path.DirectorySeparatorChar;

            if (!targetDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                targetDir += Path.DirectorySeparatorChar;

            // Create target directory if it doesn't exist
            Directory.CreateDirectory(targetDir);

            // Create all directories
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = dirPath.Substring(sourceDir.Length);
                Directory.CreateDirectory(Path.Combine(targetDir, relativePath));
            }

            // Copy all files
            foreach (string filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                string relativePath = filePath.Substring(sourceDir.Length);
                string targetPath = Path.Combine(targetDir, relativePath);
                File.Copy(filePath, targetPath, true);
            }
        }

        private async Task<bool> HandlePermissionError()
        {
            Tit("Showing permission error dialog...", Color.Yellow);

            DialogResult result = MessageBox.Show(
                "Permission error: Unable to modify files in the selected game folder.\n\n" +
                "Would you like to copy the game folder to your desktop and perform the action there?",
                "Permission Error",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string gameFolderName = Path.GetFileName(gameDir);
                string newGameDir = Path.Combine(desktopPath, gameFolderName);

                try
                {
                    Tit($"Copying game from {gameDir} to Desktop...", Color.Yellow);

                    // Copy all files recursively
                    await Task.Run(() => CopyDirectory(gameDir, newGameDir));

                    // Update gameDir to the new location
                    gameDir = newGameDir;
                    Tat(gameDir);  // Update the displayed directory

                    Tit($"Game copied to Desktop! Retrying crack with new location...", Color.Lime);

                    // Return true to retry the crack - we're already IN a crack attempt when this error happened
                    // The user already clicked crack, so continue with the same APPID
                    return true; // Tell CrackCoreAsync to retry with the new location
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to copy game: {ex.Message}", "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return false;
        }

        private async Task<bool> CrackCoreAsync()  // Core cracking logic moved to separate method
        {
            int execount = -20;
            int steam64count = -1;
            int steamcount = -1;
            string parentdir = "";

            bool cracked = false;
            bool steamlessUnpacked = false;  // Track if Steamless unpacked anything
            string originalGameDir = gameDir;  // Keep track of original location

            System.Diagnostics.Debug.WriteLine($"[CRACK] === Starting CrackCoreAsync ===");
            System.Diagnostics.Debug.WriteLine($"[CRACK] Game Directory: {gameDir}");
            System.Diagnostics.Debug.WriteLine($"[CRACK] AppID: {APPID}");
            System.Diagnostics.Debug.WriteLine($"[CRACK] Current Directory: {Environment.CurrentDirectory}");

            try
            {
                var files = Directory.GetFiles(gameDir, "*.*", SearchOption.AllDirectories);
                System.Diagnostics.Debug.WriteLine($"[CRACK] Found {files.Length} files total");


                foreach (string file in files)
                {
                    if (file.EndsWith("steam_api64.dll"))
                    {
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Found steam_api64.dll: {file}");
                        steam64count++;
                        parentdir = Directory.GetParent(file).FullName;
                        string steam = $"{parentdir}\\steam_settings";
                        if (Directory.Exists(steam))
                        {
                            var filesz = Directory.GetFiles(steam, "*.*", SearchOption.AllDirectories);
                            foreach (var filee in filesz)
                            {
                                File.Delete(filee);
                            }
                            Directory.Delete(steam, true);
                        }
                        if (File.Exists($"{file}.bak"))
                        {
                            File.Delete(file);
                            File.Move($"{file}.bak", file);
                        }
                        Tit("Replacing steam_api64.dll.", Color.LightSkyBlue);

                        try
                        {
                            File.Move(file, $"{file}.bak");
                            if (goldy)
                            {
                                Directory.CreateDirectory(steam);
                                File.Copy($"{Environment.CurrentDirectory}\\_bin\\Goldberg\\steam_api64.dll", file);
                            }
                            else
                            {
                                File.Copy($"{Environment.CurrentDirectory}\\_bin\\ALI213\\steam_api64.dll", file);
                            }
                            cracked = true;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            // Handle permission error
                            Tit($"Permission error detected: {ex.Message}", Color.Orange);
                            if (await HandlePermissionError())
                            {
                                return await CrackCoreAsync(); // Retry with new location
                            }
                            return false;
                        }

                        if (!goldy)
                        {
                            if (File.Exists(parentdir + "\\SteamConfig.ini"))
                            {
                                File.Delete(parentdir + "\\SteamConfig.ini");
                            }
                            IniFileEdit($"{Environment.CurrentDirectory}\\_bin\\ALI213\\SteamConfig.ini [Settings] \"AppID = {APPID}\"");
                            File.Copy($"{Environment.CurrentDirectory}\\_bin\\ALI213\\SteamConfig.ini", $"{parentdir}\\SteamConfig.ini");
                        }
                    }
                    if (file.EndsWith("steam_api.dll"))
                    {
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Found steam_api.dll: {file}");
                        steamcount++;
                        parentdir = Directory.GetParent(file).FullName;
                        string steam = $"{parentdir}\\steam_settings";
                        if (Directory.Exists(steam))
                        {
                            var filesz = Directory.GetFiles(steam, "*.*", SearchOption.AllDirectories);
                            foreach (var filee in filesz)
                            {
                                File.Delete(filee);
                            }
                            Directory.Delete(steam, true);
                        }
                        if (File.Exists($"{file}.bak"))
                        {
                            File.Delete(file);
                            File.Move($"{file}.bak", file);
                        }
                        Tit("Replacing steam_api.dll.", Color.LightSkyBlue);
                        parentdir = Directory.GetParent(file).FullName;

                        try
                        {
                            File.Move(file, $"{file}.bak");
                            if (goldy)
                            {
                                Directory.CreateDirectory(steam);
                                File.Copy($"{Environment.CurrentDirectory}\\_bin\\Goldberg\\steam_api.dll", file);
                            }
                            else
                            {
                                File.Copy($"{Environment.CurrentDirectory}\\_bin\\ALI213\\steam_api.dll", file);
                            }
                            cracked = true;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            // Handle permission error
                            Tit($"Permission error detected: {ex.Message}", Color.Orange);
                            if (await HandlePermissionError())
                            {
                                return await CrackCoreAsync(); // Retry with new location
                            }
                            return false;
                        }

                        if (!goldy)
                        {
                            try
                            {
                                if (File.Exists(parentdir + "\\SteamConfig.ini"))
                                {
                                    File.Delete(parentdir + "\\SteamConfig.ini");
                                }
                                IniFileEdit($"\".\\_bin\\ALI213\\SteamConfig.ini\" [Settings] \"AppID = {APPID}\"");
                                File.Copy("_bin\\ALI213\\SteamConfig.ini", $"{parentdir}\\SteamConfig.ini");
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                Tit($"Permission error on SteamConfig.ini: {ex.Message}", Color.Orange);
                                if (await HandlePermissionError())
                                {
                                    return await CrackCoreAsync();
                                }
                                return false;
                            }
                        }
                    }

                    if (Path.GetExtension(file) == ".exe")
                    {
                        // Check if Steamless exists before trying to use it
                        string steamlessPath = $"{Environment.CurrentDirectory}\\_bin\\Steamless\\Steamless.CLI.exe";
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Checking for Steamless at: {steamlessPath}");
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Steamless exists: {File.Exists(steamlessPath)}");

                        if (!File.Exists(steamlessPath))
                        {
                            System.Diagnostics.Debug.WriteLine($"[CRACK] Steamless not found, skipping EXE unpacking for: {file}");
                            Tit($"Steamless not found at {steamlessPath}, skipping EXE unpacking", Color.Yellow);
                            continue;
                        }

                        System.Diagnostics.Debug.WriteLine($"[CRACK] Processing EXE: {file}");

                        if (File.Exists($"{file}.bak"))
                        {
                            File.Delete(file);
                            File.Move($"{file}.bak", file);
                        }

                        string exeparent = Directory.GetParent(file).FullName;
                        Process x2 = new Process();
                        ProcessStartInfo pro = new ProcessStartInfo();
                        pro.WindowStyle = ProcessWindowStyle.Hidden;
                        pro.UseShellExecute = false;
                        pro.CreateNoWindow = true;
                        pro.RedirectStandardError = true;
                        pro.RedirectStandardOutput = true;
                        pro.WorkingDirectory = parentdir;
                        pro.FileName = @"C:\Windows\System32\cmd.exe";
                        pro.Arguments = $"/c start /B \"\" \"{steamlessPath}\" \"{file}\"";

                        System.Diagnostics.Debug.WriteLine($"[CRACK] Starting Steamless process");
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Working Directory: {parentdir}");
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Command: {pro.Arguments}");

                        x2.StartInfo = pro;
                        x2.Start();
                        string Output = x2.StandardError.ReadToEnd() + x2.StandardOutput.ReadToEnd();
                        x2.WaitForExit();

                        System.Diagnostics.Debug.WriteLine($"[CRACK] Steamless output: {Output}");
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Steamless exit code: {x2.ExitCode}");

                        if (File.Exists($"{file}.unpacked.exe"))
                        {
                            System.Diagnostics.Debug.WriteLine($"[CRACK] Successfully unpacked: {file}");
                            Tit($"Unpacked {file} successfully!", Color.LightSkyBlue);
                            File.Move(file, file + ".bak");
                            File.Move($"{file}.unpacked.exe", file);
                            steamlessUnpacked = true;  // Mark that we unpacked something
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[CRACK] No unpacked file created for: {file}");
                        }

                    }
                    if (steamcount > 1 || execount > 1 || steam64count > 1)
                    {
                        DialogResult Diagg = MessageBox.Show(
                            "This is the 2nd steam_api64.dll on this run - something is broken. " +
                            "The APPID has to match the ini/txt files or the cracks will not work.\n\n" +
                            "This usually happens when SACGUI determines the wrong parent dir " +
                            "from EXE or when user selects incorrect folder. Please select " +
                            "the GAME DIRECTORY, for example:\n\n" +
                            "-CORRECT-\nD:\\SteamLibrary\\Common\\SomeGameName\n\n" +
                            "-INCORRECT-\nD:\\SteamLibrary\\Common\n\n" +
                            "If you think this message is wrong, verify the path on bottom left and hit YES to continue..",
                            "Somethings wrong..., continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (Diagg == DialogResult.Yes)
                        {
                            execount = -600;
                            steam64count = -600;
                            steamcount = -600;
                        }
                        else
                        {
                            break;
                        }
                    }

                }


                if (cracked)
                {
                    if (goldy)
                    {
                        if (Directory.Exists($"{parentdir}\\steam_settings"))
                        {
                            Directory.Delete($"{parentdir}\\steam_settings", true);
                        }
                        Directory.CreateDirectory($"{parentdir}\\steam_settings");

                        // Create steam_appid.txt with just the APPID - this is what gbe_fork expects
                        File.WriteAllText($"{parentdir}\\steam_settings\\steam_appid.txt", APPID);

                        // Check if user wants LAN multiplayer support (from checkbox)
                        if (enableLanMultiplayer)
                        {
                            // Create custom_broadcasts.txt for multiplayer discovery
                            // Just a simple list of IPs, one per line
                            string broadcastContent =
                                "255.255.255.255\n" +  // Broadcast to local network
                                "127.0.0.1\n";         // Localhost

                            File.WriteAllText($"{parentdir}\\steam_settings\\custom_broadcasts.txt", broadcastContent);

                            // Copy lobby_connect tool to game directory for easy access
                            string lobbyConnectSource = "";
                            // Check in _bin folder first (where updater puts them)
                            if (File.Exists($"{Environment.CurrentDirectory}\\_bin\\lobby_connect_x64.exe"))
                                lobbyConnectSource = $"{Environment.CurrentDirectory}\\_bin\\lobby_connect_x64.exe";
                            else if (File.Exists($"{Environment.CurrentDirectory}\\_bin\\lobby_connect_x32.exe"))
                                lobbyConnectSource = $"{Environment.CurrentDirectory}\\_bin\\lobby_connect_x32.exe";
                            // Fallback to _bin\\Goldberg for backward compatibility
                            else if (File.Exists($"{Environment.CurrentDirectory}\\_bin\\Goldberg\\lobby_connect_x64.exe"))
                                lobbyConnectSource = $"{Environment.CurrentDirectory}\\_bin\\Goldberg\\lobby_connect_x64.exe";
                            else if (File.Exists($"{Environment.CurrentDirectory}\\_bin\\Goldberg\\lobby_connect_x32.exe"))
                                lobbyConnectSource = $"{Environment.CurrentDirectory}\\_bin\\Goldberg\\lobby_connect_x32.exe";

                            if (!string.IsNullOrEmpty(lobbyConnectSource))
                            {
                                // Copy lobby_connect tool to GAME directory (not crack directory)
                                string lobbyConnectFileName = Path.GetFileName(lobbyConnectSource); // e.g. lobby_connect_x64.exe
                                string lobbyConnectDest = Path.Combine(gameDir, $"_{lobbyConnectFileName}"); // e.g. _lobby_connect_x64.exe

                                Tit($"Copying lobby_connect to game folder: {gameDir}", Color.Yellow);
                                File.Copy(lobbyConnectSource, lobbyConnectDest, true);

                                // Find the main game executable IN THE GAME DIRECTORY
                                string gameExe = "";
                                var exeFiles = Directory.GetFiles(gameDir, "*.exe", SearchOption.TopDirectoryOnly)
                                    .Where(f => !f.Contains("lobby_connect") &&
                                               !f.Contains("UnityCrashHandler") &&
                                               !f.Contains("unins") &&
                                               !f.Contains("setup"))
                                    .ToList();

                                if (exeFiles.Count == 1)
                                {
                                    gameExe = exeFiles[0];
                                }
                                else if (exeFiles.Count > 1)
                                {
                                    // Try to find the most likely game exe
                                    gameExe = exeFiles.FirstOrDefault(f =>
                                        Path.GetFileNameWithoutExtension(f).ToLower().Contains(gameDirName.ToLower()))
                                        ?? exeFiles[0];
                                }

                                // Create a config file for lobby_connect to remember the game exe
                                if (!string.IsNullOrEmpty(gameExe))
                                {
                                    Tit($"Found game exe: {Path.GetFileName(gameExe)}", Color.Yellow);

                                    // Save the game exe path for lobby_connect
                                    string lobbyConfigFile = Path.GetFileNameWithoutExtension(lobbyConnectSource) + ".txt";
                                    File.WriteAllText(Path.Combine(gameDir, lobbyConfigFile), gameExe);

                                    // Create batch files in steam_settings folder (tucked away)
                                    string steamSettingsPath = Path.Combine(parentdir, "steam_settings");

                                    // Join batch - automatically connects and launches the game
                                    string lobbyExeName = "_" + Path.GetFileName(lobbyConnectSource); // e.g. _lobby_connect_x64.exe
                                    string joinBatchContent = $@"@echo off
cd /d ""{gameDir}""
echo Searching for LAN games and connecting...
{lobbyExeName} --connect ""{Path.GetFileName(gameExe)}""
if errorlevel 1 (
    echo Could not find any LAN games. Starting as host instead...
    start """" ""{Path.GetFileName(gameExe)}""
)
exit";

                                    // Host batch - just launches the game normally
                                    string hostBatchContent = $@"@echo off
cd /d ""{gameDir}""
start """" ""{Path.GetFileName(gameExe)}""
exit";

                                    // Edit broadcasts batch - opens custom_broadcasts.txt in notepad
                                    string editBroadcastsContent = $@"@echo off
echo Opening custom_broadcasts.txt for editing...
echo Add your friends' public IPs to play over the internet!
echo.
echo This window will automatically close when you close notepad.
notepad ""{Path.Combine(steamSettingsPath, "custom_broadcasts.txt")}""
exit";

                                    // Create batch files in steam_settings folder
                                    string joinBatchPath = Path.Combine(steamSettingsPath, "_JoinLAN.bat");
                                    string hostBatchPath = Path.Combine(steamSettingsPath, "_HostLAN.bat");
                                    string editBroadcastsPath = Path.Combine(steamSettingsPath, "_EditMultiplayerIPs.bat");

                                    Tit($"Creating batch files in steam_settings folder", Color.Cyan);

                                    File.WriteAllText(joinBatchPath, joinBatchContent);
                                    File.WriteAllText(hostBatchPath, hostBatchContent);
                                    File.WriteAllText(editBroadcastsPath, editBroadcastsContent);

                                    // Verify batch files were created
                                    if (!File.Exists(joinBatchPath) || !File.Exists(hostBatchPath))
                                    {
                                        Tit($"ERROR: Failed to create batch files!", Color.Red);
                                    }
                                    else
                                    {
                                        Tit($"Batch files created successfully", Color.Lime);
                                    }

                                    // Create Windows shortcuts in game folder pointing to batch files in steam_settings
                                    try
                                    {
                                        string gameName = Path.GetFileNameWithoutExtension(gameExe);

                                        // Shortcuts in game folder
                                        string joinShortcutPath = Path.Combine(gameDir, $"_[Join LAN] {gameName}.lnk");
                                        string hostShortcutPath = Path.Combine(gameDir, $"_[Host LAN] {gameName}.lnk");
                                        string editIPsShortcutPath = Path.Combine(gameDir, $"_[Edit Multiplayer IPs].lnk");

                                        Tit($"Creating shortcuts in game folder for {gameName}", Color.Yellow);

                                        // Use VBScript to create shortcuts pointing to batch files in steam_settings
                                        string vbsScript = $@"
Set oWS = WScript.CreateObject(""WScript.Shell"")

' Join LAN shortcut
Set oLink = oWS.CreateShortcut(""{joinShortcutPath.Replace("\\", "\\\\").Replace("\"", "\"\"")}"")
oLink.TargetPath = ""{joinBatchPath.Replace("\\", "\\\\").Replace("\"", "\"\"")}""
oLink.WorkingDirectory = ""{gameDir.Replace("\\", "\\\\").Replace("\"", "\"\"")}""
oLink.Description = ""Search and join LAN/Internet games for {gameName.Replace("\"", "\"\"")}""
oLink.IconLocation = ""{gameExe.Replace("\\", "\\\\").Replace("\"", "\\\\")},0""
oLink.Save

' Host LAN shortcut
Set oLink2 = oWS.CreateShortcut(""{hostShortcutPath.Replace("\\", "\\\\").Replace("\"", "\"\"")}"")
oLink2.TargetPath = ""{hostBatchPath.Replace("\\", "\\\\").Replace("\"", "\"\"")}""
oLink2.WorkingDirectory = ""{gameDir.Replace("\\", "\\\\").Replace("\"", "\"\"")}""
oLink2.Description = ""Host a LAN/Internet game for {gameName.Replace("\"", "\"\"")}""
oLink2.IconLocation = ""{gameExe.Replace("\\", "\\\\").Replace("\"", "\\\\")},0""
oLink2.Save

' Edit Multiplayer IPs shortcut
Set oLink3 = oWS.CreateShortcut(""{editIPsShortcutPath.Replace("\\", "\\\\").Replace("\"", "\"\"")}"")
oLink3.TargetPath = ""{editBroadcastsPath.Replace("\\", "\\\\").Replace("\"", "\"\"")}""
oLink3.WorkingDirectory = ""{steamSettingsPath.Replace("\\", "\\\\").Replace("\"", "\"\"")}""
oLink3.Description = ""Edit custom_broadcasts.txt to add friend IPs for internet play""
oLink3.IconLocation = ""notepad.exe,0""
oLink3.Save";

                                        string tempVbs = Path.GetTempFileName() + ".vbs";
                                        File.WriteAllText(tempVbs, vbsScript);
                                        Process vbsProcess = Process.Start("wscript.exe", $"\"{tempVbs}\"");
                                        vbsProcess.WaitForExit();

                                        try { File.Delete(tempVbs); } catch { }

                                        // Check if at least the main shortcuts were created
                                        if (File.Exists(joinShortcutPath) || File.Exists(hostShortcutPath))
                                        {
                                            Tit("Multiplayer shortcuts created in game folder!", Color.MediumSpringGreen);
                                            Tit("Batch files stored in steam_settings folder", Color.Lime);
                                        }
                                        else
                                        {
                                            Tit("Shortcuts failed, but batch files are ready in steam_settings!", Color.Yellow);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Tit($"Shortcut error: {ex.Message}", Color.Yellow);
                                        Tit("Batch files ready in steam_settings folder!", Color.MediumSpringGreen);
                                    }
                                }
                            }
                        }

                        // Generate interfaces file if the tool exists (required for gbe_fork)
                        // Try different possible locations and names for the generate_interfaces tool
                        string generateInterfacesPath = "";
                        string[] possiblePaths = new string[] {
                            $"{Environment.CurrentDirectory}\\_bin\\Goldberg\\generate_interfaces_x64.exe",
                            $"{Environment.CurrentDirectory}\\_bin\\Goldberg\\generate_interfaces_x32.exe",
                            $"{Environment.CurrentDirectory}\\_bin\\generate_interfaces_x64.exe",
                            $"{Environment.CurrentDirectory}\\_bin\\generate_interfaces_x32.exe",
                            $"{Environment.CurrentDirectory}\\_bin\\Goldberg\\generate_interfaces_file.exe",
                            $"{Environment.CurrentDirectory}\\_bin\\generate_interfaces.exe"
                        };

                        foreach (string path in possiblePaths)
                        {
                            if (File.Exists(path))
                            {
                                generateInterfacesPath = path;
                                break;
                            }
                        }
                        if (File.Exists(generateInterfacesPath))
                        {
                            Tit("Generating steam interfaces...", Color.Cyan);
                            try
                            {
                                // Run generate_interfaces to create steam_interfaces.txt
                                Process genInterfaces = new Process();
                                genInterfaces.StartInfo.CreateNoWindow = true;
                                genInterfaces.StartInfo.UseShellExecute = false;
                                genInterfaces.StartInfo.FileName = generateInterfacesPath;
                                genInterfaces.StartInfo.WorkingDirectory = parentdir;
                                // Try to find the original steam dll backup to generate interfaces from
                                string originalDll = "";
                                if (File.Exists($"{parentdir}\\steam_api64.dll.bak"))
                                    originalDll = $"{parentdir}\\steam_api64.dll.bak";
                                else if (File.Exists($"{parentdir}\\steam_api.dll.bak"))
                                    originalDll = $"{parentdir}\\steam_api.dll.bak";

                                if (!string.IsNullOrEmpty(originalDll))
                                {
                                    genInterfaces.StartInfo.Arguments = $"\"{originalDll}\"";
                                }
                                else
                                {
                                    // If no backup found, try with the replaced dll (might not work as well)
                                    if (File.Exists($"{parentdir}\\steam_api64.dll"))
                                        genInterfaces.StartInfo.Arguments = $"\"{parentdir}\\steam_api64.dll\"";
                                    else if (File.Exists($"{parentdir}\\steam_api.dll"))
                                        genInterfaces.StartInfo.Arguments = $"\"{parentdir}\\steam_api.dll\"";
                                }
                                genInterfaces.Start();
                                genInterfaces.WaitForExit();

                                // Move generated file to steam_settings if it was created
                                if (File.Exists($"{parentdir}\\steam_interfaces.txt"))
                                {
                                    File.Move($"{parentdir}\\steam_interfaces.txt", $"{parentdir}\\steam_settings\\steam_interfaces.txt");
                                }
                            }
                            catch { }
                        }

                        // Try achievements_parser first (works better with modern games)
                        string achievementsParserPath = $"{Environment.CurrentDirectory}\\_bin\\achievements_parser.exe";
                        if (File.Exists(achievementsParserPath))
                        {
                            Tit("Getting achievements data...", Color.Cyan);
                            try
                            {
                                var achievementsResult = await Cli.Wrap(achievementsParserPath)
                                    .WithValidation(CommandResultValidation.None)
                                    .WithArguments($"--appid {APPID}")
                                    .WithWorkingDirectory(Environment.CurrentDirectory)
                                    .ExecuteBufferedAsync();

                                if (!string.IsNullOrWhiteSpace(achievementsResult.StandardOutput))
                                {
                                    // Save achievements to the steam_settings folder
                                    string achievementsPath = $"{parentdir}\\steam_settings\\achievements.ini";
                                    File.WriteAllText(achievementsPath, achievementsResult.StandardOutput);
                                    Tit($"Found and saved achievement data!", Color.Green);
                                }
                            }
                            catch { }
                        }

                        // Try old method for DLC if generate_game_infos exists (for compatibility)
                        string generateGameInfosPath = $"{Environment.CurrentDirectory}\\_bin\\generate_game_infos.exe";
                        if (File.Exists(generateGameInfosPath))
                        {
                            Tit("Generating DLC info...", Color.Cyan);
                            try
                            {
                                var infos = Cli.Wrap(generateGameInfosPath)
                                    .WithValidation(CommandResultValidation.None)
                                    .WithArguments($"{APPID} -s 92CD46192F62DE1A769F79A667CE5631 -o\"{parentdir}\\steam_settings\"")
                                    .WithWorkingDirectory(parentdir)
                                    .ExecuteAsync()
                                    .GetAwaiter()
                                    .GetResult();
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CRACK] UnauthorizedAccessException: {ex.Message}");
                File.WriteAllText("_CRASHlog.txt", $"[UnauthorizedAccessException]\n{ex.StackTrace}\n{ex.Message}");
                Tit($"Permission denied. Try running as administrator or copy game to desktop.", Color.Red);
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CRACK] Exception: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CRACK] Stack trace: {ex.StackTrace}");
                File.WriteAllText("_CRASHlog.txt", $"[{ex.GetType().Name}]\n{ex.StackTrace}\n{ex.Message}\n\nFull Exception:\n{ex.ToString()}");
                throw; // Re-throw to let caller handle it
            }

            System.Diagnostics.Debug.WriteLine($"[CRACK] === Crack Complete ===");
            System.Diagnostics.Debug.WriteLine($"[CRACK] Cracked: {cracked}, Steamless Unpacked: {steamlessUnpacked}");
            // Return true only if we actually cracked something
            return cracked || steamlessUnpacked;
        }

        public void IniFileEdit(string args)
        {
            Process IniProcess = new Process();
            IniProcess.StartInfo.CreateNoWindow = true;
            IniProcess.StartInfo.UseShellExecute = false;
            IniProcess.StartInfo.FileName = $"{Environment.CurrentDirectory}\\_bin\\ALI213\\inifile.exe";

            IniProcess.StartInfo.Arguments = args;
            IniProcess.Start();
            IniProcess.WaitForExit();
        }

        
        private string parentOfSelection;
        private string gameDir;
        private string gameDirName;

        // Public property to allow EnhancedShareWindow to set game directory
        public string GameDirectory
        {
            get { return gameDir; }
            set { gameDir = value; }
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            OpenDir.Visible = false;
            OpenDir.SendToBack();
            FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
            folderSelectDialog.Title = "Select the game's main folder.";

            if (Properties.Settings.Default.lastDir.Length > 0)
                folderSelectDialog.InitialDirectory = Properties.Settings.Default.lastDir;

            if (folderSelectDialog.Show(Handle))
            {
                gameDir = folderSelectDialog.FileName;

                // Hide OpenDir and ZipToShare when new directory selected
                OpenDir.Visible = false;
                ZipToShare.Visible = false;
                parentOfSelection = Directory.GetParent(gameDir).FullName;
                gameDirName = Path.GetFileName(gameDir);
                btnManualEntry.Visible = true;
                resinstruccZip.Visible = true;
                resinstruccZipTimer.Stop();
                resinstruccZipTimer.Start();  // Start 30 second timer
                mainPanel.Visible = false;  // Hide mainPanel so dataGridView is visible

                startCrackPic.Visible = true;
                Tit("Please select the correct game from the list!! (if list empty do manual search!)", Color.LightSkyBlue);

                // Stop label5 timer when game dir is selected
                label5Timer.Stop();
                label5.Visible = false;

                isFirstClickAfterSelection = true;  // Set before changing text
                isInitialFolderSearch = true;  // This is the initial search from folder
                searchTextBox.Text = gameDirName;
                Properties.Settings.Default.lastDir = parentOfSelection;
                Properties.Settings.Default.Save();
            }
            else
            { 
                MessageBox.Show("You must select a folder to continue..."); 
            }
        }
        public bool cracking;
        private async void startCrackPic_Click(object sender, EventArgs e)
        {
           if (!cracking)
            {
                cracking = true;

                // Hide OpenDir and ZipToShare during cracking
                OpenDir.Visible = false;
                ZipToShare.Visible = false;

                bool crackedSuccessfully = await CrackAsync();
                cracking = false;

                // Check if we actually cracked anything
                if (crackedSuccessfully)
                {
                    // Set the permanent success message
                    Tit("Crack complete!\nSelect another game directory to keep the party going!", Color.LightSkyBlue);

                    // Show both buttons after successful crack
                    OpenDir.Visible = true;
                    ZipToShare.Visible = true;
                    ZipToShare.Text = "Zip Dir";  // Ensure text is set
                    ZipToShare.BringToFront();
                }
                else
                {
                    Tit("No files to crack found!", Color.Red);
                    await Task.Delay(3000);
                    Tit("Click folder & select game's parent directory.", Color.Cyan);

                    OpenDir.BringToFront();
                    OpenDir.Visible = true;
                    ZipToShare.Visible = false;
                }

                startCrackPic.Visible = false;
                donePic.Visible = true;
                donePic.Visible = false;
            }
        }
        public bool goldy = false;
        public bool enableLanMultiplayer = false;
        public bool autoCrackEnabled = true;  // Default to ON

        private void lanMultiplayerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            enableLanMultiplayer = lanMultiplayerCheckBox.Checked;
        }
        private void dllSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dllSelect.SelectedIndex == 0)
            {
                goldy = true;
                Properties.Settings.Default.Goldy = true;
                // Show LAN checkbox for Goldberg
                lanMultiplayerCheckBox.Visible = true;
            }
            else if (dllSelect.SelectedIndex == 1)
            {
                goldy = false;
                Properties.Settings.Default.Goldy = false;
                // Hide LAN checkbox for Ali213 (not supported)
                lanMultiplayerCheckBox.Visible = false;
                lanMultiplayerCheckBox.Checked = false;
                enableLanMultiplayer = false;
            }
            Properties.Settings.Default.Save();
        }
        private void selectDir_MouseEnter(object sender, EventArgs e)
        {
            selectDir.Image = Image.FromFile("_bin\\hoveradd.png");
        }
        private void selectDir_MouseHover(object sender, EventArgs e)
        {
            selectDir.Image = Image.FromFile("_bin\\hoveradd.png");
        }

        private void selectDir_MouseDown(object sender, MouseEventArgs e)
        {
            selectDir.Image = Image.FromFile("_bin\\clickadd.png");
        }

        private void startCrackPic_MouseDown(object sender, MouseEventArgs e)
        {
            startCrackPic.Image = Image.FromFile("_bin\\clickr2c.png");
        }

        private void startCrackPic_MouseEnter(object sender, EventArgs e)
        {
            startCrackPic.Image = Image.FromFile("_bin\\hoverr2c.png");
        }

        private void startCrackPic_MouseHover(object sender, EventArgs e)
        {
            startCrackPic.Image = Image.FromFile("_bin\\hoverr2c.png");
        }

        private void selectDir_MouseLeave(object sender, EventArgs e)
        {
            selectDir.Image = Image.FromFile("_bin\\add.png");
        }

        private void startCrackPic_MouseLeave(object sender, EventArgs e)
        {
            startCrackPic.Image = Image.FromFile("_bin\\r2c.png");
        }

        private void mainPanel_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            drgdropText.BringToFront();
            drgdropText.Visible = true;
            e.Effect = DragDropEffects.Copy;
        }

        private void mainPanel_DragLeave(object sender, EventArgs e)
        {
            drgdropText.SendToBack();
            drgdropText.Visible = false;
        }

        private void mainPanel_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {

            string[] drops = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string d in drops)
            {
                FileAttributes attr = File.GetAttributes(d);
                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    //DIR
                    gameDir = d;

                    // Hide OpenDir and ZipToShare when new directory selected
                    OpenDir.Visible = false;
                    ZipToShare.Visible = false;
                    parentOfSelection = Directory.GetParent(gameDir).FullName;
                    gameDirName = Path.GetFileName(gameDir);
                    mainPanel.Visible = false;  // Hide mainPanel so dataGridView is visible
                    btnManualEntry.Visible = true;
                    resinstruccZip.Visible = true;  // Show this too!
                    resinstruccZipTimer.Stop();
                    resinstruccZipTimer.Start();  // Start 30 second timer
                    startCrackPic.Visible = true;
                    Tit("Please select the correct game from the list!! (if list empty do manual search!)", Color.LightSkyBlue);

                    // Stop label5 timer when game dir is selected
                    label5Timer.Stop();
                    label5.Visible = false;

                    isInitialFolderSearch = true;  // This is the initial search
                    searchTextBox.Text = gameDirName;
                    isFirstClickAfterSelection = true;  // Set AFTER changing text to avoid race condition
                    Properties.Settings.Default.lastDir = parentOfSelection;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    //FILE - reject all files
                    Tit("Please drag and drop a FOLDER, not a file!", Color.LightSkyBlue);
                    MessageBox.Show("Drag and drop a game folder, not individual files!");
                }
            }
            drgdropText.SendToBack();
            drgdropText.Visible = false;
        }

        private void unPin_Click(object sender, EventArgs e)
        {
            this.TopMost = false;
            pin.BringToFront();
            Properties.Settings.Default.Pinned = false;
            Properties.Settings.Default.Save();
        }

        private void pin_Click(object sender, EventArgs e)
        {
            this.TopMost = true;
            unPin.BringToFront();
            Properties.Settings.Default.Pinned = true;
            Properties.Settings.Default.Save();
        }

        private void autoCrackOff_Click(object sender, EventArgs e)
        {
            autoCrackEnabled = true;
            autoCrackOn.BringToFront();
            Properties.Settings.Default.AutoCrack = true;
            Properties.Settings.Default.Save();
        }

        private void autoCrackOn_Click(object sender, EventArgs e)
        {
            autoCrackEnabled = false;
            autoCrackOff.BringToFront();
            Properties.Settings.Default.AutoCrack = false;
            Properties.Settings.Default.Save();
        }

        private void SteamAppId_FormClosing(object sender, FormClosingEventArgs e)
        {
            SteamAppIdIdentifier.Program.CmdKILL("APPID");
        }

        private void OpenDir_Click(object sender, EventArgs e)
        {
            Process.Start(gameDir);
        }

        // Override KeyProcessCmdKey to catch Ctrl+S
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                // Force show compression settings dialog
                Properties.Settings.Default.ZipDontAsk = false;
                Properties.Settings.Default.Save();

                if (ZipToShare.Visible && ZipToShare.Enabled)
                {
                    ZipToShare_Click(null, null);
                }
                else
                {
                    // Show compression dialog even without a completed crack for testing
                    using (var compressionForm = new CompressionSettingsForm())
                    {
                        compressionForm.Owner = this;
                        compressionForm.StartPosition = FormStartPosition.CenterParent;
                        compressionForm.TopMost = true;
                        compressionForm.BringToFront();
                        compressionForm.ShowDialog(this);
                    }
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async Task ShareGameAsync(string gameName, string gamePath, bool crack, Form parentForm)
        {
            try
            {
                // Store original gameDir and restore after
                var originalGameDir = gameDir;
                gameDir = gamePath;
                gameDirName = gameName;

                if (crack)
                {
                    // Crack the game first
                    if (parentForm.IsHandleCreated)
                    {
                        parentForm.Invoke(new Action(() => {
                            Tit($"Cracking {gameName}...", Color.Yellow);
                        }));
                    }

                    bool crackSuccess = await CrackAsync();
                    if (!crackSuccess)
                    {
                        MessageBox.Show($"Failed to crack {gameName}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        gameDir = originalGameDir;
                        return;
                    }
                }

                // Now show compression form with upload options
                using (var compressionForm = new CompressionSettingsFormExtended(gameName, crack))
                {
                    compressionForm.Owner = parentForm;
                    compressionForm.StartPosition = FormStartPosition.CenterParent;
                    compressionForm.TopMost = true;

                    if (compressionForm.ShowDialog() != DialogResult.OK)
                    {
                        gameDir = originalGameDir;
                        return;
                    }

                    // Handle compression and upload
                    await CompressAndUploadAsync(
                        gamePath,
                        gameName,
                        crack,
                        compressionForm.SelectedFormat,
                        compressionForm.SelectedLevel,
                        compressionForm.UploadToBackend,
                        compressionForm.EncryptForRIN,
                        parentForm
                    );
                }

                gameDir = originalGameDir;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sharing game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CompressAndUploadAsync(string gamePath, string gameName, bool isCracked,
            string format, string level, bool upload, bool encryptForRIN, Form parentForm)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string prefix = isCracked ? "[CRACKED]" : "[CLEAN]";
                string zipName = $"{prefix} {gameName}.{format.ToLower()}";
                string zipPath = Path.Combine(desktopPath, zipName);

                if (parentForm.IsHandleCreated)
                {
                    parentForm.Invoke(new Action(() => {
                        Tit($"Compressing {gameName}...", Color.Cyan);
                    }));
                }

                // Perform compression
                bool compressionSuccess = await CompressGameAsync(gamePath, zipPath, format, level, encryptForRIN);

                if (!compressionSuccess)
                {
                    if (parentForm.IsHandleCreated)
                    {
                        parentForm.Invoke(new Action(() => {
                            Tit($"Compression failed!", Color.Red);
                        }));
                    }
                    return;
                }

                if (upload)
                {
                    if (parentForm.IsHandleCreated)
                    {
                        parentForm.Invoke(new Action(() => {
                            Tit($"Uploading to YSG/HFP backend (6 month expiry)...", Color.Magenta);
                        }));
                    }

                    string uploadUrl = await UploadToBackend(zipPath, parentForm);

                    if (!string.IsNullOrEmpty(uploadUrl))
                    {
                        // Show success with copy button
                        ShowUploadSuccess(uploadUrl, gameName, isCracked, parentForm);
                    }
                }
                else
                {
                    if (parentForm.IsHandleCreated)
                    {
                        parentForm.Invoke(new Action(() => {
                            Tit($"Saved to Desktop: {zipName}", Color.Green);
                        }));
                    }
                    Process.Start("explorer.exe", desktopPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Compression/Upload failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<bool> CompressGameAsync(string sourcePath, string outputPath, string format, string level, bool encryptForRIN)
        {
            try
            {
                // Build 7-zip command
                string sevenZipPath = @"C:\Program Files\7-Zip\7z.exe";
                if (!File.Exists(sevenZipPath))
                {
                    sevenZipPath = @"C:\Program Files (x86)\7-Zip\7z.exe";
                    if (!File.Exists(sevenZipPath))
                    {
                        // Fall back to System.IO.Compression for basic zip without password
                        if (format.ToLower() == "zip" && !encryptForRIN)
                        {
                            await Task.Run(() => {
                                System.IO.Compression.ZipFile.CreateFromDirectory(sourcePath, outputPath,
                                    System.IO.Compression.CompressionLevel.Optimal, false);
                            });
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("7-Zip not found! Please install 7-Zip for advanced compression.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }

                // Map compression level
                string compressionSwitch = "";
                switch (level.ToLower())
                {
                    case "no compression":
                        compressionSwitch = "-mx0";
                        break;
                    case "fast":
                        compressionSwitch = "-mx1";
                        break;
                    case "normal":
                        compressionSwitch = "-mx5";
                        break;
                    case "maximum":
                        compressionSwitch = "-mx9";
                        break;
                    case "ultra":
                        compressionSwitch = "-mx9 -mfb=273 -ms=on";
                        break;
                }

                // Build command arguments
                string archiveType = format.ToLower() == "7z" ? "7z" : "zip";
                string passwordArg = encryptForRIN ? "-p\"cs.rin.ru\" -mhe=on" : "";

                string arguments = $"a -t{archiveType} {compressionSwitch} {passwordArg} \"{outputPath}\" \"{sourcePath}\\*\" -r";

                // Execute 7-zip
                var processInfo = new ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    await Task.Run(() => process.WaitForExit());
                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Compression error: {ex.Message}");
                return false;
            }
        }

        private async Task<string> UploadToBackend(string filePath, Form parentForm)
        {
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] === Starting upload process ===");
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] File path: {filePath}");
            System.Diagnostics.Debug.WriteLine($"[UPLOAD] File exists: {File.Exists(filePath)}");

            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] File size: {fileInfo.Length / (1024.0 * 1024.0):F2} MB");
            }

            System.Diagnostics.Debug.WriteLine($"[UPLOAD] Parent form handle created: {parentForm.IsHandleCreated}");

            try
            {
                // Check if the form handle is created before invoking
                if (!parentForm.IsHandleCreated)
                {
                    // If handle not created, we can't show UI updates
                    System.Diagnostics.Debug.WriteLine("[UPLOAD] Warning: Form handle not created, UI updates disabled");
                    // Continue with upload anyway
                }
                System.Diagnostics.Debug.WriteLine("[UPLOAD] Creating HttpClient...");
                using (var client = new HttpClient())
                {
                    // SACGUI backend configuration - no auth headers needed
                    System.Diagnostics.Debug.WriteLine("[UPLOAD] Configuring client for SACGUI...");
                    client.Timeout = TimeSpan.FromHours(2); // Allow for large files
                    System.Diagnostics.Debug.WriteLine("[UPLOAD] Timeout set to 2 hours");

                    var fileInfo = new FileInfo(filePath);
                    long fileSize = fileInfo.Length;

                    // Use multipart form for upload
                    System.Diagnostics.Debug.WriteLine("[UPLOAD] Creating multipart form content...");
                    using (var content = new MultipartFormDataContent())
                    {
                        // Read file in chunks to avoid memory issues with large files
                        System.Diagnostics.Debug.WriteLine("[UPLOAD] Opening file stream...");
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            System.Diagnostics.Debug.WriteLine("[UPLOAD] File stream opened successfully");

                            // Check file size before upload
                            var uploadFileInfo = new FileInfo(filePath);
                            var fileSizeMB = uploadFileInfo.Length / (1024.0 * 1024.0);
                            System.Diagnostics.Debug.WriteLine($"[UPLOAD] File size: {fileSizeMB:F2} MB");

                            if (fileSizeMB > 500)
                            {
                                System.Diagnostics.Debug.WriteLine($"[UPLOAD] WARNING: File is larger than 500MB, may exceed server limits!");

                                // Show warning to user
                                if (parentForm.IsHandleCreated)
                                {
                                    parentForm.Invoke(new Action(() => {
                                        MessageBox.Show($"File is {fileSizeMB:F0}MB. The server may reject files over 500MB.\n\nConsider using higher compression settings.",
                                            "Large File Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }));
                                }
                            }

                            var fileContent = new StreamContent(fileStream);
                            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                            content.Add(fileContent, "file", Path.GetFileName(filePath));

                            // Add required form fields for SACGUI
                            string hwid = HWIDManager.GetHWID(); // Get the hardware ID
                            content.Add(new StringContent(hwid), "hwid");
                            content.Add(new StringContent("SACGUI-2.0"), "version");

                            // Extract game name from filename (remove prefix and extension)
                            string fileName = Path.GetFileNameWithoutExtension(filePath);
                            string gameName = fileName.Replace("[CRACKED] ", "").Replace("[CLEAN] ", "").Replace("[SACGUI] ", "");
                            content.Add(new StringContent(gameName), "game_name");

                            // Get client IP - use local IP for now
                            string clientIp = "127.0.0.1";
                            try
                            {
                                using (var ipClient = new HttpClient())
                                {
                                    ipClient.Timeout = TimeSpan.FromSeconds(5);
                                    var ipResponse = await ipClient.GetStringAsync("https://api.ipify.org");
                                    if (!string.IsNullOrEmpty(ipResponse))
                                        clientIp = ipResponse.Trim();
                                }
                            }
                            catch { }
                            content.Add(new StringContent(clientIp), "client_ip");

                            System.Diagnostics.Debug.WriteLine($"[UPLOAD] Added HWID: {hwid}");
                            System.Diagnostics.Debug.WriteLine($"[UPLOAD] Added Version: SACGUI-2.0");
                            System.Diagnostics.Debug.WriteLine($"[UPLOAD] Added Game Name: {gameName}");
                            System.Diagnostics.Debug.WriteLine($"[UPLOAD] Added Client IP: {clientIp}");

                            // Upload with progress tracking
                            var progressHandler = new ProgressMessageHandler();
                            progressHandler.HttpSendProgress += (s, e) => {
                                int percentage = (int)((e.BytesTransferred * 100) / fileSize);
                                if (parentForm.IsHandleCreated)
                                {
                                    parentForm.Invoke(new Action(() => {
                                        Tit($"Uploading... {percentage}%", Color.Magenta);
                                    }));
                                }
                            };

                            using (var progressClient = new HttpClient(progressHandler))
                            {
                                // Set timeout and user agent
                                progressClient.Timeout = TimeSpan.FromHours(2);
                                progressClient.DefaultRequestHeaders.Add("User-Agent", "SACGUI-Uploader/2.0");

                                // Use 1fichier upload
                                System.Diagnostics.Debug.WriteLine("[UPLOAD] Using 1fichier for file upload");
                                System.Diagnostics.Debug.WriteLine("[UPLOAD] Request starting...");

                                // Create progress handler for 1fichier upload
                                var progress = new Progress<double>(value =>
                                {
                                    int percentage = (int)(value * 100);
                                    if (parentForm.IsHandleCreated)
                                    {
                                        parentForm.Invoke(new Action(() => {
                                            Tit($"Uploading to 1fichier... {percentage}%", Color.Magenta);
                                        }));
                                    }
                                });

                                // Use 1fichier uploader
                                using (var uploader = new OneFichierUploader())
                                {
                                    var uploadResult = await uploader.UploadFileAsync(filePath, progress);

                                    System.Diagnostics.Debug.WriteLine($"[UPLOAD] 1fichier upload successful!");

                                    if (!string.IsNullOrEmpty(uploadResult.DownloadUrl))
                                    {
                                        string shareUrl = uploadResult.DownloadUrl;
                                        System.Diagnostics.Debug.WriteLine($"[UPLOAD] Download URL: {shareUrl}");
                                        System.Diagnostics.Debug.WriteLine($"[UPLOAD] Extracted share URL: {shareUrl}");
                                        return shareUrl;
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine("[UPLOAD] WARNING: Upload succeeded but no URL was returned");
                                        throw new Exception("Upload succeeded but no download URL was returned");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] HTTP Request Exception: {httpEx.Message}");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Inner Exception: {httpEx.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Stack Trace:\n{httpEx.StackTrace}");

                if (parentForm.IsHandleCreated)
                {
                    parentForm.Invoke(new Action(() => {
                        MessageBox.Show($"Upload failed (HTTP Error):\n{httpEx.Message}\n\nInner: {httpEx.InnerException?.Message}", "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            }
            catch (TaskCanceledException tcEx)
            {
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Task Cancelled/Timeout: {tcEx.Message}");

                if (parentForm.IsHandleCreated)
                {
                    parentForm.Invoke(new Action(() => {
                        MessageBox.Show("Upload failed: Request timed out (file too large or slow connection)", "Upload Timeout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] General Exception: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Inner Exception: {ex.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Stack Trace:\n{ex.StackTrace}");

                if (parentForm.IsHandleCreated)
                {
                    parentForm.Invoke(new Action(() => {
                        MessageBox.Show($"Upload failed:\n{ex.Message}\n\nType: {ex.GetType().Name}\n\nInner: {ex.InnerException?.Message}", "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            }

            System.Diagnostics.Debug.WriteLine("[UPLOAD] === Upload process ended with failure ===");
            return null;
        }

        // Progress handler for upload tracking
        private class ProgressMessageHandler : HttpClientHandler
        {
            public event EventHandler<HttpProgressEventArgs> HttpSendProgress;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // For now, just send without progress tracking
                // Full implementation would require tracking the request stream
                var response = await base.SendAsync(request, cancellationToken);

                // Simulate progress complete
                HttpSendProgress?.Invoke(this, new HttpProgressEventArgs(0, 100, 100));

                return response;
            }
        }

        public class HttpProgressEventArgs : EventArgs
        {
            public long BytesTransferred { get; }
            public long? TotalBytes { get; }
            public int ProgressPercentage { get; }

            public HttpProgressEventArgs(int progressPercentage, long bytesTransferred, long? totalBytes)
            {
                ProgressPercentage = progressPercentage;
                BytesTransferred = bytesTransferred;
                TotalBytes = totalBytes;
            }
        }

        private void ShowUploadSuccess(string url, string gameName, bool isCracked, Form parentForm)
        {
            var successForm = new Form();
            successForm.Text = "Upload Complete!";
            successForm.Size = new Size(600, 200);
            successForm.StartPosition = FormStartPosition.CenterParent;
            successForm.BackColor = Color.FromArgb(0, 20, 50);
            successForm.Owner = parentForm;

            var label = new Label();
            label.Text = $"{(isCracked ? "CRACKED" : "CLEAN")} {gameName}\nUploaded Successfully!\nLink valid for 6 months";
            label.AutoSize = false;
            label.Size = new Size(580, 60);
            label.Location = new Point(10, 20);
            label.ForeColor = Color.FromArgb(192, 255, 255);
            label.TextAlign = ContentAlignment.MiddleCenter;

            var urlTextBox = new TextBox();
            urlTextBox.Text = url;
            urlTextBox.ReadOnly = true;
            urlTextBox.Size = new Size(400, 25);
            urlTextBox.Location = new Point(50, 90);
            urlTextBox.BackColor = Color.FromArgb(0, 0, 100);
            urlTextBox.ForeColor = Color.White;

            var copyButton = new Button();
            copyButton.Text = "📋 Copy";
            copyButton.Size = new Size(100, 25);
            copyButton.Location = new Point(460, 90);
            copyButton.FlatStyle = FlatStyle.Flat;
            copyButton.ForeColor = Color.FromArgb(192, 255, 255);
            copyButton.Click += (s, e) => {
                Clipboard.SetText(url);
                copyButton.Text = "✓ Copied!";
                Task.Delay(2000).ContinueWith(t => {
                    if (copyButton.IsHandleCreated)
                    {
                        copyButton.Invoke(new Action(() => copyButton.Text = "📋 Copy"));
                    }
                });
            };

            successForm.Controls.Add(label);
            successForm.Controls.Add(urlTextBox);
            successForm.Controls.Add(copyButton);
            successForm.ShowDialog();
        }

        private async void ZipToShare_Click(object sender, EventArgs e)
        {
            ZipToShare.Enabled = false;
            ZipToShare.Text = "Zip Dir";  // Ensure text stays visible

            try
            {
                string gameName = gameDirName;
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                string compressionType = Properties.Settings.Default.ZipFormat;
                string compressionLevelStr = Properties.Settings.Default.ZipLevel;
                bool skipDialog = Properties.Settings.Default.ZipDontAsk;

                // Show dialog if not saved or if user wants to be asked (or Ctrl+S was pressed)
                if (!skipDialog || string.IsNullOrEmpty(compressionType))
                {
                    // Use the custom CompressionSettingsForm
                    using (var compressionForm = new CompressionSettingsForm())
                    {
                        compressionForm.Owner = this;
                        compressionForm.StartPosition = FormStartPosition.CenterParent;
                        compressionForm.TopMost = true;
                        compressionForm.BringToFront();
                        if (compressionForm.ShowDialog(this) != DialogResult.OK)
                        {
                            ZipToShare.Enabled = true;
                            ZipToShare.Text = "Zip Dir";  // Ensure text stays visible
                            // Keep both buttons visible if user cancels
                            ZipToShare.Visible = true;
                            OpenDir.Visible = true;
                            return;
                        }

                        compressionType = compressionForm.SelectedFormat;
                        compressionLevelStr = compressionForm.SelectedLevel;

                        // Save preferences if requested
                        if (compressionForm.RememberChoice)
                    {
                            Properties.Settings.Default.ZipFormat = compressionType;
                            Properties.Settings.Default.ZipLevel = compressionLevelStr;
                            Properties.Settings.Default.ZipDontAsk = true;
                            Properties.Settings.Default.Save();
                        }
                    }
                }

                bool use7z = compressionType.StartsWith("7Z");
                System.IO.Compression.CompressionLevel compressionLevel;

                // Parse compression level from string
                int levelNum = 0;
                if (!string.IsNullOrEmpty(compressionLevelStr))
                {
                    // Try to parse the number from the string
                    var match = System.Text.RegularExpressions.Regex.Match(compressionLevelStr, @"\d+");
                    if (match.Success)
                    {
                        int.TryParse(match.Value, out levelNum);
                    }
                }

                // Map the level number to compression levels
                if (levelNum == 0)
                    compressionLevel = System.IO.Compression.CompressionLevel.NoCompression;
                else if (levelNum <= 3)
                    compressionLevel = System.IO.Compression.CompressionLevel.Fastest;
                else if (levelNum >= 7)
                    compressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                else
                    compressionLevel = System.IO.Compression.CompressionLevel.Fastest; // Medium

                string zipName = use7z ? $"[SACGUI] {gameName}.7z" : $"[SACGUI] {gameName}.zip";
                string zipPath = Path.Combine(desktopPath, zipName);

                // RGB color array - pastel and neon colors
                Color[] rgbColors = new Color[]
                {
                    Color.FromArgb(255, 182, 193),  // Light Pink
                    Color.FromArgb(255, 255, 153),  // Light Yellow
                    Color.FromArgb(255, 153, 153),  // Light Red
                    Color.FromArgb(153, 255, 153),  // Light Green
                    Color.FromArgb(153, 204, 255),  // Light Blue
                    Color.FromArgb(221, 160, 221),  // Plum
                    Color.FromArgb(255, 218, 185),  // Peach
                    Color.FromArgb(230, 230, 250),  // Lavender
                    Color.FromArgb(152, 255, 152),  // Pale Green
                    Color.FromArgb(255, 192, 203),  // Pink
                    Color.FromArgb(255, 160, 122),  // Light Salmon
                    Color.FromArgb(176, 224, 230),  // Powder Blue
                    Color.FromArgb(255, 228, 181),  // Moccasin
                    Color.FromArgb(216, 191, 216),  // Thistle
                    Color.FromArgb(127, 255, 212),  // Aquamarine
                };

                if (use7z)
                {
                    // Use 7zip for ultra compression
                    await Task.Run(async () =>
                    {
                        int colorIndex = 0;

                        // Use 7za.exe from _bin folder
                        string sevenZipPath = $"{Environment.CurrentDirectory}\\_bin\\7z\\7za.exe";

                        // Build 7z command - mx9 for maximum compression
                        string args = $"a -mx9 -t7z \"{zipPath}\" \"{gameDir}\\*\" -r";

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = sevenZipPath,
                            Arguments = args,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        };

                        using (Process p = Process.Start(psi))
                        {
                            string line;
                            while ((line = p.StandardOutput.ReadLine()) != null)
                            {
                                // RGB cycling while 7z works
                                Color currentColor = rgbColors[colorIndex % rgbColors.Length];
                                colorIndex++;

                                this.Invoke(new Action(() =>
                                {
                                    Tit($"Ultra compressing with 7z...", currentColor);
                                }));

                                await Task.Delay(100);
                            }
                            p.WaitForExit();
                        }
                    });
                }
                else
                {
                    // Use standard .NET zip
                    await Task.Run(async () =>
                    {
                        var allFiles = Directory.GetFiles(gameDir, "*", SearchOption.AllDirectories);
                        int totalFiles = allFiles.Length;
                        int currentFile = 0;
                        int colorIndex = 0;

                        using (var archive = System.IO.Compression.ZipFile.Open(zipPath, System.IO.Compression.ZipArchiveMode.Create))
                        {
                            foreach (string file in allFiles)
                            {
                                currentFile++;
                                int percentage = (currentFile * 100) / totalFiles;

                                // Cycle through RGB colors
                                Color currentColor = rgbColors[colorIndex % rgbColors.Length];
                                colorIndex++;

                                this.Invoke(new Action(() =>
                                {
                                    string compressionText = compressionLevel == System.IO.Compression.CompressionLevel.NoCompression ? "Zipping (fast)..." : "Compressing...";
                                    Tit($"{compressionText} {percentage}%", currentColor);
                                }));

                                string relativePath = file.Replace(gameDir + Path.DirectorySeparatorChar, "");
                                var entry = archive.CreateEntry(Path.Combine(gameName, relativePath), compressionLevel);
                                using (var entryStream = entry.Open())
                                using (var fileStream = File.OpenRead(file))
                                {
                                    fileStream.CopyTo(entryStream);
                                }

                                // Update color every 500ms or every 10 files, whichever comes first
                                if (currentFile % 10 == 0)
                                {
                                    await Task.Delay(50);
                                }
                            }
                        }
                    });
                }

                // Final RGB flash
                for (int i = 0; i < 10; i++)
                {
                    Tit($"Saved to Desktop: {zipName}", rgbColors[i % rgbColors.Length]);
                    await Task.Delay(100);
                }

                Process.Start("explorer.exe", desktopPath);
            }
            catch (Exception ex)
            {
                Tit($"Zip failed: {ex.Message}", Color.Red);
            }
            finally
            {
                ZipToShare.Enabled = true;
                ZipToShare.Text = "Zip Dir";  // Ensure text stays visible
                // Keep both buttons visible after zipping
                ZipToShare.Visible = true;
                OpenDir.Visible = true;
            }
        }

        private void ManAppPanel_Paint(object sender, PaintEventArgs e)
        {
            // Draw modern border with subtle glow
            using (Pen borderPen = new Pen(Color.FromArgb(180, 100, 150, 200), 2))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, ManAppPanel.Width - 1, ManAppPanel.Height - 1);
            }
        }

        private void ManAppBtn_Click(object sender, EventArgs e)
        {
            if (ManAppBox.Text.Length > 2 && isnumeric)
            {
                APPID = ManAppBox.Text;
                APPNAME = "";

                // Try to fetch game name from Steam API
                FetchGameNameFromSteamAPI(APPID);

                searchTextBox.Clear();
                ManAppBox.Clear();
                ManAppPanel.Visible = false;
                mainPanel.Visible = true;
                btnManualEntry.Visible = false;
                startCrackPic.Visible = true;
                Tit("READY! Click skull folder above to perform crack!", Color.LightSkyBlue);

                resinstruccZip.Visible = false;

                // Auto-crack if enabled
                if (autoCrackEnabled && !string.IsNullOrEmpty(gameDir))
                {
                    // Trigger crack just like clicking the button
                    startCrackPic_Click(null, null);
                }
            }
            else
            {
                MessageBox.Show("Enter APPID or press ESC to cancel!", "No APPID entered!");
            }
        }

        private void ManAppBox_TextChanged(object sender, EventArgs e)
        {
            double parsedValue;

            if (!double.TryParse(ManAppBox.Text, out parsedValue))
            {
                ManAppBox.Text = "";
                isnumeric= false;
            }
            if (ManAppBox.Text.Length > 0)
            {
                ManAppBtn.Enabled = true;
                isnumeric= true;    
            }
            else
            {
                isnumeric= false;
                ManAppBtn.Enabled = false;
            }
   
        }
        public bool isnumeric = false;

        private async void FetchGameNameFromSteamAPI(string appId)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string url = $"https://store.steampowered.com/api/appdetails?appids={appId}";
                    string json = await client.DownloadStringTaskAsync(url);

                    // Simple JSON parsing without external libraries
                    if (json.Contains($"\"{appId}\":{{\"success\":true"))
                    {
                        int nameIndex = json.IndexOf("\"name\":\"");
                        if (nameIndex != -1)
                        {
                            nameIndex += 8; // Length of "name":"
                            int endIndex = json.IndexOf("\"", nameIndex);
                            if (endIndex != -1)
                            {
                                APPNAME = json.Substring(nameIndex, endIndex - nameIndex);
                                Tat($"{APPNAME} ({appId})");
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If API lookup fails, just show the AppID
                System.Diagnostics.Debug.WriteLine($"Failed to fetch game name: {ex.Message}");
            }

            // If we couldn't get the name, just show the AppID
            Tat($"AppID: {appId}");
        }

        private void btnManualEntry_Click(object sender, EventArgs e)
        {
            ManAppPanel.Visible = true;
            ManAppPanel.BringToFront();  // Make sure it's on top!
            ManAppBox.Focus();
            ManAppBtn.Visible = true;
        }


        private void ManAppBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                if (ManAppBox.Text.Length > 2 && isnumeric)
                {
                    APPID = ManAppBox.Text;
                    APPNAME = "";
                    btnManualEntry.Visible = false;
                    // Try to fetch game name from Steam API
                    FetchGameNameFromSteamAPI(APPID);

                    searchTextBox.Clear();
                    ManAppBox.Clear();
                    ManAppPanel.Visible = false;
                    mainPanel.Visible = true;
                    btnManualEntry.Visible = false;
                    startCrackPic.Visible = true;
                    Tit("READY! Click skull folder above to perform crack!", Color.HotPink);

                    resinstruccZip.Visible = false;

                    // Auto-crack if enabled
                    if (autoCrackEnabled && !string.IsNullOrEmpty(gameDir))
                    {
                        // Trigger crack just like clicking the button
                        startCrackPic_Click(null, null);
                    }
                }
                else
                {
                    MessageBox.Show("Enter APPID or press ESC to cancel!");
                }

            }
            if (e.KeyCode == Keys.Escape) {
                CloseManAppPanel();
            }
        }

        private void CloseManAppPanel()
        {
            ManAppBox.Clear();
            ManAppPanel.Visible = false;
            searchTextBox.Focus();
        }

        private void Form_Click(object sender, EventArgs e)
        {
            if (ManAppPanel.Visible)
            {
                CloseManAppPanel();
            }
        }

        private void searchTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            // Only clear on FIRST click after folder/file selection
            if (isFirstClickAfterSelection)
            {
                searchTextBox.Clear();
                isFirstClickAfterSelection = false;  // Reset flag
            }
        }

        private void ShareButton_Click(object sender, EventArgs e)
        {
            // Show the enhanced share window with your Steam games
            var shareForm = new EnhancedShareWindow(this);
            shareForm.Show();
        }

        private void RequestButton_Click(object sender, EventArgs e)
        {
            // Show the requests window for browsing and requesting games
            var requestForm = new RequestsWindow(this);
            requestForm.Show();
        }

        public System.Data.DataTable GetDataTable()
        {
            return dataTableGeneration?.DataTableToGenerate;
        }

        private void ShowSteamGamesForm()
        {
            // Create new form for Steam games list
            var gamesForm = new Form();
            gamesForm.Text = "Your Steam Library - Share with Friends!";
            gamesForm.Size = new Size(900, 600);
            gamesForm.StartPosition = FormStartPosition.CenterParent;
            gamesForm.BackColor = Color.FromArgb(0, 20, 50);
            gamesForm.ForeColor = Color.FromArgb(192, 255, 255);
            gamesForm.Owner = this;  // Set owner so it appears above main form

            // Create DataGridView
            var gamesGrid = new DataGridView();
            gamesGrid.Dock = DockStyle.Fill;
            gamesGrid.BackgroundColor = Color.FromArgb(0, 2, 10);
            gamesGrid.ForeColor = Color.FromArgb(192, 255, 255);
            gamesGrid.GridColor = Color.FromArgb(0, 50, 100);
            gamesGrid.BorderStyle = BorderStyle.None;
            gamesGrid.AllowUserToAddRows = false;
            gamesGrid.AllowUserToDeleteRows = false;
            gamesGrid.ReadOnly = true;
            gamesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gamesGrid.RowHeadersVisible = false;
            gamesGrid.DefaultCellStyle.BackColor = Color.FromArgb(0, 2, 10);
            gamesGrid.DefaultCellStyle.ForeColor = Color.FromArgb(192, 255, 255);
            gamesGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 50, 100);
            gamesGrid.DefaultCellStyle.SelectionForeColor = Color.White;
            gamesGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 30, 60);
            gamesGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(220, 255, 255);
            gamesGrid.EnableHeadersVisualStyles = false;

            // Add columns - simplified view with build info
            var gameNameColumn = new DataGridViewTextBoxColumn();
            gameNameColumn.Name = "GameName";
            gameNameColumn.HeaderText = "Game Title";
            gameNameColumn.Width = 350;
            gamesGrid.Columns.Add(gameNameColumn);

            var buildIdColumn = new DataGridViewTextBoxColumn();
            buildIdColumn.Name = "BuildID";
            buildIdColumn.HeaderText = "Build ID";
            buildIdColumn.Width = 100;
            buildIdColumn.DefaultCellStyle.ForeColor = Color.FromArgb(150, 200, 255);
            gamesGrid.Columns.Add(buildIdColumn);

            var statusColumn = new DataGridViewTextBoxColumn();
            statusColumn.Name = "RINStatus";
            statusColumn.HeaderText = "RIN Status";
            statusColumn.Width = 120;
            gamesGrid.Columns.Add(statusColumn);

            // Hidden columns for data
            var appIdColumn = new DataGridViewTextBoxColumn();
            appIdColumn.Name = "AppID";
            appIdColumn.Visible = false;
            gamesGrid.Columns.Add(appIdColumn);

            var pathColumn = new DataGridViewTextBoxColumn();
            pathColumn.Name = "InstallPath";
            pathColumn.Visible = false;
            gamesGrid.Columns.Add(pathColumn);

            // Add button columns
            var searchRinColumn = new DataGridViewButtonColumn();
            searchRinColumn.Name = "SearchRIN";
            searchRinColumn.HeaderText = "Search RIN";
            searchRinColumn.Text = "🔍 RIN";
            searchRinColumn.UseColumnTextForButtonValue = true;
            searchRinColumn.Width = 90;
            searchRinColumn.FlatStyle = FlatStyle.Flat;
            searchRinColumn.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 100);
            searchRinColumn.DefaultCellStyle.ForeColor = Color.FromArgb(150, 200, 255);
            gamesGrid.Columns.Add(searchRinColumn);

            var cleanShareColumn = new DataGridViewButtonColumn();
            cleanShareColumn.Name = "CleanShare";
            cleanShareColumn.HeaderText = "Zip Clean";
            cleanShareColumn.Text = "📦 Clean";
            cleanShareColumn.UseColumnTextForButtonValue = true;
            cleanShareColumn.Width = 100;
            cleanShareColumn.FlatStyle = FlatStyle.Flat;
            cleanShareColumn.DefaultCellStyle.BackColor = Color.FromArgb(0, 100, 50);
            cleanShareColumn.DefaultCellStyle.ForeColor = Color.FromArgb(150, 255, 150);
            gamesGrid.Columns.Add(cleanShareColumn);

            var crackedShareColumn = new DataGridViewButtonColumn();
            crackedShareColumn.Name = "CrackedShare";
            crackedShareColumn.HeaderText = "Zip Cracked";
            crackedShareColumn.Text = "🎮 Cracked";
            crackedShareColumn.UseColumnTextForButtonValue = true;
            crackedShareColumn.Width = 100;
            crackedShareColumn.FlatStyle = FlatStyle.Flat;
            crackedShareColumn.DefaultCellStyle.BackColor = Color.FromArgb(100, 50, 0);
            crackedShareColumn.DefaultCellStyle.ForeColor = Color.FromArgb(255, 200, 100);
            gamesGrid.Columns.Add(crackedShareColumn);

            // Scan for Steam games
            var games = ScanSteamLibraries();

            // Populate grid - with build ID
            foreach (var game in games)
            {
                var rowIndex = gamesGrid.Rows.Add(game.Name, game.BuildId, "Checking...", game.AppId, game.InstallDir, "🔍 RIN", "📦 Clean", "🎮 Cracked");

                // Start background task to check RIN status
                _ = Task.Run(async () => {
                    try
                    {
                        // Update status to show we're searching
                        gamesForm.Invoke(new Action(() => {
                            if (rowIndex < gamesGrid.Rows.Count)
                            {
                                gamesGrid.Rows[rowIndex].Cells["RINStatus"].Value = "Searching...";
                            }
                        }));

                        // Search for the game on RIN - get multiple results
                        var searchResults = await SearchRinForGameAsync(game.Name);
                        RinCleanInfo rinInfo = null;
                        string successfulThreadUrl = null;

                        // Check each search result until we find clean files
                        foreach (var threadUrl in searchResults)
                        {
                            System.Diagnostics.Debug.WriteLine($"[CHECKING THREAD] {threadUrl}");
                            rinInfo = await ScrapeRinThreadAsync(threadUrl, game.Name);
                            if (rinInfo != null)
                            {
                                successfulThreadUrl = threadUrl;
                                System.Diagnostics.Debug.WriteLine($"[SUCCESS] Found clean files in this thread!");
                                break;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[SKIP] No clean files in this thread, trying next...");
                            }
                        }

                        if (rinInfo != null)
                        {
                            // Update the status cell on UI thread with actual info
                            gamesForm.Invoke(new Action(() => {
                                if (rowIndex < gamesGrid.Rows.Count)
                                {
                                    var row = gamesGrid.Rows[rowIndex];
                                    if (!string.IsNullOrEmpty(rinInfo.BuildId))
                                    {
                                        // Show the actual RIN build number
                                        row.Cells["RINStatus"].Value = $"RIN: {rinInfo.BuildId}";
                                        row.Cells["RINStatus"].Tag = rinInfo.PostUrl; // Store URL for later

                                        // Compare builds
                                        if (long.TryParse(game.BuildId, out long local) &&
                                            long.TryParse(rinInfo.BuildId, out long rin))
                                        {
                                            if (local > rin)
                                            {
                                                row.Cells["RINStatus"].Style.ForeColor = Color.FromArgb(100, 255, 100);
                                                row.Cells["RINStatus"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                                                row.DefaultCellStyle.BackColor = Color.FromArgb(0, 40, 20);
                                            }
                                            else if (local == rin)
                                            {
                                                row.Cells["RINStatus"].Style.ForeColor = Color.FromArgb(150, 150, 150);
                                            }
                                            else
                                            {
                                                row.Cells["RINStatus"].Style.ForeColor = Color.FromArgb(200, 150, 100);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Found thread but no build number
                                        row.Cells["RINStatus"].Value = "RIN: No build";
                                        row.Cells["RINStatus"].Tag = successfulThreadUrl;
                                        row.Cells["RINStatus"].Style.ForeColor = Color.FromArgb(255, 255, 100);
                                    }
                                }
                            }));
                        }
                        else
                        {
                            // No clean files found after checking 20 pages - THEY NEED IT BAD!
                            gamesForm.Invoke(new Action(() => {
                                if (rowIndex < gamesGrid.Rows.Count)
                                {
                                    var row = gamesGrid.Rows[rowIndex];

                                    if (searchResults.Count > 0)
                                    {
                                        // Thread exists but no clean files in 20 pages!
                                        row.Cells["RINStatus"].Value = "NEEDS CLEAN FILES!";
                                        row.Cells["RINStatus"].Style.BackColor = Color.Red;
                                        row.Cells["RINStatus"].Style.ForeColor = Color.White;
                                        row.Cells["RINStatus"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                                        row.DefaultCellStyle.BackColor = Color.FromArgb(40, 0, 0);
                                    }
                                    else
                                    {
                                        // Not on RIN at all
                                        row.Cells["RINStatus"].Value = "NOT ON RIN";
                                        row.Cells["RINStatus"].Style.ForeColor = Color.Gray;
                                    }
                                }
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        // Show the actual error instead of hiding it
                        gamesForm.Invoke(new Action(() => {
                            if (rowIndex < gamesGrid.Rows.Count)
                            {
                                gamesGrid.Rows[rowIndex].Cells["RINStatus"].Value = "Error";
                                gamesGrid.Rows[rowIndex].Cells["RINStatus"].ToolTipText = ex.Message;
                                gamesGrid.Rows[rowIndex].Cells["RINStatus"].Style.ForeColor = Color.Red;
                            }
                        }));
                        System.Diagnostics.Debug.WriteLine($"Error checking RIN for {game.Name}: {ex.Message}");
                    }
                });
            }

            // Handle button clicks
            gamesGrid.CellClick += async (s, args) =>
            {
                if (args.RowIndex < 0) return;

                var gameName = gamesGrid.Rows[args.RowIndex].Cells["GameName"].Value.ToString();
                var appId = gamesGrid.Rows[args.RowIndex].Cells["AppID"].Value?.ToString();
                var installPath = gamesGrid.Rows[args.RowIndex].Cells["InstallPath"].Value?.ToString();

                if (string.IsNullOrEmpty(installPath))
                {
                    MessageBox.Show("Game installation path not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (args.ColumnIndex == gamesGrid.Columns["SearchRIN"].Index)
                {
                    // Search cs.rin.ru for this game
                    var sanitized = SanitizeGameNameForURL(gameName);
                    var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords={sanitized}&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
                    Process.Start(searchUrl);
                }
                else if (args.ColumnIndex == gamesGrid.Columns["CleanShare"].Index)
                {
                    await ShareGameAsync(gameName, installPath, false, gamesForm); // Clean share
                }
                else if (args.ColumnIndex == gamesGrid.Columns["CrackedShare"].Index)
                {
                    await ShareGameAsync(gameName, installPath, true, gamesForm); // Cracked share
                }
            };

            // Add status label
            var statusLabel = new Label();
            statusLabel.Text = $"Found {games.Count} games in your Steam library";
            statusLabel.Dock = DockStyle.Bottom;
            statusLabel.Height = 30;
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            statusLabel.BackColor = Color.FromArgb(0, 30, 60);
            statusLabel.ForeColor = Color.FromArgb(192, 255, 255);

            gamesForm.Controls.Add(gamesGrid);
            gamesForm.Controls.Add(statusLabel);
            gamesForm.ShowDialog();
        }

        private string SanitizeGameNameForURL(string gameName)
        {
            // First replace spaces with %20
            var result = gameName.Replace(" ", "%20");

            // Remove special characters
            var charsToRemove = new[] { '-', ':', '_', '&', '!', '?', ',', '.', '(', ')', '$', '#', '%', '+', ';', '\'', '`', '"' };

            foreach (var c in charsToRemove)
            {
                result = result.Replace(c.ToString(), "");
            }

            return result;
        }

        private List<SteamGame> ScanSteamLibraries()
        {
            var games = new List<SteamGame>();
            var steamPaths = new List<string>();

            // Add default Steam path
            var defaultSteamPath = @"C:\Program Files (x86)\Steam\steamapps";
            if (Directory.Exists(defaultSteamPath))
            {
                steamPaths.Add(defaultSteamPath);
            }

            // Also check 64-bit Program Files
            var steam64Path = @"C:\Program Files\Steam\steamapps";
            if (Directory.Exists(steam64Path))
            {
                steamPaths.Add(steam64Path);
            }

            // Check all drives for SteamLibrary folders
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    var steamLibPath = Path.Combine(drive.Name, "SteamLibrary", "steamapps");
                    if (Directory.Exists(steamLibPath) && !steamPaths.Contains(steamLibPath))
                    {
                        steamPaths.Add(steamLibPath);
                    }
                }
            }

            // Look for additional library folders in libraryfolders.vdf
            foreach (var basePath in steamPaths.ToList())
            {
                var libraryFoldersPath = Path.Combine(basePath, "libraryfolders.vdf");
                if (File.Exists(libraryFoldersPath))
                {
                    try
                    {
                        var content = File.ReadAllText(libraryFoldersPath);
                        // Simple regex to find paths
                        var pathMatches = Regex.Matches(content, @"""path""\s+""([^""]+)""");
                        foreach (Match match in pathMatches)
                        {
                            var libPath = Path.Combine(match.Groups[1].Value.Replace(@"\\", @"\"), "steamapps");
                            if (Directory.Exists(libPath) && !steamPaths.Contains(libPath))
                            {
                                steamPaths.Add(libPath);
                            }
                        }
                    }
                    catch { }
                }
            }

            // Scan each Steam library for appmanifest files
            foreach (var steamPath in steamPaths)
            {
                try
                {
                    var manifestFiles = Directory.GetFiles(steamPath, "appmanifest_*.acf");

                    foreach (var manifestFile in manifestFiles)
                    {
                        var game = ParseAppManifest(manifestFile);
                        if (game != null && !games.Any(g => g.AppId == game.AppId))
                        {
                            games.Add(game);
                        }
                    }
                }
                catch { }
            }

            // Sort by name
            return games.OrderBy(g => g.Name).ToList();
        }

        private SteamGame ParseAppManifest(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath);

                // Extract appid
                var appIdMatch = Regex.Match(content, @"""appid""\s+""(\d+)""");
                if (!appIdMatch.Success) return null;

                // Extract name
                var nameMatch = Regex.Match(content, @"""name""\s+""([^""]+)""");
                if (!nameMatch.Success) return null;

                // Extract install dir
                var installDirMatch = Regex.Match(content, @"""installdir""\s+""([^""]+)""");
                var installDir = installDirMatch.Success ? installDirMatch.Groups[1].Value : "";

                // Extract build id
                var buildIdMatch = Regex.Match(content, @"""buildid""\s+""([^""]+)""");
                var buildId = buildIdMatch.Success ? buildIdMatch.Groups[1].Value : "Unknown";

                // Get the actual path
                var manifestDir = Path.GetDirectoryName(filePath);
                var commonDir = Path.Combine(manifestDir, "common", installDir);

                // Debug output
                System.Diagnostics.Debug.WriteLine($"Game: {nameMatch.Groups[1].Value}");
                System.Diagnostics.Debug.WriteLine($"  Manifest: {filePath}");
                System.Diagnostics.Debug.WriteLine($"  CommonDir: {commonDir}");
                System.Diagnostics.Debug.WriteLine($"  Exists: {Directory.Exists(commonDir)}");

                return new SteamGame
                {
                    AppId = appIdMatch.Groups[1].Value,
                    Name = nameMatch.Groups[1].Value,
                    InstallDir = commonDir,  // Always set the path, even if directory doesn't exist yet
                    BuildId = buildId
                };
            }
            catch
            {
                return null;
            }
        }

        private class SteamGame
        {
            public string AppId { get; set; }
            public string Name { get; set; }
            public string InstallDir { get; set; }
            public string BuildId { get; set; }
        }

        private class RinCleanInfo
        {
            public string BuildId { get; set; }
            public string BuildNumber { get; set; }
            public DateTime PostDate { get; set; }
            public string PostUrl { get; set; }
            public bool HasCleanFiles { get; set; }
            public string Status { get; set; }
        }

        private RinSeleniumScraper rinScraper;

        private async Task<List<string>> SearchRinForGameAsync(string gameName)
        {
            var results = new List<string>();
            try
            {
                System.Diagnostics.Debug.WriteLine($"\n{'='*80}");
                System.Diagnostics.Debug.WriteLine($"SEARCHING RIN FOR: {gameName}");
                System.Diagnostics.Debug.WriteLine($"{'='*80}");

                // Initialize Selenium scraper if needed
                if (rinScraper == null)
                {
                    rinScraper = new RinSeleniumScraper();
                    if (!await rinScraper.Initialize(this))
                    {
                        System.Diagnostics.Debug.WriteLine("[ERROR] Failed to initialize Selenium scraper");
                        return results;
                    }
                }

                // Search using Selenium
                var gameInfo = await rinScraper.SearchAndScrapeGame(gameName);
                if (gameInfo != null && !string.IsNullOrEmpty(gameInfo.ThreadUrl))
                {
                    results.Add(gameInfo.ThreadUrl);
                }
                System.Diagnostics.Debug.WriteLine($"[SELENIUM] Found {results.Count} results for '{gameName}'");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EXCEPTION] {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[STACK] {ex.StackTrace}");
            }
            return results;
        }

        private async Task<RinCleanInfo> ScrapeRinThreadAsync(string threadUrl, string gameName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"\n{'='*80}");
                System.Diagnostics.Debug.WriteLine($"SCRAPING THREAD FOR: {gameName}");
                System.Diagnostics.Debug.WriteLine($"{'='*80}");
                System.Diagnostics.Debug.WriteLine($"[THREAD URL] {threadUrl}");

                // Use Selenium to scrape the thread
                if (rinScraper == null)
                {
                    rinScraper = new RinSeleniumScraper();
                    if (!await rinScraper.Initialize(this))
                    {
                        System.Diagnostics.Debug.WriteLine("[ERROR] Failed to initialize Selenium scraper");
                        return null;
                    }
                }

                // Since ScrapeThread doesn't exist, use SearchAndScrapeGame again with the game name
                var gameInfo = await rinScraper.SearchAndScrapeGame(gameName);
                if (gameInfo != null)
                {
                    return new RinCleanInfo
                    {
                        BuildNumber = gameInfo.BuildNumber,
                        HasCleanFiles = !string.IsNullOrEmpty(gameInfo.BuildNumber),
                        Status = gameInfo.Status
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR scraping thread: {ex.Message}");
            }
            return null;
        }

        // This method was removed due to broken implementation
        /* Original code had 250+ lines of unreachable code here that was deleted */
        private void UpdateStatusCell(DataGridViewRow row, string localBuild, string rinBuild)
        {
            // Clear any existing styles first
            row.DefaultCellStyle.BackColor = Color.FromArgb(0, 2, 10);

            if (string.IsNullOrEmpty(rinBuild))
            {
                // No clean files found on RIN - URGENT NEED!
                row.Cells["RINStatus"].Value = "🚨 NEEDED!";
                row.Cells["RINStatus"].Style.ForeColor = Color.FromArgb(255, 100, 100);
                row.Cells["RINStatus"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);

                // Highlight entire row in red tint
                row.DefaultCellStyle.BackColor = Color.FromArgb(40, 0, 0);

                // Make Clean Share button glow
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.OwningColumn.Name == "CleanShare")
                    {
                        cell.Style.BackColor = Color.FromArgb(150, 50, 50);
                        cell.Style.ForeColor = Color.FromArgb(255, 200, 200);
                        cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
            }
            else
            {
                // Try to compare build numbers
                if (long.TryParse(localBuild, out long local) && long.TryParse(rinBuild, out long rin))
                {
                    if (local > rin)
                    {
                        // WE HAVE NEWER VERSION - SHARE IT!
                        row.Cells["RINStatus"].Value = "💎 NEWER!";
                        row.Cells["RINStatus"].Style.ForeColor = Color.FromArgb(100, 255, 100);
                        row.Cells["RINStatus"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);

                        // Green glow for newer builds
                        row.DefaultCellStyle.BackColor = Color.FromArgb(0, 40, 20);

                        // Highlight Clean Share button
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell.OwningColumn.Name == "CleanShare")
                            {
                                cell.Style.BackColor = Color.FromArgb(50, 150, 50);
                                cell.Style.ForeColor = Color.FromArgb(200, 255, 200);
                                cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                            }
                        }
                    }
                    else if (local == rin)
                    {
                        row.Cells["RINStatus"].Value = "✅ Current";
                        row.Cells["RINStatus"].Style.ForeColor = Color.FromArgb(100, 200, 100);
                    }
                    else
                    {
                        row.Cells["RINStatus"].Value = "📦 Old";
                        row.Cells["RINStatus"].Style.ForeColor = Color.Gray;
                    }
                }
                else if (!string.IsNullOrEmpty(rinBuild))
                {
                    // RIN has a build but we can't compare numbers
                    row.Cells["RINStatus"].Value = "❓ Check";
                    row.Cells["RINStatus"].Style.ForeColor = Color.Yellow;
                }
                else
                {
                    // RIN has clean files but no build number found
                    row.Cells["RINStatus"].Value = "📋 Has Clean";
                    row.Cells["RINStatus"].Style.ForeColor = Color.FromArgb(150, 150, 255);
                }
            }
        }

        private void resinstruccZip_Click(object sender, EventArgs e)
        {

        }
    }
}