using System;
using System.IO;
using Newtonsoft.Json;

namespace APPID
{
    public class AppSettings
    {
        private static AppSettings _instance;
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SACGUI",
            "settings.json");

        // Settings properties
        public string lastDir { get; set; } = "";
        public bool Goldy { get; set; } = true;
        public bool Pinned { get; set; } = true;
        public bool AutoCrack { get; set; } = false;
        public bool LANMultiplayer { get; set; } = false;
        public bool UseRinPassword { get; set; } = false;
        public string SharedGamesData { get; set; } = "";
        public string ZipFormat { get; set; } = "zip";
        public string ZipLevel { get; set; } = "Normal";
        public bool ZipDontAsk { get; set; } = false;
        public bool DeleteZipsAfterUpload { get; set; } = true;
        public string ZipOutputFolder { get; set; } = "";  // Empty = same folder as game
        public int CompressionLevel { get; set; } = 5;  // 0-10 slider value
        public string CompressionFormat { get; set; } = "ZIP";  // ZIP or 7Z
        public bool SkipPyDriveConversion { get; set; } = false;
        public string UploadBandwidthLimit { get; set; } = "";
        public double LastZipRateLevel0 { get; set; } = 0;
        public double LastZipRateCompressed { get; set; } = 0;
        public double LastUploadRate { get; set; } = 0;

        public static AppSettings Default
        {
            get
            {
                if (_instance == null)
                {
                    Load();
                }
                return _instance;
            }
        }

        private static void Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    _instance = JsonConvert.DeserializeObject<AppSettings>(json);
                }
                else
                {
                    _instance = new AppSettings();
                }
            }
            catch
            {
                _instance = new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }
    }
}