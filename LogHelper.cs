using System;
using System.IO;

namespace APPID
{
    public static class LogHelper
    {
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;

        public static void Log(string message)
        {
            try
            {
                lock (_lockObject)
                {
                    string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SACGUI");
                    if (!Directory.Exists(appDataPath))
                        Directory.CreateDirectory(appDataPath);
                    string logFile = Path.Combine(appDataPath, "debug.log");

                    // On first log of session, add separator if not already initialized
                    if (!_initialized)
                    {
                        _initialized = true;

                        // Wipe log if it's over 10MB
                        if (File.Exists(logFile))
                        {
                            FileInfo fi = new FileInfo(logFile);
                            if (fi.Length > 10 * 1024 * 1024) // 10MB
                            {
                                File.Delete(logFile);
                            }
                        }

                        string separator = $"{Environment.NewLine}{Environment.NewLine}================================={Environment.NewLine}SACGUI LAUNCHED{Environment.NewLine}{DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}================================={Environment.NewLine}";
                        File.AppendAllText(logFile, separator);
                    }

                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                    File.AppendAllText(logFile, logEntry);
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
            }
            catch { }
        }

        public static void LogError(string context, Exception ex)
        {
            Log($"[ERROR] {context}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Log($"[ERROR] Inner: {ex.InnerException.Message}");
            }
        }

        public static void LogUpdate(string component, string version)
        {
            Log($"[UPDATE] {component} updated to version {version}");
        }

        public static void LogNetwork(string message)
        {
            Log($"[NETWORK] {message}");
        }

        public static void LogAPI(string api, string status)
        {
            Log($"[API] {api}: {status}");
        }
    }
}