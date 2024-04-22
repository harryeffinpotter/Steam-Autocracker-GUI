using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        private void Fuzzy(string searchTerms)
        {


        }
        public async Task<DataTable> GetDataTableAsync(DataTableGeneration dataTableGeneration, string searchTerms, Dictionary<string, long> steam_dict)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("AppId", typeof(int));
            var rows_added = false;
            var split_search_length = 0;
            var search_list = new List<string>();
            foreach (var word in searchTerms.Split(' '))
            {
                if (word.Length > 0)
                {
                    split_search_length++;
                    search_list.Add(word);
                }
            }
            foreach (var steam_app in steam_dict)
            {
                if (searchTerms.ToLower().Trim() == steam_app.Key.ToLower().Trim())
                {
                    rows_added = true;
                    dt.Rows.Add(steam_app.Key, steam_app.Value);
                }
            }
            
            if (rows_added)
            {
                dataTableGeneration.DataTableToGenerate = dt;
                return dt;
            }
            return null;

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
