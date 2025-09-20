using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

class QuickTestComparison
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("QUICK BUILD COMPARISON TEST");
        Console.WriteLine("===========================\n");

        // Find a few installed games to test
        var steamPaths = GetSteamLibraryPaths();
        var manifests = new List<string>();

        foreach (var path in steamPaths)
        {
            if (Directory.Exists(path))
            {
                manifests.AddRange(Directory.GetFiles(path, "appmanifest_*.acf"));
            }
        }

        Console.WriteLine($"Found {manifests.Count} installed games\n");

        // Test first 3 games
        int tested = 0;
        foreach (var manifest in manifests.Take(5))
        {
            var game = ParseManifest(manifest);
            if (game == null) continue;

            tested++;
            Console.WriteLine($"\n[{tested}] {game.Name}");
            Console.WriteLine($"    Your Build: {game.BuildId}");
            Console.Write($"    RIN Build:  ");

            // Quick scrape for this game
            var rinBuild = await QuickScrapeRin(game.Name);

            if (rinBuild != null)
            {
                Console.WriteLine(rinBuild);

                // Compare
                if (long.TryParse(game.BuildId, out long local) && long.TryParse(rinBuild, out long rin))
                {
                    if (local > rin)
                        Console.WriteLine("    ✅ YOU HAVE NEWER - SHARE IT!");
                    else if (local == rin)
                        Console.WriteLine("    🔵 Same version");
                    else
                        Console.WriteLine("    ⚠️ RIN has newer");
                }
            }
            else
            {
                Console.WriteLine("NOT FOUND");
                Console.WriteLine("    🔴 RIN NEEDS YOUR FILES!");
            }

            if (tested >= 3) break;
        }

        Console.WriteLine("\n\nPress any key to exit...");
        Console.ReadKey();
    }

    static List<string> GetSteamLibraryPaths()
    {
        var paths = new List<string>();

        // Common Steam locations
        var commonPaths = new[]
        {
            @"C:\Program Files (x86)\Steam\steamapps",
            @"C:\Program Files\Steam\steamapps",
            @"D:\Steam\steamapps",
            @"D:\SteamLibrary\steamapps",
            @"E:\Steam\steamapps",
            @"E:\SteamLibrary\steamapps"
        };

        paths.AddRange(commonPaths.Where(Directory.Exists));

        // Check for additional libraries
        var mainSteamPath = commonPaths.FirstOrDefault(Directory.Exists);
        if (mainSteamPath != null)
        {
            var vdfPath = Path.Combine(mainSteamPath, "libraryfolders.vdf");
            if (File.Exists(vdfPath))
            {
                var vdfContent = File.ReadAllText(vdfPath);
                var pathMatches = Regex.Matches(vdfContent, @"""path""\s+""([^""]+)""");
                foreach (Match match in pathMatches)
                {
                    var libPath = Path.Combine(match.Groups[1].Value.Replace(@"\\", @"\"), "steamapps");
                    if (Directory.Exists(libPath) && !paths.Contains(libPath))
                        paths.Add(libPath);
                }
            }
        }

        return paths;
    }

    static GameInfo ParseManifest(string manifestPath)
    {
        try
        {
            var content = File.ReadAllText(manifestPath);

            var appIdMatch = Regex.Match(content, @"""appid""\s+""(\d+)""");
            var nameMatch = Regex.Match(content, @"""name""\s+""([^""]+)""");
            var buildIdMatch = Regex.Match(content, @"""buildid""\s+""([^""]+)""");

            if (!appIdMatch.Success || !nameMatch.Success) return null;

            return new GameInfo
            {
                AppId = appIdMatch.Groups[1].Value,
                Name = nameMatch.Groups[1].Value,
                BuildId = buildIdMatch.Success ? buildIdMatch.Groups[1].Value : "Unknown"
            };
        }
        catch
        {
            return null;
        }
    }

    static async Task<string> QuickScrapeRin(string gameName)
    {
        try
        {
            var handler = new HttpClientHandler();
            var proxyUrl = "http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959";
            handler.Proxy = new WebProxy(proxyUrl);
            handler.UseProxy = true;

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                client.Timeout = TimeSpan.FromSeconds(10);

                // Search
                var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords={gameName.Replace(" ", "%20")}&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
                var searchHtml = await client.GetStringAsync(searchUrl);

                // Find Main Forum thread
                var threadPattern = @"viewtopic\.php\?f=10&amp;t=(\d+)";
                var threadMatch = Regex.Match(searchHtml, threadPattern);

                if (!threadMatch.Success) return null;

                var threadId = threadMatch.Groups[1].Value;

                // Find highest start value for last page
                var pagePattern = $@"viewtopic\.php\?f=10&amp;t={threadId}[^""]*?&amp;start=(\d+)";
                var pageMatches = Regex.Matches(searchHtml, pagePattern);

                int maxStart = 0;
                foreach (Match m in pageMatches)
                {
                    int start = int.Parse(m.Groups[1].Value);
                    if (start > maxStart) maxStart = start;
                }

                // Go to last page
                var threadUrl = $"https://cs.rin.ru/forum/viewtopic.php?f=10&t={threadId}&start={maxStart}";
                var threadHtml = await client.GetStringAsync(threadUrl);

                // Quick scan for build number
                var buildMatch = Regex.Match(threadHtml, @"\[?Build\s*[:=]?\s*(\d{5,})\]?", RegexOptions.IgnoreCase);

                if (buildMatch.Success)
                    return buildMatch.Groups[1].Value;

                return null;
            }
        }
        catch
        {
            return null;
        }
    }

    class GameInfo
    {
        public string AppId { get; set; }
        public string Name { get; set; }
        public string BuildId { get; set; }
    }
}