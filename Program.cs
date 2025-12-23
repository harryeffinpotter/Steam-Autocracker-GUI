using APPID;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    internal static class Program
    {
        public static Mutex mutex;
        public static APPID.SteamAppId form;
        public static string[] args2;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // Set up global exception handlers FIRST
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Application.ThreadException += OnThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            // Bootstrap _bin folder if missing
            BootstrapBinFolder();

            // Extract embedded _bin files on first run
            ResourceExtractor.ExtractBinFiles();

            try
            {

                bool mutexCreated = false;
                mutex = new System.Threading.Mutex(false, "APPID.exe", out mutexCreated);

                if (!mutexCreated)
                {
                    MessageBox.Show(new Form { TopMost = true }, "Steam APPID finder is already running!!", "Already Runing!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    mutex.Close();
                    Application.Exit();
                    return;
                }


                // Application.EnableVisualStyles();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                // Initialize bandwidth limit from settings
                try
                {
                    string bwLimit = APPID.Properties.Settings.Default.UploadBandwidthLimit ?? "";
                    SteamAutocrackGUI.CompressionSettingsForm.ParseBandwidthLimit(bwLimit);
                }
                catch { }

                form = new SteamAppId();
                args2 = args;
                Application.Run(form);
                Application.Exit();
            }
            catch (Exception ex)
            {
                WriteCrashLog(ex);
            }
        }

        private static void BootstrapBinFolder()
        {
            // Force use exe's actual directory
            string basePath = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
            string binPath = Path.Combine(basePath, "_bin");
            string sevenZipPath = Path.Combine(binPath, "7z", "7za.exe");

            // If 7za.exe exists, _bin folder is good
            if (File.Exists(sevenZipPath))
                return;

            try
            {
                string zipUrl = "https://share.harryeffingpotter.com/u/tattered-aidi.zip";
                string tempZip = Path.Combine(basePath, "_bin_download.zip");

                // Download the zip
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var response = client.GetAsync(zipUrl).Result;
                    response.EnsureSuccessStatusCode();
                    var bytes = response.Content.ReadAsByteArrayAsync().Result;
                    File.WriteAllBytes(tempZip, bytes);
                }

                // Extract using built-in .NET ZipFile
                ZipFile.ExtractToDirectory(tempZip, basePath, true);

                // Clean up
                File.Delete(tempZip);
            }
            catch (Exception ex)
            {
                // Log to file so we can see what went wrong
                try
                {
                    string logPath = Path.Combine(basePath, "bootstrap_error.log");
                    File.WriteAllText(logPath, $"Bootstrap failed: {ex}");
                }
                catch { }
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteCrashLog(e.ExceptionObject as Exception);
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            WriteCrashLog(e.Exception);
        }

        private static void WriteCrashLog(Exception ex)
        {
            try
            {
                string crashInfo = $"================================={Environment.NewLine}" +
                                 $"SACGUI CRASH REPORT{Environment.NewLine}" +
                                 $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}" +
                                 $"================================={Environment.NewLine}" +
                                 $"Version: {Application.ProductVersion}{Environment.NewLine}" +
                                 $"OS: {Environment.OSVersion}{Environment.NewLine}" +
                                 $".NET: {Environment.Version}{Environment.NewLine}" +
                                 $"{Environment.NewLine}" +
                                 $"Exception:{Environment.NewLine}" +
                                 $"{ex?.ToString() ?? "Unknown exception"}{Environment.NewLine}";

                // Write crash.log next to exe for easy access
                string crashFile = Path.Combine(AppContext.BaseDirectory, "crash.log");
                File.WriteAllText(crashFile, crashInfo);

                // Also log to debug.log if possible
                try
                {
                    LogHelper.Log($"[FATAL CRASH] {ex?.Message ?? "Unknown exception"}");
                }
                catch { }

                MessageBox.Show($"SACGUI has crashed!{Environment.NewLine}{Environment.NewLine}" +
                              $"A crash report has been saved to:{Environment.NewLine}{crashFile}{Environment.NewLine}{Environment.NewLine}" +
                              $"Error: {ex?.Message ?? "Unknown error"}",
                              "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // Last resort - at least try to show something
                MessageBox.Show("Fatal crash - unable to write crash log", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void CmdKILL(string appname)
        {
            Process[] processlist = Process.GetProcesses();
            foreach (Process process in processlist)
            {
                if (process.ProcessName.Contains(appname))
                {
                    ProcessStartInfo info = new ProcessStartInfo(@"cmd.exe");
                    info.UseShellExecute = true;
                    info.CreateNoWindow = true;
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    info.UseShellExecute = true;
                    info.Arguments = $"/C WMIC PROCESS WHERE \"Name Like '%{appname}%'\" CALL Terminate 1>nul 2>nul";
                    Process.Start(info);
                }
            }

        }
    }
}
