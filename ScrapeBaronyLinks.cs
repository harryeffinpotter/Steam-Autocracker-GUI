using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class ScrapeBaronyLinks
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("SCRAPING ALL LINKS FROM BARONY SEARCH PAGE");
        Console.WriteLine("===========================================\n");

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

                var searchUrl = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

                Console.WriteLine("Fetching search page...");
                var html = await client.GetStringAsync(searchUrl);
                Console.WriteLine($"✅ Got {html.Length:N0} bytes\n");

                // Save for inspection
                System.IO.File.WriteAllText("barony_search.html", html);
                Console.WriteLine("Saved to: barony_search.html\n");

                // 1. FIND ALL VIEWTOPIC LINKS
                Console.WriteLine("═══════════════════════════════════════════════════════");
                Console.WriteLine("ALL VIEWTOPIC.PHP LINKS:");
                Console.WriteLine("═══════════════════════════════════════════════════════\n");

                var allViewtopicLinks = Regex.Matches(html, @"viewtopic\.php\?([^""'\s>]+)");
                var uniqueLinks = new HashSet<string>();

                foreach (Match m in allViewtopicLinks)
                {
                    uniqueLinks.Add(m.Value);
                }

                var linksByType = new Dictionary<string, List<string>>();

                foreach (var link in uniqueLinks)
                {
                    // Extract forum ID
                    var forumMatch = Regex.Match(link, @"f=(\d+)");
                    var threadMatch = Regex.Match(link, @"t=(\d+)");
                    var startMatch = Regex.Match(link, @"start=(\d+)");

                    if (forumMatch.Success)
                    {
                        string forumId = forumMatch.Groups[1].Value;
                        string key = forumId == "10" ? "MAIN FORUM (f=10)" :
                                    forumId == "22" ? "CONTENT SHARING (f=22)" :
                                    $"OTHER FORUM (f={forumId})";

                        if (!linksByType.ContainsKey(key))
                            linksByType[key] = new List<string>();

                        linksByType[key].Add(link);
                    }
                }

                // Display organized by forum
                foreach (var kvp in linksByType.OrderBy(k => k.Key))
                {
                    Console.WriteLine($"\n{kvp.Key}:");
                    Console.WriteLine(new string('-', 60));

                    foreach (var link in kvp.Value)
                    {
                        var threadMatch = Regex.Match(link, @"t=(\d+)");
                        var startMatch = Regex.Match(link, @"start=(\d+)");

                        string info = "";
                        if (threadMatch.Success)
                            info += $"Thread: {threadMatch.Groups[1].Value}";
                        if (startMatch.Success)
                        {
                            int start = int.Parse(startMatch.Groups[1].Value);
                            int page = (start / 15) + 1;
                            info += $", Page {page} (start={start})";
                        }

                        Console.WriteLine($"  {link}");
                        if (!string.IsNullOrEmpty(info))
                            Console.WriteLine($"    ({info})");
                    }
                }

                // 2. FIND THREAD TITLES
                Console.WriteLine("\n\n═══════════════════════════════════════════════════════");
                Console.WriteLine("THREAD TITLES WITH THEIR LINKS:");
                Console.WriteLine("═══════════════════════════════════════════════════════\n");

                var threadTitles = Regex.Matches(html, @"<a href=""(viewtopic\.php\?[^""]+)"" class=""topictitle"">([^<]+)</a>");

                int num = 1;
                foreach (Match m in threadTitles)
                {
                    var url = m.Groups[1].Value;
                    var title = m.Groups[2].Value;

                    var forumMatch = Regex.Match(url, @"f=(\d+)");
                    var threadMatch = Regex.Match(url, @"t=(\d+)");

                    string forumType = "";
                    if (forumMatch.Success)
                    {
                        string fid = forumMatch.Groups[1].Value;
                        forumType = fid == "10" ? "[MAIN]" :
                                   fid == "22" ? "[SKIP-CONTENT-SHARING]" :
                                   $"[f={fid}]";
                    }

                    Console.WriteLine($"{num}. {forumType} \"{title}\"");
                    Console.WriteLine($"   URL: {url}");

                    if (threadMatch.Success)
                        Console.WriteLine($"   Thread ID: {threadMatch.Groups[1].Value}");

                    Console.WriteLine();
                    num++;
                }

                // 3. FIND HIGHEST START VALUE
                Console.WriteLine("═══════════════════════════════════════════════════════");
                Console.WriteLine("FINDING LAST PAGE FOR MAIN FORUM THREAD:");
                Console.WriteLine("═══════════════════════════════════════════════════════\n");

                // Find first Main Forum thread
                var mainForumThread = Regex.Match(html, @"viewtopic\.php\?f=10&amp;t=(\d+)");
                if (mainForumThread.Success)
                {
                    var threadId = mainForumThread.Groups[1].Value;
                    Console.WriteLine($"Main Forum Thread ID: {threadId}");

                    // Find all start values for this thread
                    var pattern = $@"viewtopic\.php\?f=10&amp;t={threadId}[^""]*?(?:&amp;start=(\d+))?";
                    var matches = Regex.Matches(html, pattern);

                    var startValues = new HashSet<int> { 0 };
                    foreach (Match m in matches)
                    {
                        if (m.Groups[1].Success)
                        {
                            startValues.Add(int.Parse(m.Groups[1].Value));
                        }
                    }

                    Console.WriteLine($"\nFound {startValues.Count} different page positions:");
                    foreach (var start in startValues.OrderBy(s => s))
                    {
                        int page = (start / 15) + 1;
                        Console.WriteLine($"  Page {page}: start={start}");
                    }

                    int maxStart = startValues.Max();
                    Console.WriteLine($"\n🎯 LAST PAGE: start={maxStart} (Page {(maxStart/15)+1})");
                    Console.WriteLine($"\nDirect URL to last page:");
                    Console.WriteLine($"https://cs.rin.ru/forum/viewtopic.php?f=10&t={threadId}&start={maxStart}");
                }
                else
                {
                    Console.WriteLine("❌ No Main Forum (f=10) thread found!");
                }

                // 4. RAW LINK DUMP
                Console.WriteLine("\n\n═══════════════════════════════════════════════════════");
                Console.WriteLine("RAW HREF DUMP (first 20):");
                Console.WriteLine("═══════════════════════════════════════════════════════\n");

                var allHrefs = Regex.Matches(html, @"href=""([^""]+)""");
                int count = 0;
                foreach (Match m in allHrefs)
                {
                    if (m.Groups[1].Value.Contains("viewtopic"))
                    {
                        Console.WriteLine(m.Groups[1].Value);
                        count++;
                        if (count >= 20) break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
        }

        Console.WriteLine("\n\nPress any key to exit...");
        Console.ReadKey();
    }
}