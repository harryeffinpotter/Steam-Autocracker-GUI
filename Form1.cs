using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CliWrap;
using CliWrap.Buffered;
using SteamAppIdIdentifier;
using Clipboard = System.Windows.Forms.Clipboard;
using DataFormats = System.Windows.Forms.DataFormats;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using Timer = System.Windows.Forms.Timer;

namespace APPID
{
    public partial class SteamAppId : Form
    {
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
        }
        public static void Tit(string Message, Color color)
        {
            // Handle cross-thread calls
            if (Program.form.InvokeRequired)
            {
                Program.form.Invoke(new Action(() => Tit(Message, color)));
                return;
            }

            string MessageLow = Message.ToLower();
            if (MessageLow.Contains("READY TO CRACK!".ToLower()))
            {
                Program.form.StatusLabel.ForeColor = Color.HotPink;
            }
            else if (MessageLow.Contains("Complete".ToLower()))
            {
                Program.form.StatusLabel.ForeColor = Color.MediumSpringGreen;
            }
            else if (MessageLow.Contains("no steam".ToLower()))
            {
                Program.form.StatusLabel.ForeColor = Color.Crimson;
            }
            else
            {
                Program.form.StatusLabel.ForeColor = color;
            }
            Program.form.StatusLabel.Text = Message;
            Program.form.Text = Message.Replace("&&", "&");
        }
        public static void Tat(string Message)
        {
            // Handle cross-thread calls
            if (Program.form.InvokeRequired)
            {
                Program.form.Invoke(new Action(() => Tat(Message)));
                return;
            }
            Program.form.currDIrText.Text = $"{Message}";
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
            if (Properties.Settings.Default.AutoCrack)
            {
                autoCrackEnabled = true;
                autoCrackOn.BringToFront();
            }
            else
            {
                autoCrackEnabled = false;
                autoCrackOff.BringToFront();
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
                foreach (string args1 in Program.args2)
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
        async Task PutTaskDelay()
        {
            await Task.Delay(1000);
        }
        bool textChanged = false;
        bool isManualSearch = false;  // Track if user is manually typing vs folder drop
        bool isFirstClickAfterSelection = false;  // Track first click after folder/file selection
        bool isInitialFolderSearch = false;  // Track if this is the FIRST search after folder selection

        private async void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            // If user is typing (searchbox has focus), mark as manual search
            if (searchTextBox.Focused)
            {
                isManualSearch = true;
            }

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
                }

                // Clear the initial folder search flag - any further typing is manual
                isInitialFolderSearch = false;

                // Fallback: If we still have no game selected but we do have search results, ensure buttons are visible
                if (string.IsNullOrEmpty(APPID) && dataGridView1.Rows.Count > 1)
                {
                    btnManualEntry.Visible = true;
                    resinstruccZip.Visible = true;
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

        private async Task<bool> CrackCoreAsync()  // Core cracking logic moved to separate method
        {
            int execount = -20;
            int steam64count = -1;
            int steamcount = -1;
            string parentdir = "";

            bool cracked = false;
            bool steamlessUnpacked = false;  // Track if Steamless unpacked anything
            try
            {
                var files = Directory.GetFiles(gameDir, "*.*", SearchOption.AllDirectories);


                foreach (string file in files)
                {
                    if (file.EndsWith("steam_api64.dll"))
                    {
                        
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
                        cracked = true;
                        File.Move(file, $"{file}.bak");
                        if (goldy)
                        {
                            Directory.CreateDirectory(steam);
                            File.Copy($"{Environment.CurrentDirectory}\\_bin\\Goldberg\\steam_api64.dll", file);
                        }
                        else
                        {
                            File.Copy($"{Environment.CurrentDirectory}\\_bin\\ALI213\\steam_api64.dll", file);
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
                        cracked = true;
                        File.Move(file, $"{file}.bak");

                        if (goldy)
                        {
                            Directory.CreateDirectory(steam);
                            File.Copy($"{Environment.CurrentDirectory}\\_bin\\Goldberg\\steam_api.dll", file);
                        }
                        else
                        {
                            File.Copy($"{Environment.CurrentDirectory}\\_bin\\ALI213\\steam_api.dll", file);
                            if (File.Exists(parentdir + "\\SteamConfig.ini"))
                            {
                                File.Delete(parentdir + "\\SteamConfig.ini");
                            }
                            IniFileEdit($"\".\\_bin\\ALI213\\SteamConfig.ini\" [Settings] \"AppID = {APPID}\"");
                            File.Copy("_bin\\ALI213\\SteamConfig.ini", $"{parentdir}\\SteamConfig.ini");
                        }

                    }
                    if (Path.GetExtension(file) == ".exe")
                    {
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
                        pro.Arguments = $"/c start /B \"\" \"{Environment.CurrentDirectory}\\_bin\\Steamless\\Steamless.CLI.exe\" \"{file}\"";
                        x2.StartInfo = pro;
                        x2.Start();
                        string Output = x2.StandardError.ReadToEnd() + x2.StandardOutput.ReadToEnd();
                        x2.WaitForExit();
                        if (File.Exists($"{file}.unpacked.exe"))
                        {
                            Tit($"Unpacked {file} successfully!", Color.LightSkyBlue);
                            File.Move(file, file + ".bak");
                            File.Move($"{file}.unpacked.exe", file);
                            steamlessUnpacked = true;  // Mark that we unpacked something
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
            catch (Exception ex){ File.WriteAllText("_CRASHlog.txt", ex.StackTrace + "\n" + ex.Message); }

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
                if (folderSelectDialog.FileName.Contains("Program Files"))
                {
                    string warned = $"{Environment.CurrentDirectory}\\_bin\\warn";
                    if (!File.Exists(warned))
                    {
                        DialogResult response = MessageBox.Show("It looks like you selected a Program Files directory, " +
                      "this will often cause the autocrack to fail!!!\n\nIf you understand and DO NOT WANT TO SEE THIS WARNING AGAIN - SELECT YES!\nSelect NO to continue crack but keep warning enabled\nSelect CANCEL to cancel selection and try again!!", "PROGRAM FILES = BAD", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                        if (DialogResult == DialogResult.Yes)
                        {
                            File.WriteAllText(warned, "WARNED");

                        }
                        else if (DialogResult == DialogResult.Cancel)
                        {
                            OpenDir.Visible = true;
                            OpenDir.BringToFront();
                            return;
                        }
                    }
                 
                }
                gameDir = folderSelectDialog.FileName;
                OpenDir.Visible = true;
                parentOfSelection = Directory.GetParent(gameDir).FullName;
                gameDirName = Path.GetFileName(gameDir);
                btnManualEntry.Visible = true;
                resinstruccZip.Visible = true;
                mainPanel.Visible = false;  // Hide mainPanel so dataGridView is visible

                startCrackPic.Visible = true;
                Tit("Please select the correct game from the list!! (if list empty do manual search!)", Color.LightSkyBlue);

                isManualSearch = false;  // This is from folder selection, not manual typing
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

                bool crackedSuccessfully = await CrackAsync();
                cracking = false;

                // Check if we actually cracked anything
                if (crackedSuccessfully)
                {
                    Tit("Crack complete!", Color.LightSkyBlue);
                }
                else
                {
                    Tit("No files to crack found!", Color.Red);
                }

                OpenDir.BringToFront();
                OpenDir.Visible = true;
                startCrackPic.Visible = false;
                donePic.Visible = true;
                    donePic.Visible = false;
                await Task.Delay(3000);


                Tit("Click folder & select game's parent directory.", Color.Cyan);
            }
        }
        public bool goldy = false;
        public bool enableLanMultiplayer = false;
        public bool autoCrackEnabled = false;

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
                    OpenDir.Visible = true;
                    parentOfSelection = Directory.GetParent(gameDir).FullName;
                    gameDirName = Path.GetFileName(gameDir);
                    mainPanel.Visible = false;  // Hide mainPanel so dataGridView is visible
                    btnManualEntry.Visible = true;
                    resinstruccZip.Visible = true;  // Show this too!
                    startCrackPic.Visible = true;
                    Tit("Please select the correct game from the list!! (if list empty do manual search!)", Color.LightSkyBlue);
                    isManualSearch = false;  // This is from folder selection, not manual typing
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
            Program.CmdKILL("APPID");
        }

        private void OpenDir_Click(object sender, EventArgs e)
        {
            Process.Start(gameDir);
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
                ManAppBox.Clear();
                ManAppPanel.Visible = false;
                searchTextBox.Focus();
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
            isManualSearch = true;  // User clicked in search box - manual mode
        }
    }
}