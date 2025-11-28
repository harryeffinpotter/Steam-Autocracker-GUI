using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace APPID
{
    public class DataTableGeneration
    {
        public static DataTable dataTable;
        public static DataTable dt;

        public DataTableGeneration() { }
        public static string RemoveSpecialCharacters(string str)
        {
            str = str.Replace(":", " -").Replace("'", "").Replace("&", "and");
            return Regex.Replace(str, "[^a-zA-Z0-9._0 -]+", "", RegexOptions.Compiled);
        }
        private static void LogError(string message)
        {
            // Use the centralized LogHelper instead
            LogHelper.Log($"[DataTableGeneration] {message}");
        }

        public async Task<DataTable> GetDataTableAsync(DataTableGeneration dataTableGeneration)
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SACGUI");
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);
            string cacheFile = Path.Combine(appDataPath, "steam_cache.json");
            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("AppId", typeof(int));

            // Load from cache first if it exists
            if (File.Exists(cacheFile))
            {
                try
                {
                    string cachedContent = File.ReadAllText(cacheFile);
                    SteamGames cachedGames = JsonConvert.DeserializeObject<SteamGames>(cachedContent);
                    if (cachedGames != null && cachedGames.Apps != null)
                    {
                        foreach (var item in cachedGames.Apps)
                        {
                            if (item.Name.ToLower().Contains("demo") || item.Name.ToLower().Contains("soundtrack"))
                                continue;
                            string ItemWithoutTroubles = RemoveSpecialCharacters(item.Name);
                            dt.Rows.Add(ItemWithoutTroubles, item.Appid);
                        }
                        dataTableGeneration.DataTableToGenerate = dt;

                        // Update cache in background
                        Task.Run(async () => {
                            try
                            {
                                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                                using (HttpClient client = new HttpClient())
                                {
                                    client.Timeout = TimeSpan.FromSeconds(60);
                                    string freshContent = await client.GetStringAsync("https://pydrive.harryeffingpotter.com/sacgui/steam-applist");
                                    File.WriteAllText(cacheFile, freshContent);
                                    LogHelper.Log("[Steam Cache] Background update completed successfully");
                                }
                            }
                            catch (Exception ex)
                            {
                                LogError($"Background cache update failed: {ex.Message}");
                            }
                        });

                        return dt;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"[DataTableGeneration] Failed to load cache: {ex.Message}");
                }
            }

            // No cache or failed to load, fetch from backend
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(60);
                string content = await httpClient.GetStringAsync("https://pydrive.harryeffingpotter.com/sacgui/steam-applist");

                // Save to cache
                File.WriteAllText(cacheFile, content);

                SteamGames steamGames = JsonConvert.DeserializeObject<SteamGames>(content);
                foreach (var item in steamGames.Apps)
                {
                    if (item.Name.ToLower().Contains("demo") || item.Name.ToLower().Contains("soundtrack"))
                        continue;
                    string ItemWithoutTroubles = RemoveSpecialCharacters(item.Name);
                    dt.Rows.Add(ItemWithoutTroubles, item.Appid);
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to fetch from backend: {ex.Message}");
                LogHelper.LogNetwork("No internet connection - app running in offline mode");
            }

            dataTableGeneration.DataTableToGenerate = dt;
            return dt;
        }

        #region Get and Set
        public DataTable DataTableToGenerate
        {
            get { return dataTable; }   // get method
            set { dataTable = value; }  // set method
        }
        #endregion

        #region JSON Properties
        public partial class SteamGames
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("apps")]
            public App[] Apps { get; set; }
        }

        public partial class App
        {
            [JsonProperty("appid")]
            public long Appid { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
        #endregion
    }
}
