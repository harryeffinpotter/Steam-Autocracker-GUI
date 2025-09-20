using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class TestBasicScrape
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("BASIC SCRAPE TEST - Can we even get text from CS.RIN.RU?");
        Console.WriteLine("=========================================================\n");

        try
        {
            var handler = new HttpClientHandler();
            var proxyUrl = "http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959";
            handler.Proxy = new WebProxy(proxyUrl);
            handler.UseProxy = true;

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.Timeout = TimeSpan.FromSeconds(30);

                // TEST 1: Can we get search results?
                Console.WriteLine("[TEST 1] Searching for 'Beat Saber'...");
                var searchUrl = "https://cs.rin.ru/forum/search.php?keywords=Beat+Saber&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

                var html = await client.GetStringAsync(searchUrl);
                Console.WriteLine($"✅ Got {html.Length} bytes\n");

                // TEST 2: Can we find ANY thread titles?
                Console.WriteLine("[TEST 2] Finding thread titles...");
                var titles = Regex.Matches(html, @"class=""topictitle"">([^<]+)</a>");
                Console.WriteLine($"Found {titles.Count} thread titles:");
                foreach (Match m in titles)
                {
                    Console.WriteLine($"  - {m.Groups[1].Value}");
                }

                // TEST 3: Can we find forum IDs?
                Console.WriteLine("\n[TEST 3] Finding forum IDs (f=10 is Main, f=22 is Content Sharing)...");
                var forums = Regex.Matches(html, @"viewtopic\.php\?f=(\d+)&amp;t=(\d+)");
                var f10Count = 0;
                var f22Count = 0;
                foreach (Match m in forums)
                {
                    if (m.Groups[1].Value == "10") f10Count++;
                    if (m.Groups[1].Value == "22") f22Count++;
                }
                Console.WriteLine($"  Main Forum (f=10): {f10Count} links");
                Console.WriteLine($"  Content Sharing (f=22): {f22Count} links");

                // TEST 4: Can we find page navigation?
                Console.WriteLine("\n[TEST 4] Finding page navigation links...");
                var pageLinks = Regex.Matches(html, @"&amp;start=(\d+)");
                Console.WriteLine($"Found {pageLinks.Count} page links:");
                foreach (Match m in pageLinks)
                {
                    int start = int.Parse(m.Groups[1].Value);
                    int page = (start / 15) + 1;
                    Console.WriteLine($"  Page {page}: start={start}");
                }

                // TEST 5: Pick first Main Forum thread and try to get it
                Console.WriteLine("\n[TEST 5] Getting first Main Forum thread...");
                var mainThread = Regex.Match(html, @"viewtopic\.php\?f=10&amp;t=(\d+)");
                if (mainThread.Success)
                {
                    var threadId = mainThread.Groups[1].Value;
                    var threadUrl = $"https://cs.rin.ru/forum/viewtopic.php?f=10&t={threadId}";
                    Console.WriteLine($"Thread URL: {threadUrl}");

                    var threadHtml = await client.GetStringAsync(threadUrl);
                    Console.WriteLine($"✅ Got {threadHtml.Length} bytes\n");

                    // Can we find posts?
                    var posts = Regex.Matches(threadHtml, @"<div class=""postbody"">");
                    Console.WriteLine($"Found {posts.Count} posts");

                    // Can we find "Clean Files" text?
                    var cleanMatches = Regex.Matches(threadHtml, @"Clean\s*(Steam\s*)?Files?", RegexOptions.IgnoreCase);
                    Console.WriteLine($"Found {cleanMatches.Count} mentions of 'Clean Files'");

                    // Can we find Build numbers?
                    var buildMatches = Regex.Matches(threadHtml, @"Build\s*(\d{5,})", RegexOptions.IgnoreCase);
                    Console.WriteLine($"Found {buildMatches.Count} build numbers:");
                    foreach (Match m in buildMatches)
                    {
                        Console.WriteLine($"  - Build {m.Groups[1].Value}");
                    }
                }
                else
                {
                    Console.WriteLine("❌ No Main Forum thread found!");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}