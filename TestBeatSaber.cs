using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

class TestBeatSaber
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🎮 BEAT SABER CS.RIN.RU SCRAPER TEST");
        Console.WriteLine("=====================================\n");

        await ScrapeGame("Beat Saber");

        Console.WriteLine("\n\nDone! Press any key to exit...");
        Console.ReadKey();
    }

    static async Task ScrapeGame(string gameName)
    {
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

                // SEARCH
                Console.WriteLine($"🔍 SEARCHING FOR: {gameName}");
                Console.WriteLine("─" * 60);

                var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords=Beat%20Saber&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
                Console.WriteLine($"URL: {searchUrl}\n");

                var html = await client.GetStringAsync(searchUrl);
                Console.WriteLine($"✅ Got {html.Length:N0} bytes from search\n");

                // Save for analysis
                System.IO.File.WriteAllText("beatsaber_search.html", html);

                // FIND ALL THREADS
                Console.WriteLine("📋 THREADS FOUND:");
                Console.WriteLine("─" * 60);

                var threadPattern = @"<a href=""(viewtopic\.php\?f=(\d+)&amp;t=(\d+)[^""]*)"" class=""topictitle"">([^<]+)</a>";
                var threads = Regex.Matches(html, threadPattern);

                var threadInfo = new List<(string url, string forumId, string threadId, string title, int replies, int maxStart)>();

                foreach (Match thread in threads)
                {
                    var url = thread.Groups[1].Value;
                    var forumId = thread.Groups[2].Value;
                    var threadId = thread.Groups[3].Value;
                    var title = thread.Groups[4].Value;

                    // Find replies
                    var replyPattern = $@"t={threadId}[^>]*>.*?Replies.*?<strong>(\d+)</strong>";
                    var replyMatch = Regex.Match(html, replyPattern, RegexOptions.Singleline);
                    int replies = replyMatch.Success ? int.Parse(replyMatch.Groups[1].Value) : 0;

                    threadInfo.Add((url, forumId, threadId, title, replies, 0));
                }

                // Display threads
                int num = 1;
                foreach (var t in threadInfo)
                {
                    string forumName = t.forumId == "10" ? "MAIN FORUM" :
                                      t.forumId == "22" ? "CONTENT SHARING" :
                                      $"Forum #{t.forumId}";

                    Console.WriteLine($"\nThread #{num}: \"{t.title}\"");
                    Console.WriteLine($"  Forum: {forumName} (f={t.forumId})");
                    Console.WriteLine($"  Thread ID: {t.threadId}");
                    Console.WriteLine($"  Replies: {t.replies}");
                    Console.WriteLine($"  Base URL: viewtopic.php?f={t.forumId}&t={t.threadId}");

                    if (t.forumId == "22")
                    {
                        Console.WriteLine($"  ⚠️  SKIPPING - This is Steam Content Sharing");
                    }
                    else if (t.forumId == "10")
                    {
                        Console.WriteLine($"  ✅ MAIN FORUM - This is what we want!");
                    }

                    num++;
                }

                // FIND ALL CLICKABLE PAGE LINKS
                Console.WriteLine("\n\n🔗 FINDING ALL PAGE LINKS IN SEARCH RESULTS:");
                Console.WriteLine("─" * 60);

                // For each Main Forum thread, find ALL its page links
                var mainThreads = threadInfo.Where(t => t.forumId == "10").ToList();

                if (mainThreads.Count == 0)
                {
                    Console.WriteLine("❌ No Main Forum threads found!");
                    return;
                }

                var bestThread = mainThreads[0]; // Take first main forum thread
                Console.WriteLine($"\nAnalyzing: \"{bestThread.title}\" (t={bestThread.threadId})");

                // Find ALL viewtopic links for this thread ID
                var allLinksPattern = $@"viewtopic\.php\?f=10&amp;t={bestThread.threadId}(&amp;hilit=[^&""]*)?(&amp;start=(\d+))?";
                var allLinks = Regex.Matches(html, allLinksPattern);

                Console.WriteLine($"\nFound {allLinks.Count} total links for this thread:");

                var startValues = new HashSet<int>();
                startValues.Add(0); // First page

                foreach (Match link in allLinks)
                {
                    var fullLink = link.Value;
                    var startGroup = link.Groups[3];

                    if (startGroup.Success)
                    {
                        int start = int.Parse(startGroup.Value);
                        startValues.Add(start);
                    }
                }

                // Sort and display all unique page positions
                var sortedStarts = startValues.OrderBy(s => s).ToList();
                Console.WriteLine($"\nUnique page positions found: {sortedStarts.Count}");
                foreach (var start in sortedStarts)
                {
                    int pageNum = (start / 15) + 1;
                    Console.WriteLine($"  Page {pageNum,3}: start={start,4}");
                }

                int maxStart = sortedStarts.Last();
                int lastPage = (maxStart / 15) + 1;

                Console.WriteLine($"\n🎯 HIGHEST START VALUE: {maxStart} (Page {lastPage})");

                // BUILD URL TO LAST PAGE
                string lastPageUrl = $"https://cs.rin.ru/forum/viewtopic.php?f=10&t={bestThread.threadId}&start={maxStart}";

                Console.WriteLine($"\n📄 URL TO LAST PAGE:");
                Console.WriteLine(lastPageUrl);

                // FETCH THE LAST PAGE
                Console.WriteLine("\n\n🔍 FETCHING LAST PAGE OF THREAD:");
                Console.WriteLine("─" * 60);

                var threadHtml = await client.GetStringAsync(lastPageUrl);
                Console.WriteLine($"✅ Got {threadHtml.Length:N0} bytes\n");

                // Save for analysis
                System.IO.File.WriteAllText($"beatsaber_lastpage.html", threadHtml);

                // Verify we're on the right page
                var pageMatch = Regex.Match(threadHtml, @"Page <strong>(\d+)</strong> of <strong>(\d+)</strong>");
                if (pageMatch.Success)
                {
                    Console.WriteLine($"✅ Confirmed: On page {pageMatch.Groups[1].Value} of {pageMatch.Groups[2].Value}");
                }

                // Find posts with clean files
                Console.WriteLine("\n📝 SCANNING POSTS FOR CLEAN FILES:");
                Console.WriteLine("─" * 60);

                var posts = Regex.Matches(threadHtml, @"<div class=""postbody"">.*?</div>\s*</div>\s*</div>", RegexOptions.Singleline);
                Console.WriteLine($"Found {posts.Count} posts on this page\n");

                string latestBuild = null;
                int postNum = 0;

                foreach (Match post in posts.Reverse()) // Check newest first (bottom of page)
                {
                    postNum++;
                    var content = post.Value;

                    // Check indicators
                    bool hasClean = Regex.IsMatch(content, @"Clean\s*(Steam\s*)?Files?", RegexOptions.IgnoreCase);
                    bool hasVersion = Regex.IsMatch(content, @"(Version:|Uploaded\s*version:)", RegexOptions.IgnoreCase);
                    bool hasLinks = content.Contains("[[Please login to see this link.]]");
                    bool hasBuild = Regex.IsMatch(content, @"Build\s*\d{5,}", RegexOptions.IgnoreCase);

                    if (hasClean || hasVersion || hasBuild)
                    {
                        Console.WriteLine($"Post #{posts.Count - postNum + 1} (from bottom):");

                        if (hasClean) Console.WriteLine("  ✓ Has 'Clean Files' text");
                        if (hasVersion) Console.WriteLine("  ✓ Has Version text");
                        if (hasBuild) Console.WriteLine("  ✓ Has Build number");
                        if (hasLinks)
                        {
                            var linkCount = Regex.Matches(content, @"\[\[Please login").Count;
                            Console.WriteLine($"  ✓ Has {linkCount} hidden links");
                        }

                        // Extract build number
                        var buildPatterns = new[] {
                            @"\[Build\s*(\d{5,})\]",
                            @"Build\s*(\d{5,})",
                            @"Build:\s*(\d{5,})",
                            @"Uploaded version:.*\[Build\s*(\d{5,})\]"
                        };

                        foreach (var pattern in buildPatterns)
                        {
                            var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                latestBuild = match.Groups[1].Value;
                                Console.WriteLine($"\n  🎯 BUILD NUMBER FOUND: {latestBuild}");
                                break;
                            }
                        }

                        // Extract text preview
                        var text = Regex.Replace(content, "<[^>]+>", " ");
                        text = Regex.Replace(text, @"\s+", " ").Trim();
                        if (text.Length > 250) text = text.Substring(0, 250) + "...";
                        Console.WriteLine($"\n  Preview: {text}\n");

                        if (latestBuild != null)
                        {
                            Console.WriteLine("─" * 60);
                            Console.WriteLine($"✅ LATEST BUILD FOR BEAT SABER: {latestBuild}");
                            Console.WriteLine("─" * 60);
                            break;
                        }
                    }
                }

                if (latestBuild == null)
                {
                    Console.WriteLine("\n❌ No build number found on last page");
                    Console.WriteLine("💡 Might need to check previous pages");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
        }
    }
}