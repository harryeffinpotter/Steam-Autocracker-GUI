using Newtonsoft.Json;
using Octokit;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace APPID
{

    internal class Updater
    {
        public static bool hasinternet = false;
        private static string[] hosts = { "1.1.1.1", "8.8.8.8", "208.67.222.222" };


        public static dynamic getJson(string requestURL)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            try
            {

                string BaseURL = "https://api.github.com/repos/";
                var options = new RestClientOptions(BaseURL)
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                var client = new RestClient(options);
                Console.WriteLine($"Requesting: {BaseURL}{requestURL}");
                var request = new RestRequest(requestURL, Method.Get);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                var queryResult = client.Execute(request);
                var obj = JsonConvert.DeserializeObject<dynamic>(queryResult.Content);
                return obj;
            }
            catch
            {

                return null;
            }
        }
        public static bool offline = false;
        public static bool CheckForNet()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            int i = 0;
            if (!offline)
            {
                while (!hasinternet && i < hosts.Length)
                {
                    hasinternet = CheckForInternet(hosts[i]);
                    i++;
                }
                if (!hasinternet)
                {
                    offline = true;
                }
            }
            return hasinternet;
        }

        public static async System.Threading.Tasks.Task CheckGitHubNewerVersion(string User, string Repo, string APIBase)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var obj = getJson($"{User}/{Repo}/releases");
            GitHubClient client = new GitHubClient(new ProductHeaderValue($"{Repo}"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(User, Repo);
            string balls = releases.ToString();
            string latestGitHubVersion = releases[0].TagName.ToString().Replace("v", "");
            string localVersion = File.ReadAllText($"_bin\\{Repo}.ver").Trim();
            if (localVersion != latestGitHubVersion)
            {
                if (Directory.Exists("_bin\\Steamless"))
                {
                    Directory.Delete("_bin\\Steamless", true);
                }
                foreach (var o in obj[0])
                {
                    if (o.ToString().Contains("browser_download_url"))
                    {
                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        string ballss = StringTools.RemoveEverythingBeforeFirstRemoveString(o.Value.ToString(), "browser_download_url\": \"");
                        string ballsss = StringTools.RemoveEverythingAfterFirstRemoveString(ballss, "\"");
                        WebClient client2 = new WebClient();
                        client2.DownloadFile(ballsss, "SLS.zip");
                        ExtractFile("SLS.zip", "_bin\\Steamless");
                        File.Delete("SLS.zip");
                        File.Delete($"_bin\\{Repo}.ver");
                        File.WriteAllText($"_bin\\{Repo}.ver", latestGitHubVersion);
                    }
                }
            }
            else
            {
                return;
            }
        }

        public static void UpdateGoldBerg()
        {// Stop the process from opening a new window
            ProcessStartInfo pro = new ProcessStartInfo();
            pro.CreateNoWindow = true;
            pro.WindowStyle = ProcessWindowStyle.Hidden;
            pro.UseShellExecute = false;
            pro.WorkingDirectory = $"{Environment.CurrentDirectory}\\_bin";
            pro.FileName = $"{Environment.CurrentDirectory}\\_bin\\Gberg.bat";
            Process x = Process.Start(pro);
            x.WaitForExit();
        }
        public static void ExtractFile(string sourceArchive, string destination)
        {
            try
            {


                if (!File.Exists(Environment.CurrentDirectory + "\\7z.exe") || !File.Exists(Environment.CurrentDirectory + "\\7z.dll"))
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    WebClient client = new WebClient();
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    client.DownloadFile("https://github.com/harryeffinpotter/-Loader/raw/main/7z.exe", "7z.exe");
                    client.DownloadFile("https://github.com/harryeffinpotter/-Loader/raw/main/7z.dll", "7z.dll");
                }
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
                pro.FileName = $"{Environment.CurrentDirectory}\\7z.exe";
                pro.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", sourceArchive, destination);
                Process x = Process.Start(pro);
                if (!x.HasExited)
                    x.WaitForExit();
            }
            catch
            {
                MessageBox.Show("Unable to extract updated files. If you have WINRAR try uninstalling it then trying again! If you have not installed FFAIO since version 2.0.13 then ");
            }
        }

        public static bool CheckForInternet(string host)
        {
            bool success = false;
            Ping myPing = new Ping();
            byte[] buffer = new byte[32];
            int timeout = 60000;
            PingOptions pingOptions = new PingOptions();
            try
            {
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {
                    hasinternet = true;
                    offline = false;
                    success = true;
                    return true;
                }
            }
            catch
            {
                hasinternet = false;
                offline = true;
                return false;
            }
            if (success == true)
            {
                hasinternet = true;
                offline = false;
                return true;
            }
            offline = true;
            return false;

        }
    }
}