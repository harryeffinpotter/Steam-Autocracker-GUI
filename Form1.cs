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
using Newtonsoft.Json.Linq;
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
        // Exe directory - use this for all paths
        public static readonly string ExeDir = Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory;
        public static readonly string BinPath = Path.Combine(ExeDir, "_bin");

        // Event for crack status updates
        public event EventHandler<string> CrackStatusChanged;

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

        #region Crack Details Tracking
        /// <summary>
        /// Tracks details about what was cracked during a crack operation
        /// </summary>
        public class CrackDetails
        {
            public string GameName { get; set; }
            public string GamePath { get; set; }
            public string AppId { get; set; }
            public List<string> DllsBackedUp { get; } = new List<string>();
            public List<string> DllsReplaced { get; } = new List<string>();
            public List<string> ExesUnpacked { get; } = new List<string>();
            public List<string> ExesSkipped { get; } = new List<string>();
            public List<string> Errors { get; } = new List<string>();
            public bool Success { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.Now;

            // Zip/Upload tracking
            public bool ZipAttempted { get; set; }
            public bool ZipSuccess { get; set; }
            public string ZipError { get; set; }
            public string ZipPath { get; set; }
            public bool UploadAttempted { get; set; }
            public bool UploadSuccess { get; set; }
            public string UploadError { get; set; }
            public string UploadUrl { get; set; }
            public int UploadRetryCount { get; set; }

            public bool HasAnyChanges => DllsReplaced.Count > 0 || ExesUnpacked.Count > 0;

            public string GetSummary()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"=== Crack Details for {GameName} ===");
                sb.AppendLine($"Path: {GamePath}");
                sb.AppendLine($"AppID: {AppId}");
                sb.AppendLine($"Time: {Timestamp:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Success: {Success}");
                sb.AppendLine();

                if (DllsBackedUp.Count > 0)
                {
                    sb.AppendLine($"DLLs Backed Up ({DllsBackedUp.Count}):");
                    foreach (var dll in DllsBackedUp)
                        sb.AppendLine($"  - {dll}");
                    sb.AppendLine();
                }

                if (DllsReplaced.Count > 0)
                {
                    sb.AppendLine($"DLLs Replaced ({DllsReplaced.Count}):");
                    foreach (var dll in DllsReplaced)
                        sb.AppendLine($"  - {dll}");
                    sb.AppendLine();
                }

                if (ExesUnpacked.Count > 0)
                {
                    sb.AppendLine($"EXEs Unpacked by Steamless ({ExesUnpacked.Count}):");
                    foreach (var exe in ExesUnpacked)
                        sb.AppendLine($"  - {exe}");
                    sb.AppendLine();
                }

                if (ExesSkipped.Count > 0)
                {
                    sb.AppendLine($"EXEs Skipped (Non-game utilities) ({ExesSkipped.Count}):");
                    foreach (var exe in ExesSkipped)
                        sb.AppendLine($"  - {exe}");
                    sb.AppendLine();
                }

                if (Errors.Count > 0)
                {
                    sb.AppendLine($"Errors ({Errors.Count}):");
                    foreach (var err in Errors)
                        sb.AppendLine($"  - {err}");
                    sb.AppendLine();
                }

                // Zip status
                if (ZipAttempted)
                {
                    sb.AppendLine($"Zip: {(ZipSuccess ? "Success" : "Failed")}");
                    if (!string.IsNullOrEmpty(ZipPath))
                        sb.AppendLine($"  Path: {ZipPath}");
                    if (!string.IsNullOrEmpty(ZipError))
                        sb.AppendLine($"  Error: {ZipError}");
                    sb.AppendLine();
                }

                // Upload status
                if (UploadAttempted)
                {
                    sb.AppendLine($"Upload: {(UploadSuccess ? "Success" : "Failed")}");
                    if (UploadRetryCount > 0)
                        sb.AppendLine($"  Retries: {UploadRetryCount}");
                    if (!string.IsNullOrEmpty(UploadUrl))
                        sb.AppendLine($"  URL: {UploadUrl}");
                    if (!string.IsNullOrEmpty(UploadError))
                        sb.AppendLine($"  Error: {UploadError}");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Current crack details being populated during a crack operation
        /// </summary>
        public CrackDetails CurrentCrackDetails { get; private set; }

        /// <summary>
        /// History of crack operations for the current session
        /// </summary>
        public List<CrackDetails> CrackHistory { get; } = new List<CrackDetails>();

        /// <summary>
        /// Patterns for EXE files that should be skipped during Steamless unpacking.
        /// These are utility/installer EXEs, not actual game executables.
        /// </summary>
        private static readonly string[] SkipExePatterns = new string[]
        {
            // Crash handlers
            @"(?i)crash.*report",
            @"(?i)crash.*handler",
            @"(?i)crash.*pad",
            @"(?i)unity.*crash",
            @"(?i)CrashReportClient",
            @"(?i)UnityCrashHandler",

            // Redistributables and installers
            @"(?i)vc_?redist",
            @"(?i)vcredist",
            @"(?i)dxsetup",
            @"(?i)dxwebsetup",
            @"(?i)directx",
            @"(?i)dotnet",
            @"(?i)\.net.*install",
            @"(?i)physx",
            @"(?i)openal",
            @"(?i)oalinst",
            @"(?i)UE.*Prereq",
            @"(?i)UEPrereq",

            // Uninstallers
            @"(?i)unins\d*",
            @"(?i)uninstall",
            @"(?i)uninst",

            // Development/Editor tools
            @"(?i)sdk",
            @"(?i)editor",
            @"(?i)modkit",

            // Launchers and helpers
            @"(?i)_?lobby_connect",
            @"(?i)webhelper",
            @"(?i)cefsubprocess",
            @"(?i)browsersubprocess",
            @"(?i)steamwebhelper",

            // Easy Anti-Cheat and anti-cheat
            @"(?i)easyanticheat",
            @"(?i)eac_",
            @"(?i)battleye",
            @"(?i)beclient",
            @"(?i)beservice",

            // Updaters and launchers
            @"(?i)updater",
            @"(?i)patcher",
            @"(?i)launcher.*helper",

            // Specific known utilities
            @"(?i)cleanup",
            @"(?i)^7z",
            @"(?i)^rar",
            @"(?i)^setup",
            @"(?i)install",
        };

        /// <summary>
        /// Folder names that typically contain non-game executables
        /// </summary>
        private static readonly string[] SkipFolderNames = new string[]
        {
            "_CommonRedist",
            "CommonRedist",
            "Redist",
            "Redistributables",
            "__Installer",
            "_Installer",
            "DirectX",
            "VCRedist",
            "DotNetFX",
            ".NET",
            "Support",
            "Prerequisites",
            "Prereqs",
        };

        /// <summary>
        /// Checks if an EXE file should be skipped during Steamless unpacking
        /// </summary>
        private static bool ShouldSkipExe(string exePath)
        {
            string fileName = Path.GetFileName(exePath);
            string directory = Path.GetDirectoryName(exePath) ?? "";

            // Check if the file is in a skip folder
            foreach (var skipFolder in SkipFolderNames)
            {
                if (directory.IndexOf(skipFolder, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            // Check against skip patterns
            foreach (var pattern in SkipExePatterns)
            {
                try
                {
                    if (Regex.IsMatch(fileName, pattern, RegexOptions.IgnoreCase))
                        return true;
                }
                catch { }
            }

            return false;
        }
        #endregion

        protected DataTableGeneration dataTableGeneration;
        public static int CurrentCell = 0;
        public static string APPNAME = "";
        public static string APPDATA = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public SteamAppId()
        {
            ServicePointManager.ServerCertificateValidationCallback =
            (s, certificate, chain, sslPolicyErrors) => true;

            // Load auto-crack setting FIRST before anything else - directly assign the value
            autoCrackEnabled = AppSettings.Default.AutoCrack;
            System.Diagnostics.Debug.WriteLine($"[CONSTRUCTOR] Loaded autoCrackEnabled = {autoCrackEnabled} from Settings");

            dataTableGeneration = new DataTableGeneration();
            Task.Run(async () => await dataTableGeneration.GetDataTableAsync(dataTableGeneration)).Wait();
            InitializeComponent();

            // Set auto-crack UI immediately after InitializeComponent so it's ready BEFORE Form_Load
            if (autoCrackEnabled)
            {
                autoCrackOn.BringToFront();
            }
            else
            {
                autoCrackOff.BringToFront();
            }

            // Apply saved pin state
            if (AppSettings.Default.Pinned)
            {
                this.TopMost = true;
                unPin.BringToFront();
            }
            else
            {
                this.TopMost = false;
                pin.BringToFront();
            }

            InitializeTimers();

            // Make form background draggable (click empty space to drag)
            this.MouseDown += TitleBar_MouseDown;

            // Make main panel draggable too if it exists
            if (mainPanel != null)
            {
                mainPanel.MouseDown += TitleBar_MouseDown;
            }

            // Apply acrylic blur and rounded corners
            this.Load += (s, e) => ApplyAcrylicEffect();

            // Apply rounded corners to buttons
            ApplyRoundedCornersToButton(OpenDir);
            ApplyRoundedCornersToButton(ZipToShare);
            ApplyRoundedCornersToButton(btnManualEntry);
        }

        private void ApplyRoundedCornersToButton(Button btn)
        {
            // Make button background transparent so we can draw custom
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            // Enable transparency support
            typeof(Button).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, btn, new object[] { true });

            // Disable focus cues to prevent orange highlighting
            typeof(Button).InvokeMember("SetStyle",
                System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, btn, new object[] { System.Windows.Forms.ControlStyles.Selectable, false });

            // Paint event for rounded corners
            btn.Paint += (sender, e) =>
            {
                Button b = sender as Button;

                // DISABLED - This causes deadlocks when updating buttons from background threads
                // Draw the parent's background in the button area first for true transparency
                /*if (b.Parent != null)
                {
                    using (var bmp = new Bitmap(b.Parent.Width, b.Parent.Height))
                    {
                        b.Parent.DrawToBitmap(bmp, new Rectangle(0, 0, b.Parent.Width, b.Parent.Height));
                        e.Graphics.DrawImage(bmp, new Rectangle(0, 0, b.Width, b.Height),
                            new Rectangle(b.Left, b.Top, b.Width, b.Height), GraphicsUnit.Pixel);
                    }
                }*/

                // Enable maximum quality anti-aliasing
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Create rounded rectangle that fits perfectly - subtle modern look
                int radius = 8;
                Rectangle rect = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

                int diameter = radius * 2;
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                // Determine colors based on state
                Color bgColor = Color.FromArgb(38, 38, 42);
                if (b.ClientRectangle.Contains(b.PointToClient(Cursor.Position)))
                {
                    bgColor = Color.FromArgb(50, 50, 55); // Lighter on hover
                }

                // Draw background
                using (var brush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Draw border
                using (var pen = new Pen(Color.FromArgb(55, 55, 60), 1.5f))
                {
                    e.Graphics.DrawPath(pen, path);
                }

                // Draw text
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle,
                    Color.FromArgb(220, 220, 225),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            // Refresh on mouse enter/leave for hover effect
            btn.MouseEnter += (s, e) => btn.Invalidate();
            btn.MouseLeave += (s, e) => btn.Invalidate();
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
                accent.AccentFlags = ThemeConfig.BlurIntensity;
                // Get acrylic color from ThemeConfig.cs - EDIT THAT FILE TO CHANGE COLORS!
                accent.GradientColor = ThemeConfig.AcrylicBlurColor;

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
                // Don't drag if clicking on DataGridView (allows column resizing)
                var clickedControl = this.GetChildAtPoint(this.PointToClient(Cursor.Position));
                if (clickedControl is DataGridView)
                    return;

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

        // User tracking removed - anonymous sharing now

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
            // Skip if suppressing status updates (cracking from share window)
            if (SteamAppIdIdentifier.Program.form.suppressStatusUpdates)
                return;

            // Handle cross-thread calls
            if (SteamAppIdIdentifier.Program.form.InvokeRequired)
            {
                SteamAppIdIdentifier.Program.form.BeginInvoke(new Action(() => Tit(Message, color)));
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
            // Skip if suppressing status updates (cracking from share window)
            if (SteamAppIdIdentifier.Program.form.suppressStatusUpdates)
                return;

            // Handle cross-thread calls
            if (SteamAppIdIdentifier.Program.form.InvokeRequired)
            {
                SteamAppIdIdentifier.Program.form.BeginInvoke(new Action(() => Tat(Message)));
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
                CurrentAppId = dataGridView1[1, rowIndex].Value.ToString();
                APPNAME = dataGridView1[0, rowIndex].Value.ToString().Trim();
                Tat($"{APPNAME} ({CurrentAppId})");
                searchTextBox.Clear();
                btnManualEntry.Visible = false;
                mainPanel.Visible = true;
                startCrackPic.Visible = true;
                resinstruccZip.Visible = false;

                // Auto-crack if enabled
                System.Diagnostics.Debug.WriteLine($"[AUTO-CRACK CHECK] autoCrackEnabled={autoCrackEnabled}, gameDir='{gameDir ?? "NULL"}'");
                if (autoCrackEnabled && !string.IsNullOrEmpty(gameDir))
                {
                    System.Diagnostics.Debug.WriteLine("[AUTO-CRACK] Triggering auto-crack NOW!");
                    // Auto-cracking - show different message
                    Tit("Auto-cracking...", Color.Lime);
                    // Trigger crack just like clicking the button
                    startCrackPic_Click(null, null);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[AUTO-CRACK] NOT triggering - showing manual message");
                    // Manual mode - show ready message
                    Tit("READY! Click skull folder above to perform crack!", Color.HotPink);
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
            // Load LAN multiplayer setting
            enableLanMultiplayer = AppSettings.Default.LANMultiplayer;
            lanMultiplayerCheckBox.Checked = enableLanMultiplayer;

            if (AppSettings.Default.Pinned)
            {
                this.TopMost = true;
                unPin.BringToFront();
            }
            else
            {
                this.TopMost = false;
                pin.BringToFront();
            }

            // Update UI for auto-crack setting (value already loaded in constructor)
            // When enabled, show the green ON button. When disabled, show the red OFF button.
            System.Diagnostics.Debug.WriteLine($"[FORM_LOAD] autoCrackEnabled = {autoCrackEnabled}, bringing {(autoCrackEnabled ? "autoCrackOn" : "autoCrackOff")} to front");
            if (autoCrackEnabled)
            {
                autoCrackOn.BringToFront();  // Show green button when enabled
            }
            else
            {
                autoCrackOff.BringToFront();  // Show red button when disabled
            }

            if (AppSettings.Default.Goldy)
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
        public static String CurrentAppId;
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
                CurrentAppId = dataGridView1[1, e.RowIndex].Value.ToString();
                APPNAME = dataGridView1[0, e.RowIndex].Value.ToString().Trim();
                Tat($"{APPNAME} ({CurrentAppId})");
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
                    CurrentAppId = dataGridView1[1, CurrentCell].Value.ToString();
                    APPNAME = dataGridView1[0, CurrentCell].Value.ToString().Trim();
                    Tat($"{APPNAME} ({CurrentAppId})");
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

                // Check if DataSource is ready
                if (dataGridView1.DataSource == null) return;

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
                if (string.IsNullOrEmpty(CurrentAppId) && dataGridView1.Rows.Count > 1)
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
            CurrentAppId = dataGridView1[1, CurrentCell].Value.ToString();
            APPNAME = dataGridView1[0, CurrentCell].Value.ToString().Trim();
            Tat($"{APPNAME} ({CurrentAppId})");
            searchTextBox.Clear();
            mainPanel.Visible = true;
            btnManualEntry.Visible = false;
            startCrackPic.Visible = true;
            resinstruccZip.Visible = false;

            // Auto-crack if enabled
            if (autoCrackEnabled && !string.IsNullOrEmpty(gameDir))
            {
                Tit("Autocracking...", Color.Yellow);
                // Trigger crack just like clicking the button
                startCrackPic_Click(null, null);
            }
            else
            {
                Tit("READY! Click skull folder above to perform crack!", Color.LightSkyBlue);
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
            System.Diagnostics.Debug.WriteLine("[CRACK] CrackAsync starting");
            // Use TaskScheduler.Default to avoid capturing UI synchronization context
            var result = await Task.Factory.StartNew(
                async () => await CrackCoreAsync(),
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            ).Unwrap();
            System.Diagnostics.Debug.WriteLine($"[CRACK] CrackAsync completed, result: {result}");
            return result;
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

        /// <summary>
        /// Compares two files byte-by-byte to see if they are identical
        /// </summary>
        private bool AreFilesIdentical(string file1, string file2)
        {
            try
            {
                if (!File.Exists(file1) || !File.Exists(file2))
                    return false;

                var fileInfo1 = new FileInfo(file1);
                var fileInfo2 = new FileInfo(file2);

                // Quick check: if sizes differ, files are different
                if (fileInfo1.Length != fileInfo2.Length)
                    return false;

                // Byte-by-byte comparison
                using (var fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read))
                using (var fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer1 = new byte[4096];
                    byte[] buffer2 = new byte[4096];
                    int read1, read2;

                    while ((read1 = fs1.Read(buffer1, 0, buffer1.Length)) > 0)
                    {
                        read2 = fs2.Read(buffer2, 0, buffer2.Length);
                        if (read1 != read2)
                            return false;

                        for (int i = 0; i < read1; i++)
                        {
                            if (buffer1[i] != buffer2[i])
                                return false;
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Restores all .bak files in a directory (recursive)
        /// </summary>
        private void RestoreAllBakFiles(string directory)
        {
            try
            {
                var bakFiles = Directory.GetFiles(directory, "*.bak", SearchOption.AllDirectories);
                foreach (var bakFile in bakFiles)
                {
                    var originalFile = bakFile.Substring(0, bakFile.Length - 4); // Remove .bak
                    try
                    {
                        if (File.Exists(originalFile))
                            File.Delete(originalFile);
                        File.Move(bakFile, originalFile);
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Restored {Path.GetFileName(bakFile)} -> {Path.GetFileName(originalFile)}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Failed to restore {bakFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CRACK] Error in RestoreAllBakFiles: {ex.Message}");
            }
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

            // Initialize crack details tracking
            CurrentCrackDetails = new CrackDetails
            {
                GameName = gameDirName ?? Path.GetFileName(gameDir),
                GamePath = gameDir,
                AppId = CurrentAppId
            };

            System.Diagnostics.Debug.WriteLine($"[CRACK] === Starting CrackCoreAsync ===");
            System.Diagnostics.Debug.WriteLine($"[CRACK] Game Directory: {gameDir}");
            System.Diagnostics.Debug.WriteLine($"[CRACK] AppID: {CurrentAppId}");
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

                        // Restore .bak if it exists (get clean file first)
                        if (File.Exists($"{file}.bak"))
                        {
                            System.Diagnostics.Debug.WriteLine($"[CRACK] Found existing .bak, restoring clean file first");
                            File.Delete(file);
                            File.Move($"{file}.bak", file);
                        }

                        Tit("Replacing steam_api64.dll.", Color.LightSkyBlue);
                        string emulatorName = goldy ? "Goldberg" : "ALI213";
                        CrackStatusChanged?.Invoke(this, $"Applying {emulatorName} emulator...");

                        try
                        {
                            string sourceEmulatorDll = goldy
                                ? $"{BinPath}\\Goldberg\\steam_api64.dll"
                                : $"{BinPath}\\ALI213\\steam_api64.dll";

                            // Check if current file is already the emulator DLL
                            if (AreFilesIdentical(file, sourceEmulatorDll))
                            {
                                System.Diagnostics.Debug.WriteLine($"[CRACK] steam_api64.dll is already {emulatorName}, skipping replacement to preserve clean .bak");
                                Tit($"steam_api64.dll already {emulatorName}, skipped", Color.Yellow);
                                cracked = true;
                            }
                            else
                            {
                                // Create backup and replace with emulator
                                File.Move(file, $"{file}.bak");
                                CurrentCrackDetails.DllsBackedUp.Add(file);
                                if (goldy)
                                {
                                    Directory.CreateDirectory(steam);
                                }
                                File.Copy(sourceEmulatorDll, file);
                                CurrentCrackDetails.DllsReplaced.Add($"{file} ({emulatorName})");
                                cracked = true;
                            }
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
                            IniFileEdit($"{BinPath}\\ALI213\\SteamConfig.ini [Settings] \"AppID = {CurrentAppId}\"");
                            File.Copy($"{BinPath}\\ALI213\\SteamConfig.ini", $"{parentdir}\\SteamConfig.ini");
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

                        // Restore .bak if it exists (get clean file first)
                        if (File.Exists($"{file}.bak"))
                        {
                            System.Diagnostics.Debug.WriteLine($"[CRACK] Found existing .bak, restoring clean file first");
                            File.Delete(file);
                            File.Move($"{file}.bak", file);
                        }

                        Tit("Replacing steam_api.dll.", Color.LightSkyBlue);
                        parentdir = Directory.GetParent(file).FullName;

                        try
                        {
                            string sourceEmulatorDll = goldy
                                ? $"{BinPath}\\Goldberg\\steam_api.dll"
                                : $"{BinPath}\\ALI213\\steam_api.dll";

                            // Check if current file is already the emulator DLL
                            string emulatorName = goldy ? "Goldberg" : "ALI213";
                            if (AreFilesIdentical(file, sourceEmulatorDll))
                            {
                                System.Diagnostics.Debug.WriteLine($"[CRACK] steam_api.dll is already {emulatorName}, skipping replacement to preserve clean .bak");
                                Tit($"steam_api.dll already {emulatorName}, skipped", Color.Yellow);
                                cracked = true;
                            }
                            else
                            {
                                // Create backup and replace with emulator
                                File.Move(file, $"{file}.bak");
                                CurrentCrackDetails.DllsBackedUp.Add(file);
                                if (goldy)
                                {
                                    Directory.CreateDirectory(steam);
                                }
                                File.Copy(sourceEmulatorDll, file);
                                CurrentCrackDetails.DllsReplaced.Add($"{file} ({emulatorName})");
                                cracked = true;
                            }
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
                                IniFileEdit($"\".\\_bin\\ALI213\\SteamConfig.ini\" [Settings] \"AppID = {CurrentAppId}\"");
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
                        // Skip if file path is empty or file doesn't exist
                        if (string.IsNullOrEmpty(file) || !File.Exists(file))
                        {
                            System.Diagnostics.Debug.WriteLine($"[CRACK] Invalid or missing file, skipping: {file}");
                            continue;
                        }

                        // Check if this EXE should be skipped (non-game utility executables)
                        if (ShouldSkipExe(file))
                        {
                            System.Diagnostics.Debug.WriteLine($"[CRACK] Skipping non-game EXE: {file}");
                            CurrentCrackDetails.ExesSkipped.Add(Path.GetFileName(file));
                            continue;
                        }

                        // Check if Steamless CLI exists before trying to use it (CLI version runs without GUI)
                        string steamlessPath = $"{BinPath}\\Steamless\\Steamless.CLI.exe";
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Checking for Steamless CLI at: {steamlessPath}");
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Steamless CLI exists: {File.Exists(steamlessPath)}");

                        if (!File.Exists(steamlessPath))
                        {
                            System.Diagnostics.Debug.WriteLine($"[CRACK] Steamless CLI not found, skipping EXE unpacking for: {file}");
                            Tit($"Steamless.CLI.exe not found at {steamlessPath}, skipping EXE unpacking", Color.Yellow);
                            continue;
                        }

                        System.Diagnostics.Debug.WriteLine($"[CRACK] Processing EXE: {file}");
                        CrackStatusChanged?.Invoke(this, $"Attempting to apply Steamless to {Path.GetFileName(file)}...");

                        // Restore .bak if it exists (apply Steamless to clean exe)
                        if (File.Exists($"{file}.bak"))
                        {
                            System.Diagnostics.Debug.WriteLine($"[CRACK] Found existing .bak, restoring clean exe first");
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
                        pro.FileName = steamlessPath;
                        pro.Arguments = $"\"{file}\"";

                        System.Diagnostics.Debug.WriteLine($"[CRACK] Starting Steamless process");
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Working Directory: {parentdir}");
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Command: {pro.Arguments}");

                        x2.StartInfo = pro;
                        x2.Start();

                        // Read output asynchronously to avoid blocking
                        var outputTask = x2.StandardOutput.ReadToEndAsync();
                        var errorTask = x2.StandardError.ReadToEndAsync();

                        await Task.Run(() => x2.WaitForExit());

                        string Output = await errorTask + await outputTask;

                        System.Diagnostics.Debug.WriteLine($"[CRACK] Steamless output: {Output}");
                        System.Diagnostics.Debug.WriteLine($"[CRACK] Steamless exit code: {x2.ExitCode}");

                        if (File.Exists($"{file}.unpacked.exe"))
                        {
                            System.Diagnostics.Debug.WriteLine($"[CRACK] Successfully unpacked: {file}");
                            Tit($"Unpacked {file} successfully!", Color.LightSkyBlue);
                            File.Move(file, file + ".bak");
                            File.Move($"{file}.unpacked.exe", file);
                            steamlessUnpacked = true;  // Mark that we unpacked something
                            CurrentCrackDetails.ExesUnpacked.Add(file);
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

                        // Create steam_appid.txt with just the AppId - this is what gbe_fork expects
                        File.WriteAllText($"{parentdir}\\steam_settings\\steam_appid.txt", CurrentAppId);

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
                            if (File.Exists($"{BinPath}\\lobby_connect_x64.exe"))
                                lobbyConnectSource = $"{BinPath}\\lobby_connect_x64.exe";
                            else if (File.Exists($"{BinPath}\\lobby_connect_x32.exe"))
                                lobbyConnectSource = $"{BinPath}\\lobby_connect_x32.exe";
                            // Fallback to _bin\\Goldberg for backward compatibility
                            else if (File.Exists($"{BinPath}\\Goldberg\\lobby_connect_x64.exe"))
                                lobbyConnectSource = $"{BinPath}\\Goldberg\\lobby_connect_x64.exe";
                            else if (File.Exists($"{BinPath}\\Goldberg\\lobby_connect_x32.exe"))
                                lobbyConnectSource = $"{BinPath}\\Goldberg\\lobby_connect_x32.exe";

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
                                        await Task.Run(() => vbsProcess.WaitForExit());

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
                            $"{BinPath}\\Goldberg\\generate_interfaces_x64.exe",
                            $"{BinPath}\\Goldberg\\generate_interfaces_x32.exe",
                            $"{BinPath}\\generate_interfaces_x64.exe",
                            $"{BinPath}\\generate_interfaces_x32.exe",
                            $"{BinPath}\\Goldberg\\generate_interfaces_file.exe",
                            $"{BinPath}\\generate_interfaces.exe"
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
                                await Task.Run(() => genInterfaces.WaitForExit());

                                // Move generated file to steam_settings if it was created
                                if (File.Exists($"{parentdir}\\steam_interfaces.txt"))
                                {
                                    File.Move($"{parentdir}\\steam_interfaces.txt", $"{parentdir}\\steam_settings\\steam_interfaces.txt");
                                }
                            }
                            catch { }
                        }

                        // Get achievements data using achievements_parser
                        string achievementsParserPath = $"{BinPath}\\achievements_parser.exe";
                        if (File.Exists(achievementsParserPath))
                        {
                            Tit("Getting achievements data...", Color.Cyan);
                            try
                            {
                                var achievementsResult = await Cli.Wrap(achievementsParserPath)
                                    .WithValidation(CommandResultValidation.None)
                                    .WithArguments($"--appid {CurrentAppId}")
                                    .WithWorkingDirectory(Environment.CurrentDirectory)
                                    .ExecuteBufferedAsync();

                                if (!string.IsNullOrWhiteSpace(achievementsResult.StandardOutput))
                                {
                                    // Save achievements to the steam_settings folder
                                    string achievementsPath = $"{parentdir}\\steam_settings\\achievements.ini";
                                    File.WriteAllText(achievementsPath, achievementsResult.StandardOutput);
                                    Tit($"Found and saved achievement data!", Color.Green);
                                }
                                else
                                {
                                    Tit("No achievements found for this game", Color.Yellow);
                                }
                            }
                            catch (Exception ex)
                            {
                                Tit($"Failed to get achievements: {ex.Message}", Color.Yellow);
                                System.Diagnostics.Debug.WriteLine($"[Achievements] Exception: {ex.Message}");
                            }
                        }

                        // Get DLC info from Steam Store API (no API key needed)
                        Tit("Getting DLC info from Steam...", Color.Cyan);
                        try
                        {
                            await FetchDLCInfoAsync(CurrentAppId, $"{parentdir}\\steam_settings");
                        }
                        catch (Exception ex)
                        {
                            Tit($"Failed to get DLC info: {ex.Message}", Color.Yellow);
                            System.Diagnostics.Debug.WriteLine($"[DLC] Exception: {ex.Message}");
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

            // Finalize crack details
            bool success = cracked || steamlessUnpacked;
            CurrentCrackDetails.Success = success;

            // Check if no steam_api DLLs were found
            if (CurrentCrackDetails.DllsReplaced.Count == 0 && CurrentCrackDetails.DllsBackedUp.Count == 0)
            {
                CurrentCrackDetails.Errors.Add("No steam_api.dll or steam_api64.dll found in game folder");
            }

            // Add to history
            CrackHistory.Add(CurrentCrackDetails);

            System.Diagnostics.Debug.WriteLine($"[CRACK] Details - DLLs replaced: {CurrentCrackDetails.DllsReplaced.Count}, EXEs unpacked: {CurrentCrackDetails.ExesUnpacked.Count}, EXEs skipped: {CurrentCrackDetails.ExesSkipped.Count}");

            // Return true only if we actually cracked something
            return success;
        }

        public void IniFileEdit(string args)
        {
            Process IniProcess = new Process();
            IniProcess.StartInfo.CreateNoWindow = true;
            IniProcess.StartInfo.UseShellExecute = false;
            IniProcess.StartInfo.FileName = $"{BinPath}\\ALI213\\inifile.exe";

            IniProcess.StartInfo.Arguments = args;
            IniProcess.Start();
            IniProcess.WaitForExit();
        }

        private async Task FetchDLCInfoAsync(string appId, string outputFolder)
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                try
                {
                    // Get app details from Steam Store API
                    var response = await httpClient.GetStringAsync($"https://store.steampowered.com/api/appdetails?appids={appId}");
                    var json = Newtonsoft.Json.Linq.JObject.Parse(response);

                    var appData = json[appId]?["data"];
                    if (appData == null || json[appId]?["success"]?.Value<bool>() != true)
                    {
                        Tit("No DLC info available for this game", Color.Yellow);
                        return;
                    }

                    var dlcArray = appData["dlc"] as Newtonsoft.Json.Linq.JArray;
                    if (dlcArray == null || dlcArray.Count == 0)
                    {
                        Tit("No DLCs found for this game", Color.Yellow);
                        return;
                    }

                    Tit($"Found {dlcArray.Count} DLCs, fetching names...", Color.Cyan);

                    var dlcLines = new System.Collections.Generic.List<string>();
                    int successCount = 0;

                    foreach (var dlcId in dlcArray)
                    {
                        try
                        {
                            var dlcResponse = await httpClient.GetStringAsync($"https://store.steampowered.com/api/appdetails?appids={dlcId}");
                            var dlcJson = Newtonsoft.Json.Linq.JObject.Parse(dlcResponse);

                            var dlcData = dlcJson[dlcId.ToString()]?["data"];
                            if (dlcData != null && dlcJson[dlcId.ToString()]?["success"]?.Value<bool>() == true)
                            {
                                string dlcName = dlcData["name"]?.Value<string>() ?? "Unknown";
                                dlcLines.Add($"{dlcId}={dlcName}");
                                successCount++;
                            }

                            // Rate limit to avoid Steam throttling
                            await Task.Delay(100);
                        }
                        catch
                        {
                            // Skip DLCs that fail to fetch
                            dlcLines.Add($"{dlcId}=DLC_{dlcId}");
                        }
                    }

                    // Write DLC.txt
                    string dlcPath = System.IO.Path.Combine(outputFolder, "DLC.txt");
                    System.IO.File.WriteAllLines(dlcPath, dlcLines);

                    Tit($"Saved {successCount}/{dlcArray.Count} DLC entries!", Color.Green);
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    throw new Exception("Request timed out");
                }
                catch (System.Net.Http.HttpRequestException ex)
                {
                    throw new Exception($"Network error: {ex.Message}");
                }
            }
        }


        private string parentOfSelection;
        private string gameDir;
        private string gameDirName;
        private bool suppressStatusUpdates = false;  // Flag to suppress Tit/Tat when cracking from share window

        // Public property to allow EnhancedShareWindow to set game directory
        public string GameDirectory
        {
            get { return gameDir; }
            set
            {
                gameDir = value;
                // Also set gameDirName when gameDir is set
                if (!string.IsNullOrEmpty(gameDir))
                {
                    gameDirName = Path.GetFileName(gameDir);
                }
            }
        }

        // Public method to suppress status updates when cracking from share window
        public void SetSuppressStatusUpdates(bool suppress)
        {
            suppressStatusUpdates = suppress;
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            FolderSelectDialog folderSelectDialog = new FolderSelectDialog();
            folderSelectDialog.Title = "Select the game's main folder.";

            if (AppSettings.Default.lastDir.Length > 0)
                folderSelectDialog.InitialDirectory = AppSettings.Default.lastDir;

            if (folderSelectDialog.Show(Handle))
            {
                gameDir = folderSelectDialog.FileName;

                // Always remember parent folder for next time
                try
                {
                    var parent = Directory.GetParent(gameDir);
                    if (parent != null)
                    {
                        AppSettings.Default.lastDir = parent.FullName;
                        AppSettings.Default.Save();
                    }
                }
                catch { }

                // Check if this is a folder containing multiple games (batch mode)
                if (!IsGameFolder(gameDir))
                {
                    var gamesInFolder = DetectGamesInFolder(gameDir);
                    if (gamesInFolder.Count > 0)
                    {
                        // For batch folders, remember the batch folder itself (not parent)
                        AppSettings.Default.lastDir = gameDir;
                        AppSettings.Default.Save();

                        // It's a batch folder! Show selection dialog
                        ShowBatchGameSelection(gamesInFolder);
                        return; // Don't continue with single-game flow
                    }
                }

                // Hide OpenDir and ZipToShare when new directory selected
                OpenDir.Visible = false;
                OpenDir.SendToBack();
                ZipToShare.Visible = false;
                parentOfSelection = Directory.GetParent(gameDir).FullName;
                gameDirName = Path.GetFileName(gameDir);

                // Try to get AppID from Steam manifest files
                var manifestInfo = SteamManifestParser.GetAppIdFromManifest(gameDir);
                if (manifestInfo.HasValue)
                {
                    // We found the AppID from manifest!
                    CurrentAppId = manifestInfo.Value.appId;
                    string manifestGameName = manifestInfo.Value.gameName;
                    long sizeOnDisk = manifestInfo.Value.sizeOnDisk;

                    System.Diagnostics.Debug.WriteLine($"[MANIFEST] Auto-detected from Steam manifest:");
                    System.Diagnostics.Debug.WriteLine($"[MANIFEST] AppID: {CurrentAppId}");
                    System.Diagnostics.Debug.WriteLine($"[MANIFEST] Game: {manifestGameName}");
                    System.Diagnostics.Debug.WriteLine($"[MANIFEST] Size: {sizeOnDisk / (1024 * 1024)} MB");

                    // Skip the search UI entirely - we already have the AppID!
                    // Just show main panel - it will cover all the search UI elements
                    mainPanel.Visible = true;
                    resinstruccZip.Visible = true;
                    resinstruccZipTimer.Stop();
                    resinstruccZipTimer.Start();
                    startCrackPic.Visible = true;

                    // Skip the search entirely
                    isFirstClickAfterSelection = false;
                    isInitialFolderSearch = false;

                    // Auto-crack if enabled
                    if (autoCrackEnabled && !string.IsNullOrEmpty(gameDir))
                    {
                        System.Diagnostics.Debug.WriteLine("[MANIFEST] Auto-crack enabled, starting crack...");
                        Tit($"✅ Auto-detected: {manifestGameName} (AppID: {CurrentAppId}) - Auto-cracking...", Color.Yellow);
                        // Trigger crack just like clicking the button
                        startCrackPic_Click(null, null);
                    }
                    else
                    {
                        // Update the title with game info
                        Tit($"✅ Auto-detected: {manifestGameName} (AppID: {CurrentAppId}) - Ready to crack!", Color.LightGreen);
                    }
                }
                else
                {
                    // No manifest found, proceed with normal search flow
                    btnManualEntry.Visible = true;
                    resinstruccZip.Visible = true;
                    resinstruccZipTimer.Stop();
                    resinstruccZipTimer.Start();  // Start 30 second timer
                    mainPanel.Visible = false;  // Hide mainPanel so dataGridView is visible

                    startCrackPic.Visible = true;
                    Tit("Please select the correct game from the list!! (if list empty do manual search!)", Color.LightSkyBlue);

                    // Trigger the search
                    isFirstClickAfterSelection = true;  // Set before changing text
                    isInitialFolderSearch = true;  // This is the initial search from folder
                    searchTextBox.Text = gameDirName;
                }

                // Stop label5 timer when game dir is selected
                label5Timer.Stop();
                label5.Visible = false;
                AppSettings.Default.lastDir = parentOfSelection;
                AppSettings.Default.Save();
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
            AppSettings.Default.LANMultiplayer = enableLanMultiplayer;
            AppSettings.Default.Save();
        }
        private void dllSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dllSelect.SelectedIndex == 0)
            {
                goldy = true;
                AppSettings.Default.Goldy = true;
                // Show LAN checkbox for Goldberg
                lanMultiplayerCheckBox.Visible = true;
            }
            else if (dllSelect.SelectedIndex == 1)
            {
                goldy = false;
                AppSettings.Default.Goldy = false;
                // Hide LAN checkbox for Ali213 (not supported)
                lanMultiplayerCheckBox.Visible = false;
                lanMultiplayerCheckBox.Checked = false;
                enableLanMultiplayer = false;
            }
            AppSettings.Default.Save();
        }
        private void selectDir_MouseEnter(object sender, EventArgs e)
        {
            selectDir.Image = Properties.Resources.hoveradd;
        }
        private void selectDir_MouseHover(object sender, EventArgs e)
        {
            selectDir.Image = Properties.Resources.hoveradd;
        }

        private void selectDir_MouseDown(object sender, MouseEventArgs e)
        {
            selectDir.Image = Properties.Resources.clickadd;
        }

        private void startCrackPic_MouseDown(object sender, MouseEventArgs e)
        {
            startCrackPic.Image = Properties.Resources.clickr2c;
        }

        private void startCrackPic_MouseEnter(object sender, EventArgs e)
        {
            startCrackPic.Image = Properties.Resources.hoverr2c;
        }

        private void startCrackPic_MouseHover(object sender, EventArgs e)
        {
            startCrackPic.Image = Properties.Resources.hoverr2c;
        }

        private void selectDir_MouseLeave(object sender, EventArgs e)
        {
            selectDir.Image = Properties.Resources.add;
        }

        private void startCrackPic_MouseLeave(object sender, EventArgs e)
        {
            startCrackPic.Image = Properties.Resources.r2c;
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

                    // Try to get AppID from Steam manifest files
                    var manifestInfo = SteamManifestParser.GetAppIdFromManifest(gameDir);
                    if (manifestInfo.HasValue)
                    {
                        // We found the AppID from manifest!
                        CurrentAppId = manifestInfo.Value.appId;
                        string manifestGameName = manifestInfo.Value.gameName;
                        long sizeOnDisk = manifestInfo.Value.sizeOnDisk;

                        System.Diagnostics.Debug.WriteLine($"[MANIFEST] Auto-detected from Steam manifest:");
                        System.Diagnostics.Debug.WriteLine($"[MANIFEST] AppID: {CurrentAppId}");
                        System.Diagnostics.Debug.WriteLine($"[MANIFEST] Game: {manifestGameName}");
                        System.Diagnostics.Debug.WriteLine($"[MANIFEST] Size: {sizeOnDisk / (1024 * 1024)} MB");

                        // Skip the search UI entirely - we already have the AppID!
                        // Just show main panel - it will cover all the search UI elements
                        mainPanel.Visible = true;
                        resinstruccZip.Visible = true;
                        resinstruccZipTimer.Stop();
                        resinstruccZipTimer.Start();
                        startCrackPic.Visible = true;

                        // Skip the search entirely
                        isInitialFolderSearch = false;
                        isFirstClickAfterSelection = false;

                        // Auto-crack if enabled
                        if (autoCrackEnabled && !string.IsNullOrEmpty(gameDir))
                        {
                            System.Diagnostics.Debug.WriteLine("[MANIFEST] Auto-crack enabled, starting crack...");
                            Tit($"✅ Auto-detected: {manifestGameName} (AppID: {CurrentAppId}) - Auto-cracking...", Color.Yellow);
                            // Trigger crack just like clicking the button
                            startCrackPic_Click(null, null);
                        }
                        else
                        {
                            // Update the title with game info
                            Tit($"✅ Auto-detected: {manifestGameName} (AppID: {CurrentAppId}) - Ready to crack!", Color.LightGreen);
                        }
                    }
                    else
                    {
                        // No manifest found, proceed with normal search flow
                        mainPanel.Visible = false;  // Hide mainPanel so dataGridView is visible
                        btnManualEntry.Visible = true;
                        resinstruccZip.Visible = true;  // Show this too!
                        resinstruccZipTimer.Stop();
                        resinstruccZipTimer.Start();  // Start 30 second timer
                        startCrackPic.Visible = true;
                        Tit("Please select the correct game from the list!! (if list empty do manual search!)", Color.LightSkyBlue);

                        // Trigger the search
                        isInitialFolderSearch = true;  // This is the initial search
                        searchTextBox.Text = gameDirName;
                        isFirstClickAfterSelection = true;  // Set AFTER changing text to avoid race condition
                    }

                    // Stop label5 timer when game dir is selected
                    label5Timer.Stop();
                    label5.Visible = false;

                    isInitialFolderSearch = true;  // This is the initial search
                    searchTextBox.Text = gameDirName;
                    isFirstClickAfterSelection = true;  // Set AFTER changing text to avoid race condition
                    AppSettings.Default.lastDir = parentOfSelection;
                    AppSettings.Default.Save();
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
            AppSettings.Default.Pinned = false;
            AppSettings.Default.Save();

            // Unpin share window too if it's open
            if (shareWindow != null && !shareWindow.IsDisposed)
            {
                shareWindow.TopMost = false;
            }
        }

        private void pin_Click(object sender, EventArgs e)
        {
            this.TopMost = true;
            unPin.BringToFront();
            AppSettings.Default.Pinned = true;
            AppSettings.Default.Save();

            // Pin share window too if it's open
            if (shareWindow != null && !shareWindow.IsDisposed)
            {
                shareWindow.TopMost = true;
            }
        }

        private void autoCrackOff_Click(object sender, EventArgs e)
        {
            autoCrackEnabled = true;
            autoCrackOn.BringToFront();
            AppSettings.Default.AutoCrack = true;
            AppSettings.Default.Save();
        }

        private void autoCrackOn_Click(object sender, EventArgs e)
        {
            autoCrackEnabled = false;
            autoCrackOff.BringToFront();
            AppSettings.Default.AutoCrack = false;
            AppSettings.Default.Save();
        }

        private void SteamAppId_FormClosing(object sender, FormClosingEventArgs e)
        {
            SteamAppIdIdentifier.Program.CmdKILL("APPID");
        }

        private void OpenDir_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", gameDir);
        }

        // Override KeyProcessCmdKey to catch Ctrl+S
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                // Force show compression settings dialog
                AppSettings.Default.ZipDontAsk = false;
                AppSettings.Default.Save();

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

                // Add -bsp1 to get progress percentage output
                string arguments = $"a -t{archiveType} {compressionSwitch} {passwordArg} -bsp1 \"{outputPath}\" \"{sourcePath}\\*\" -r";

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
                    // Read output asynchronously to capture progress
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            // 7-Zip outputs progress like "5%" or " 42%"
                            var match = System.Text.RegularExpressions.Regex.Match(e.Data, @"(\d+)%");
                            if (match.Success)
                            {
                                int percentage = int.Parse(match.Groups[1].Value);
                                try
                                {
                                    // Update the RGB text with actual progress
                                    this.Invoke(new Action(() =>
                                    {
                                        Tit($"Compressing... {percentage}%", Color.Cyan);
                                    }));
                                }
                                catch { }
                            }
                        }
                    };

                    process.BeginOutputReadLine();
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
                            content.Add(new StringContent("anonymous"), "hwid"); // Anonymous sharing
                            content.Add(new StringContent("SACGUI-2.2"), "version");

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
                            System.Diagnostics.Debug.WriteLine($"[UPLOAD] Added Version: SACGUI-2.2");
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
                                progressClient.DefaultRequestHeaders.Add("User-Agent", "SACGUI-Uploader/2.2");

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

        private System.Threading.CancellationTokenSource zipCancellationTokenSource;

        private async void ZipToShare_Click(object sender, EventArgs e)
        {
            // If button says "Show Zip", reveal the file instead
            if (ZipToShare.Text == "Show Zip" && ZipToShare.Tag != null)
            {
                string savedZipPath = ZipToShare.Tag.ToString();
                if (File.Exists(savedZipPath))
                {
                    Process.Start("explorer.exe", $"/select,\"{savedZipPath}\"");
                }
                // Don't reset - stays as "Show Zip" until next crack
                return;
            }

            // If button says "Cancel", cancel the compression
            if (ZipToShare.Text == "Cancel")
            {
                zipCancellationTokenSource?.Cancel();
                ZipToShare.Text = "Zip Dir";
                // Reset button appearance (remove orange glow)
                ZipToShare.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 60);
                ZipToShare.FlatAppearance.BorderSize = 1;
                ZipToShare.ForeColor = Color.FromArgb(220, 220, 225);
                Tit("Compression cancelled", Color.Orange);
                return;
            }

            string zipPath = "";  // Declare outside try block so finally can access it
            bool compressionCancelled = false;

            try
            {
                string gameName = gameDirName;
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                string compressionType = AppSettings.Default.ZipFormat;
                string compressionLevelStr = AppSettings.Default.ZipLevel;
                bool skipDialog = AppSettings.Default.ZipDontAsk;

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
                            AppSettings.Default.ZipFormat = compressionType;
                            AppSettings.Default.ZipLevel = compressionLevelStr;
                            AppSettings.Default.ZipDontAsk = true;
                            AppSettings.Default.Save();
                        }
                    }
                }

                // Prepare cancellation token but DON'T change button yet
                zipCancellationTokenSource = new System.Threading.CancellationTokenSource();

                bool use7z = true;  // Always use 7z for both .zip and .7z
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

                // Sanitize game name for filename
                string safeGameName = gameName;
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    safeGameName = safeGameName.Replace(c.ToString(), "");
                }

                // Determine file extension based on compression type
                bool is7zFormat = compressionType.StartsWith("7Z");
                string zipName = is7zFormat ? $"[SACGUI] {safeGameName}.7z" : $"[SACGUI] {safeGameName}.zip";
                zipPath = Path.Combine(desktopPath, zipName);

                // HSL to RGB converter for smooth color transitions
                Func<double, double, double, Color> HSLToRGB = (h, s, l) =>
                {
                    h = h / 360.0;
                    double r, g, b;

                    if (s == 0)
                    {
                        r = g = b = l;
                    }
                    else
                    {
                        Func<double, double, double, double> HueToRGB = (pVal, qVal, t) =>
                        {
                            if (t < 0) t += 1;
                            if (t > 1) t -= 1;
                            if (t < 1.0 / 6) return pVal + (qVal - pVal) * 6 * t;
                            if (t < 1.0 / 2) return qVal;
                            if (t < 2.0 / 3) return pVal + (qVal - pVal) * (2.0 / 3 - t) * 6;
                            return pVal;
                        };

                        var qHsl = l < 0.5 ? l * (1 + s) : l + s - l * s;
                        var pHsl = 2 * l - qHsl;
                        r = HueToRGB(pHsl, qHsl, h + 1.0 / 3);
                        g = HueToRGB(pHsl, qHsl, h);
                        b = HueToRGB(pHsl, qHsl, h - 1.0 / 3);
                    }

                    return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
                };

                if (use7z)
                {
                    // Show initial progress
                    Tit($"Starting 7z compression (level {levelNum})...", HSLToRGB(0, 1.0, 0.75));

                    // Use 7zip with selected compression level
                    await Task.Run(async () =>
                    {
                        double colorHue = 0;

                        // Check if cancelled before starting
                        if (zipCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            compressionCancelled = true;
                            return;
                        }

                        // NOW change button to Cancel with orange glow (compression is actually starting)
                        this.Invoke(new Action(() =>
                        {
                            ZipToShare.Enabled = true;
                            ZipToShare.Text = "Cancel";
                            ZipToShare.FlatAppearance.BorderColor = Color.FromArgb(255, 150, 0);
                            ZipToShare.FlatAppearance.BorderSize = 2;
                            ZipToShare.ForeColor = Color.FromArgb(255, 180, 100);
                        }));

                        // Use 7za.exe from _bin folder
                        string sevenZipPath = $"{BinPath}\\7z\\7za.exe";

                        // Build 7z command with actual compression level and progress output to stdout
                        string formatType = is7zFormat ? "7z" : "zip";

                        // Smart RAM detection - adapt dictionary size to available memory
                        string memParams = "";
                        if (formatType == "7z" && levelNum > 0)
                        {
                            // Get available physical memory using PerformanceCounter
                            ulong availRAM = 1024; // Default 1GB if detection fails
                            try
                            {
                                using (var pc = new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes"))
                                {
                                    availRAM = (ulong)pc.NextValue();
                                }
                            }
                            catch
                            {
                                // Fallback to conservative default
                                availRAM = 1024;
                            }

                            // Use 10% of available RAM for dictionary, with limits for 32-bit exe
                            ulong targetDict = availRAM / 10;

                            // Set dictionary based on available RAM and compression level
                            // 64-bit 7z.exe can use MUCH more memory!
                            int dictSize;
                            if (availRAM < 2048)  // Less than 2GB available
                                dictSize = 64;
                            else if (availRAM < 4096)  // 2-4GB available
                                dictSize = 128;
                            else if (availRAM < 8192)  // 4-8GB available
                                dictSize = 256;
                            else if (availRAM < 16384) // 8-16GB available
                                dictSize = 512;
                            else if (availRAM < 32768) // 16-32GB available
                                dictSize = 1024;  // 1GB dictionary
                            else if (availRAM < 65536) // 32-64GB available
                                dictSize = 1536;  // 1.5GB dictionary
                            else  // 64GB+ available - USE THAT SHIT
                                dictSize = 2048;  // 2GB dictionary!

                            // Cap based on compression level (but be aggressive if they have RAM)
                            if (levelNum <= 3 && dictSize > 256)
                                dictSize = 256;
                            else if (levelNum <= 5 && dictSize > 512)
                                dictSize = 512;
                            else if (levelNum <= 7 && dictSize > 1024)
                                dictSize = 1024;

                            // No need to cap for 64-bit exe!

                            memParams = $" -md={dictSize}m";
                            System.Diagnostics.Debug.WriteLine($"[7Z] Available RAM: {availRAM}MB, Using dictionary: {dictSize}MB for level {levelNum}");
                        }
                        string args = $"a -mx{levelNum} -t{formatType}{memParams} -bsp1 \"{zipPath}\" \"{gameDir}\"\\* -r";

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = sevenZipPath,
                            Arguments = args,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using (Process p = Process.Start(psi))
                        {
                            string lastProgressText = $"Compressing with 7z (level {levelNum})...";

                            // Read stderr asynchronously for progress
                            var stderrTask = Task.Run(() =>
                            {
                                string line;
                                while ((line = p.StandardError.ReadLine()) != null)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[7Z STDERR] {line}");

                                    // Parse percentage from 7z output if available
                                    var percentMatch = System.Text.RegularExpressions.Regex.Match(line, @"(\d+)%");
                                    if (percentMatch.Success)
                                    {
                                        lastProgressText = $"Compressing with 7z... {percentMatch.Groups[1].Value}%";

                                        // Smooth HSL cycling for pastel colors
                                        colorHue = (colorHue + 3) % 360;
                                        Color currentColor = HSLToRGB(colorHue, 1.0, 0.75);

                                        this.Invoke(new Action(() =>
                                        {
                                            Tit(lastProgressText, currentColor);
                                        }));
                                    }
                                }
                            });

                            // Check stdout for progress too
                            var stdoutTask = Task.Run(() =>
                            {
                                string line;
                                while ((line = p.StandardOutput.ReadLine()) != null)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[7Z STDOUT] {line}");

                                    // Parse percentage from stdout as well
                                    var percentMatch = System.Text.RegularExpressions.Regex.Match(line, @"(\d+)%");
                                    if (percentMatch.Success)
                                    {
                                        lastProgressText = $"Compressing with 7z... {percentMatch.Groups[1].Value}%";

                                        // Smooth HSL cycling for pastel colors
                                        colorHue = (colorHue + 3) % 360;
                                        Color currentColor = HSLToRGB(colorHue, 1.0, 0.75);

                                        this.Invoke(new Action(() =>
                                        {
                                            Tit(lastProgressText, currentColor);
                                        }));
                                    }
                                }
                            });

                            // Wait for process with cancellation support and show progress
                            while (!p.HasExited)
                            {
                                if (zipCancellationTokenSource.Token.IsCancellationRequested)
                                {
                                    p.Kill();
                                    compressionCancelled = true;
                                    break;
                                }

                                // Show smooth HSL progress even without percentage
                                colorHue = (colorHue + 3) % 360;
                                Color currentColor = HSLToRGB(colorHue, 1.0, 0.75);
                                this.Invoke(new Action(() =>
                                {
                                    Tit(lastProgressText, currentColor);
                                }));

                                await Task.Delay(50);
                            }

                            if (!compressionCancelled)
                            {
                                p.WaitForExit();
                                await stderrTask;
                                await stdoutTask;
                            }
                        }
                    });
                }
                else
                {
                    // Use standard .NET zip (this shouldn't run since use7z is always true)
                    await Task.Run(async () =>
                    {
                        var allFiles = Directory.GetFiles(gameDir, "*", SearchOption.AllDirectories);
                        int totalFiles = allFiles.Length;
                        int currentFile = 0;
                        double colorHue = 0;

                        using (var archive = System.IO.Compression.ZipFile.Open(zipPath, System.IO.Compression.ZipArchiveMode.Create))
                        {
                            foreach (string file in allFiles)
                            {
                                currentFile++;
                                int percentage = (currentFile * 100) / totalFiles;

                                // Smooth HSL cycling
                                colorHue = (colorHue + 3) % 360;
                                Color currentColor = HSLToRGB(colorHue, 1.0, 0.75);

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
                double finalHue = 0;
                for (int i = 0; i < 10; i++)
                {
                    finalHue = (finalHue + 36) % 360;
                    Tit($"Saved to Desktop: {zipName}", HSLToRGB(finalHue, 1.0, 0.75));
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

                // Reset button appearance (remove orange glow)
                ZipToShare.FlatAppearance.BorderColor = Color.FromArgb(55, 55, 60);
                ZipToShare.FlatAppearance.BorderSize = 1;
                ZipToShare.ForeColor = Color.FromArgb(220, 220, 225);

                // If cancelled, reset to Zip Dir
                if (compressionCancelled)
                {
                    ZipToShare.Text = "Zip Dir";
                    ZipToShare.Tag = null;
                    // Delete partial zip file if it exists
                    if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
                    {
                        try { File.Delete(zipPath); } catch { }
                    }
                }
                // Only change to "Show Zip" if the file actually exists
                else if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
                {
                    ZipToShare.Text = "Show Zip";
                    ZipToShare.Tag = zipPath;
                }
                else
                {
                    // Compression failed, keep it as Zip Dir
                    ZipToShare.Text = "Zip Dir";
                    ZipToShare.Tag = null;
                }

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
                CurrentAppId = ManAppBox.Text;
                APPNAME = "";

                // Try to fetch game name from Steam API
                FetchGameNameFromSteamAPI(CurrentAppId);

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
                    CurrentAppId = ManAppBox.Text;
                    APPNAME = "";
                    btnManualEntry.Visible = false;
                    // Try to fetch game name from Steam API
                    FetchGameNameFromSteamAPI(CurrentAppId);

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

        private EnhancedShareWindow shareWindow = null;

        private void ShareButton_Click(object sender, EventArgs e)
        {
            // Prevent multiple share windows - reuse existing one
            if (shareWindow != null && !shareWindow.IsDisposed)
            {
                shareWindow.BringToFront();
                shareWindow.Activate();
                return;
            }

            // Show the enhanced share window with your Steam games
            shareWindow = new EnhancedShareWindow(this);
            shareWindow.Owner = this;  // Set owner so it stays above main window
            shareWindow.FormClosed += (s, args) => shareWindow = null;  // Clear reference when closed

            // Sync window state - restoring either window restores both
            shareWindow.Resize += ShareWindow_Resize;

            // Sync pin state - use actual saved setting, not the form's TopMost which might be wrong
            shareWindow.TopMost = AppSettings.Default.Pinned;
            // Also fix the main form's TopMost to match the setting
            this.TopMost = AppSettings.Default.Pinned;

            shareWindow.Show();
            shareWindow.BringToFront();
            shareWindow.Activate();
        }

        private void ShareWindow_Resize(object sender, EventArgs e)
        {
            // When share window is restored, restore main window too
            if (shareWindow != null && shareWindow.WindowState == FormWindowState.Normal && this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
        }


        // All RIN scraping code removed - no longer needed

        private void resinstruccZip_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        #region Batch Game Detection

        /// <summary>
        /// Detects if a folder is a game folder
        /// </summary>
        private bool IsGameFolder(string path)
        {
            if (!Directory.Exists(path)) return false;

            try
            {
                // Has exe in folder = game
                var exeFiles = Directory.GetFiles(path, "*.exe", SearchOption.TopDirectoryOnly);
                if (exeFiles.Length > 0) return true;

                // Has Engine folder = Unreal game
                if (Directory.Exists(Path.Combine(path, "Engine"))) return true;

                // Unity pattern: *_Data folder = game
                var dirs = Directory.GetDirectories(path);
                foreach (var dir in dirs)
                {
                    if (dir.EndsWith("_Data", StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Detects games in subfolders when user selects a folder containing multiple games
        /// </summary>
        private List<string> DetectGamesInFolder(string path)
        {
            var games = new List<string>();
            if (!Directory.Exists(path)) return games;

            try
            {
                // Only check direct children - don't recurse into subfolders
                var subfolders = Directory.GetDirectories(path);
                foreach (var subfolder in subfolders)
                {
                    if (IsGameFolder(subfolder))
                    {
                        games.Add(subfolder);
                    }
                }
            }
            catch { }

            return games;
        }

        /// <summary>
        /// Shows batch game selection form (non-modal)
        /// </summary>
        private void ShowBatchGameSelection(List<string> gamePaths)
        {
            var form = new BatchGameSelectionForm(gamePaths);
            form.ProcessRequested += async (games, format, level, usePassword) =>
            {
                await ProcessBatchGames(games, format, level, usePassword, form);
            };
            form.Show(this);
        }

        /// <summary>
        /// Processes multiple games in batch with per-game actions
        /// </summary>
        private async Task ProcessBatchGames(List<BatchGameItem> games, string compressionFormat, string compressionLevel, bool usePassword, BatchGameSelectionForm batchForm)
        {
            batchForm.SetProcessingMode(true);
            await Task.Delay(50); // Let UI update

            int success = 0;
            int failed = 0;
            int zipped = 0;
            int zipFailed = 0;
            int uploaded = 0;
            int uploadFailed = 0;

            var failureReasons = new List<(string gameName, string reason)>();
            var uploadResults = new List<(string gameName, string oneFichierUrl, string pydriveUrl)>();
            var crackResults = new Dictionary<string, bool>(); // Track which games cracked successfully
            var archivePaths = new Dictionary<string, string>(); // Track archive paths for upload

            // ========== PHASE 1: CRACK (Sequential due to shared state) ==========
            foreach (var game in games.Where(g => g.Crack))
            {
                batchForm.UpdateStatus(game.Path, "Cracking...", Color.Yellow);

                if (string.IsNullOrEmpty(game.AppId))
                {
                    batchForm.UpdateStatus(game.Path, "No AppID", Color.Orange);
                    failureReasons.Add((game.Name, "No AppID found"));
                    crackResults[game.Path] = false;
                    failed++;
                    continue;
                }

                try
                {
                    gameDir = game.Path;
                    gameDirName = game.Name;
                    parentOfSelection = Directory.GetParent(game.Path).FullName;
                    CurrentAppId = game.AppId;

                    suppressStatusUpdates = true;
                    bool crackSucceeded = await CrackAsync();
                    suppressStatusUpdates = false;

                    crackResults[game.Path] = crackSucceeded;

                    // Store crack details for this game
                    if (CurrentCrackDetails != null)
                    {
                        batchForm.StoreCrackDetails(game.Path, CurrentCrackDetails);
                    }

                    if (crackSucceeded)
                    {
                        success++;
                        batchForm.UpdateStatus(game.Path, "Cracked ✓", Color.LightGreen);
                    }
                    else
                    {
                        failed++;
                        batchForm.UpdateStatus(game.Path, "Crack Failed", Color.Red);
                        failureReasons.Add((game.Name, "Crack returned false"));
                    }
                }
                catch (Exception ex)
                {
                    crackResults[game.Path] = false;
                    failed++;
                    batchForm.UpdateStatus(game.Path, "Crack Error", Color.Red);
                    failureReasons.Add((game.Name, $"Crack exception: {ex.Message}"));
                }
            }

            // Games that didn't need cracking
            foreach (var game in games.Where(g => !g.Crack))
            {
                crackResults[game.Path] = true;
            }

            suppressStatusUpdates = false;

            // ========== PHASE 2: ZIP (Parallel) ==========
            var gamesToZip = games.Where(g => g.Zip && crackResults.ContainsKey(g.Path) && crackResults[g.Path]).ToList();

            if (gamesToZip.Count > 0)
            {
                // Update all to "Zipping..."
                foreach (var game in gamesToZip)
                {
                    batchForm.UpdateStatus(game.Path, "Zipping...", Color.Cyan);
                }

                string sevenZipPath = ResourceExtractor.GetBinFilePath(Path.Combine("7z", "7za.exe"));
                string password = usePassword ? "rin" : null;

                var zipTasks = gamesToZip.Select(async game =>
                {
                    string ext = compressionFormat == "7Z" ? ".7z" : ".zip";
                    string parent = Directory.GetParent(game.Path).FullName;
                    string archivePath = Path.Combine(parent, game.Name + ext);
                    archivePaths[game.Path] = archivePath;

                    string zipError = null;
                    bool zipSuccess = await Task.Run(() =>
                    {
                        try
                        {
                            string formatArg = compressionFormat == "7Z" ? "-t7z" : "-tzip";
                            string args = $"a {formatArg} -mx={compressionLevel} \"{archivePath}\" \"{game.Path}\\*\"";
                            if (!string.IsNullOrEmpty(password)) args += $" -p{password}";

                            var psi = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = sevenZipPath,
                                Arguments = args,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            };

                            using (var proc = System.Diagnostics.Process.Start(psi))
                            {
                                string stderr = proc.StandardError.ReadToEnd();
                                proc.WaitForExit();
                                if (proc.ExitCode != 0 && !string.IsNullOrEmpty(stderr))
                                    zipError = stderr.Length > 100 ? stderr.Substring(0, 100) + "..." : stderr;
                                return proc.ExitCode == 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            zipError = ex.Message;
                            return false;
                        }
                    });

                    return (game, zipSuccess, zipError);
                }).ToList();

                var zipResults = await Task.WhenAll(zipTasks);

                foreach (var result in zipResults)
                {
                    var game = result.Item1;
                    var zipSuccess = result.Item2;
                    var zipError = result.Item3;
                    string archivePath = archivePaths.ContainsKey(game.Path) ? archivePaths[game.Path] : null;

                    if (zipSuccess)
                    {
                        zipped++;
                        batchForm.UpdateStatus(game.Path, "Zipped ✓", Color.LightGreen);
                        batchForm.UpdateZipStatus(game.Path, true, archivePath);
                    }
                    else
                    {
                        zipFailed++;
                        batchForm.UpdateStatus(game.Path, "Zip Failed", Color.Red);
                        batchForm.UpdateZipStatus(game.Path, false, archivePath, zipError ?? "Unknown error");
                        failureReasons.Add((game.Name, $"Zip failed: {zipError ?? "Unknown"}"));
                    }
                }
            }

            // ========== PHASE 3: UPLOAD (Sequential uploads with retry, parallel conversions) ==========
            var gamesToUpload = games.Where(g => g.Upload && crackResults.ContainsKey(g.Path) && crackResults[g.Path]).ToList();
            var conversionTasks = new List<Task>();
            var uploadedLinks = new System.Collections.Concurrent.ConcurrentBag<(string gameName, string path, string oneFichierUrl)>();
            const int maxRetries = 3;

            foreach (var game in gamesToUpload)
            {
                string archivePath = archivePaths.ContainsKey(game.Path) ? archivePaths[game.Path] : null;
                if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath))
                {
                    batchForm.UpdateStatus(game.Path, "No Archive", Color.Orange);
                    batchForm.UpdateUploadStatus(game.Path, false, null, "Archive not found");
                    failureReasons.Add((game.Name, "Archive not found"));
                    uploadFailed++;
                    continue;
                }

                bool uploadSuccess = false;
                string lastError = null;
                int attempt = 0;
                long fileSize = new FileInfo(archivePath).Length;

                // Show upload details panel
                batchForm.ShowUploadDetails(game.Name, fileSize);

                while (!uploadSuccess && attempt < maxRetries)
                {
                    attempt++;
                    string statusPrefix = attempt > 1 ? $"Retry {attempt}/{maxRetries}: " : "";

                    batchForm.UpdateStatus(game.Path, $"{statusPrefix}Uploading...", Color.Magenta);

                    try
                    {
                        using (var uploader = new SAC_GUI.OneFichierUploader())
                        {
                            // Capture game path for closure
                            var gamePath = game.Path;
                            var currentAttempt = attempt;

                            // Track speed
                            var uploadStartTime = DateTime.Now;
                            double lastProgress = 0;
                            DateTime lastProgressTime = DateTime.Now;
                            double smoothedSpeed = 0;

                            var progress = new Progress<double>(p =>
                            {
                                int pct = (int)(p * 100);
                                string prefix = currentAttempt > 1 ? $"Retry {currentAttempt}: " : "";
                                batchForm.UpdateStatus(gamePath, $"{prefix}Upload {pct}%", Color.Magenta);

                                // Calculate speed
                                var now = DateTime.Now;
                                double progressDelta = p - lastProgress;
                                double timeDelta = (now - lastProgressTime).TotalSeconds;

                                if (timeDelta > 0.1 && progressDelta > 0)
                                {
                                    long uploadedBytes = (long)(p * fileSize);
                                    double bytesDelta = progressDelta * fileSize;
                                    double currentSpeed = bytesDelta / timeDelta;

                                    // Smooth the speed calculation
                                    smoothedSpeed = smoothedSpeed > 0 ? (smoothedSpeed * 0.7 + currentSpeed * 0.3) : currentSpeed;

                                    batchForm.UpdateUploadProgress(pct, uploadedBytes, fileSize, smoothedSpeed);

                                    lastProgress = p;
                                    lastProgressTime = now;
                                }
                            });

                            // Wrap in Task.Run because OneFichierUploader uses synchronous blocking I/O
                            var result = await Task.Run(() => uploader.UploadFileAsync(archivePath, progress));

                            if (result != null && !string.IsNullOrEmpty(result.DownloadUrl))
                            {
                                string oneFichierUrl = result.DownloadUrl;
                                uploaded++;
                                uploadSuccess = true;
                                batchForm.UpdateStatus(game.Path, "Converting...", Color.Yellow);

                                // Store upload success in details
                                batchForm.UpdateUploadStatus(game.Path, true, oneFichierUrl, null, attempt - 1);

                                // Copy 1fichier link immediately
                                try { Clipboard.SetText(oneFichierUrl); } catch { }

                                // Start conversion in background - don't wait, continue to next upload
                                var gameCopy = game;
                                var urlCopy = oneFichierUrl;

                                // Store URL for click-to-copy
                                batchForm.SetConvertingUrl(gameCopy.Path, urlCopy);

                                var conversionTask = Task.Run(async () =>
                                {
                                    string pydriveUrl = await ConvertOneFichierToPydrive(urlCopy, fileSize,
                                        status => batchForm.UpdateStatus(gameCopy.Path, status, Color.Yellow));

                                    // Clear the converting URL (no longer needed for click-to-copy)
                                    batchForm.ClearConvertingUrl(gameCopy.Path);

                                    if (!string.IsNullOrEmpty(pydriveUrl))
                                    {
                                        uploadResults.Add((gameCopy.Name, urlCopy, pydriveUrl));
                                        batchForm.UpdateStatus(gameCopy.Path, "PyDrive ✓", Color.LightGreen);
                                        batchForm.SetFinalUrl(gameCopy.Path, pydriveUrl);
                                    }
                                    else
                                    {
                                        uploadResults.Add((gameCopy.Name, urlCopy, null));
                                        batchForm.UpdateStatus(gameCopy.Path, "1fichier ✓", Color.LightGreen);
                                        batchForm.SetFinalUrl(gameCopy.Path, urlCopy);
                                    }
                                });
                                conversionTasks.Add(conversionTask);
                            }
                            else
                            {
                                lastError = "No download URL returned";
                                System.Diagnostics.Debug.WriteLine($"[BATCH] Upload attempt {attempt} failed for {game.Name}: No URL returned");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                        System.Diagnostics.Debug.WriteLine($"[BATCH] Upload attempt {attempt} error for {game.Name}: {ex.Message}");

                        // Wait before retry (exponential backoff)
                        if (attempt < maxRetries)
                        {
                            batchForm.UpdateStatus(game.Path, $"Retry in {attempt * 2}s...", Color.Yellow);
                            await Task.Delay(attempt * 2000);
                        }
                    }
                }

                if (!uploadSuccess)
                {
                    uploadFailed++;
                    string shortError = lastError?.Length > 30 ? lastError.Substring(0, 30) + "..." : lastError;
                    batchForm.UpdateStatus(game.Path, $"Failed: {shortError}", Color.Red);
                    batchForm.UpdateUploadStatus(game.Path, false, null, lastError, attempt - 1);
                    failureReasons.Add((game.Name, $"Upload failed after {attempt} attempts: {lastError}"));
                }
            }

            // Hide upload details panel when done
            batchForm.HideUploadDetails();

            // Wait for all conversions to complete
            if (conversionTasks.Count > 0)
            {
                await Task.WhenAll(conversionTasks);
            }

            // ========== DONE ==========
            batchForm.SetProcessingMode(false);

            // Build summary
            var summaryParts = new List<string>();
            if (success > 0) summaryParts.Add($"{success} cracked");
            if (zipped > 0) summaryParts.Add($"{zipped} zipped");
            if (uploaded > 0) summaryParts.Add($"{uploaded} uploaded");
            if (failed > 0) summaryParts.Add($"{failed} crack failed");
            if (zipFailed > 0) summaryParts.Add($"{zipFailed} zip failed");
            if (uploadFailed > 0) summaryParts.Add($"{uploadFailed} upload failed");

            string summary = string.Join(", ", summaryParts);
            if (string.IsNullOrEmpty(summary)) summary = "No actions performed";

            Tit($"Batch complete! {summary}", Color.LightGreen);

            // Build all links in phpBB format for cs.rin.ru: [url=link]Game Name[/url]
            string allLinksText = "";
            if (uploadResults.Count > 0)
            {
                allLinksText = string.Join("\n", uploadResults.Select(r =>
                {
                    string url = string.IsNullOrEmpty(r.pydriveUrl) ? r.oneFichierUrl : r.pydriveUrl;
                    return $"[url={url}]{r.gameName}[/url]";
                }));
            }

            // Show Copy All button if there are links
            batchForm.ShowCopyAllButton(allLinksText);
        }

        /// <summary>
        /// Converts a 1fichier link to a pydrive link
        /// </summary>
        private async Task<string> ConvertOneFichierToPydrive(string oneFichierUrl, long fileSizeBytes = 0, Action<string> statusUpdate = null)
        {
            if (oneFichierUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                oneFichierUrl = "https://" + oneFichierUrl.Substring(7);

            // Calculate wait time based on file size (12 seconds per GB, capped at 30 hours)
            int initialWaitSeconds = 30;
            if (fileSizeBytes > 5L * 1024 * 1024 * 1024) // 5GB+
            {
                long sizeInGB = fileSizeBytes / (1024 * 1024 * 1024);
                initialWaitSeconds = (int)(sizeInGB * 12);
                initialWaitSeconds = Math.Min(initialWaitSeconds, 108000); // Cap at 30 hours
                initialWaitSeconds = Math.Max(initialWaitSeconds, 30);
            }

            // Calculate retries: enough to cover estimated wait time plus buffer
            int maxRetries = Math.Max(10, (initialWaitSeconds / 30) + 5);
            int retryDelay = 30000;

            double sizeGB = fileSizeBytes / (1024.0 * 1024.0 * 1024.0);
            Console.WriteLine($"[CONVERT] Starting conversion for {oneFichierUrl}");
            Console.WriteLine($"[CONVERT] File size: {sizeGB:F2} GB, Initial wait: {initialWaitSeconds}s, Max retries: {maxRetries}");

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    int remainingSeconds = Math.Max(0, initialWaitSeconds - ((attempt - 1) * 30));
                    string timeStr = remainingSeconds > 60 ? $"~{remainingSeconds / 60}m" : $"~{remainingSeconds}s";
                    statusUpdate?.Invoke($"Converting {attempt}/{maxRetries} ({timeStr})");

                    Console.WriteLine($"[CONVERT] Attempt {attempt}/{maxRetries} - estimated {timeStr} remaining");

                    using (var client = new System.Net.Http.HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(30); // Match EnhancedShareWindow

                        var requestBody = new { link = oneFichierUrl };
                        var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
                        var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                        var response = await client.PostAsync("https://pydrive.harryeffingpotter.com/convert-1fichier", content).ConfigureAwait(false);

                        Console.WriteLine($"[CONVERT] Response status: {(int)response.StatusCode} {response.StatusCode}");

                        if (response.IsSuccessStatusCode)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            Console.WriteLine($"[CONVERT] Response body: {responseJson}");

                            // API returns {"link": "..."} - same as EnhancedShareWindow
                            using (var doc = System.Text.Json.JsonDocument.Parse(responseJson))
                            {
                                if (doc.RootElement.TryGetProperty("link", out var linkProp))
                                {
                                    string pydriveUrl = linkProp.GetString();
                                    Console.WriteLine($"[CONVERT] SUCCESS! PyDrive URL: {pydriveUrl}");
                                    return pydriveUrl;
                                }
                                else
                                {
                                    Console.WriteLine($"[CONVERT] No 'link' in response, will retry...");
                                }
                            }
                        }
                        else
                        {
                            var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            Console.WriteLine($"[CONVERT] Error response: {errorBody}");

                            // Check if it's a "still processing" error - keep retrying
                            if (errorBody.Contains("LINK_DOWN") || errorBody.Contains("wait"))
                            {
                                Console.WriteLine($"[CONVERT] 1fichier still scanning file, waiting...");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CONVERT] Exception on attempt {attempt}: {ex.Message}");
                }

                if (attempt < maxRetries)
                {
                    Console.WriteLine($"[CONVERT] Waiting {retryDelay / 1000}s before retry...");
                    await Task.Delay(retryDelay).ConfigureAwait(false);
                }
            }

            Console.WriteLine($"[CONVERT] FAILED after {maxRetries} attempts, will use 1fichier link");
            return null; // Conversion failed, will use 1fichier link
        }

        #endregion
    }
}