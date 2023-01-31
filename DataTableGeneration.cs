
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        public async Task<DataTable> GetDataTableAsync(DataTableGeneration dataTableGeneration) {
            HttpClient httpClient = new HttpClient();
            string content = await httpClient.GetStringAsync("https://api.steampowered.com/ISteamApps/GetAppList/v2/");
            SteamGames steamGames = JsonConvert.DeserializeObject<SteamGames>(content);

            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(String));
            dt.Columns.Add("AppId", typeof(int));

            foreach (var item in steamGames.Applist.Apps)
            {
                string ItemWithoutTroubles = RemoveSpecialCharacters(item.Name);
                dt.Rows.Add(ItemWithoutTroubles, item.Appid);
            }

            dataTableGeneration.DataTableToGenerate = dt;
            return dt;
        }

        #region Get and Set
        public DataTable DataTableToGenerate{
            get { return dataTable; }   // get method
            set { dataTable = value; }  // set method
        }
        #endregion

        #region JSON Properties
        public partial class SteamGames
        {
            [JsonProperty("applist")]
            public Applist Applist { get; set; }
        }

        public partial class Applist
        {
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
