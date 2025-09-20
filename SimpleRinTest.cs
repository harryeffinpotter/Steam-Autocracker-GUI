using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

class SimpleRinTest
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("SUPER SIMPLE RIN SCRAPER TEST");
        Console.WriteLine("=============================\n");

        string gameName = args.Length > 0 ? string.Join(" ", args) : "Barony";

        Console.WriteLine($"Testing with: {gameName}");
        Console.WriteLine("Press ENTER to start...");
        Console.ReadLine();

        await TestGame(gameName);

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static async Task TestGame(string gameName)
    {
        try
        {
            // STEP 1: SEARCH
            Console.WriteLine("\n[STEP 1] SEARCHING CS.RIN.RU");
            Console.WriteLine("=" * 50);

            var handler = new HttpClientHandler();
            var proxyUrl = "http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959";
            handler.Proxy = new WebProxy(proxyUrl);
            handler.UseProxy = true;

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.Timeout = TimeSpan.FromSeconds(30);

                // Search for the game
                var sanitized = gameName.Replace(" ", "%20");
                var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords={sanitized}&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

                Console.WriteLine($"Search URL: {searchUrl}\n");
                Console.WriteLine("Fetching...");

                var html = await client.GetStringAsync(searchUrl);
                Console.WriteLine($"Got {html.Length:N0} bytes\n");

                // Save HTML for inspection
                System.IO.File.WriteAllText($"search_{sanitized}.html", html);
                Console.WriteLine($"Saved to: search_{sanitized}.html\n");

                // FIND ALL VIEWTOPIC LINKS
                Console.WriteLine("[FINDING ALL THREAD LINKS]");
                Console.WriteLine("-" * 50);

                var allLinks = Regex.Matches(html, @"viewtopic\.php\?f=(\d+)&amp;t=(\d+)[^""]*");
                Console.WriteLine($"Total viewtopic links found: {allLinks.Count}\n");

                // Group by forum ID
                var linksByForum = new Dictionary<string, List<string>>();

                foreach (Match link in allLinks)
                {
                    var forumId = link.Groups[1].Value;
                    var fullLink = link.Value;

                    if (!linksByForum.ContainsKey(forumId))
                        linksByForum[forumId] = new List<string>();

                    linksByForum[forumId].Add(fullLink);
                }

                // Display what we found
                foreach (var forum in linksByForum.OrderBy(f => f.Key))
                {
                    string forumName = forum.Key == "10" ? "MAIN FORUM (Game Releases)" :
                                      forum.Key == "22" ? "STEAM CONTENT SHARING" :
                                      $"Forum #{forum.Key}";

                    Console.WriteLine($"Forum f={forum.Key} ({forumName}): {forum.Value.Count} links");

                    if (forum.Key == "10" || forum.Key == "22")
                    {
                        // Show first few links for important forums
                        foreach (var link in forum.Value.Take(5))
                        {
                            Console.WriteLine($"  - {link}");
                        }
                        if (forum.Value.Count > 5)
                            Console.WriteLine($"  ... and {forum.Value.Count - 5} more");
                    }
                    Console.WriteLine();
                }

                // NOW FIND THE ACTUAL THREAD TITLES AND REPLIES
                Console.WriteLine("[ANALYZING SEARCH RESULTS]");
                Console.WriteLine("-" * 50);

                var threadPattern = @"<a href=""(viewtopic\.php\?f=(\d+)&amp;t=(\d+)[^""]*)"" class=""topictitle"">([^<]+)</a>";
                var threads = Regex.Matches(html, threadPattern);

                Console.WriteLine($"Found {threads.Count} thread titles:\n");

                var threadList = new List<(string url, string forumId, string threadId, string title, int replies)>();

                foreach (Match thread in threads)
                {
                    var url = thread.Groups[1].Value;
                    var forumId = thread.Groups[2].Value;
                    var threadId = thread.Groups[3].Value;
                    var title = thread.Groups[4].Value;

                    // Try to find replies count
                    var replyPattern = $@"t={threadId}[^>]*>.*?Replies.*?<strong>(\d+)</strong>";
                    var replyMatch = Regex.Match(html, replyPattern, RegexOptions.Singleline);
                    int replies = replyMatch.Success ? int.Parse(replyMatch.Groups[1].Value) : 0;

                    threadList.Add((url, forumId, threadId, title, replies));
                }

                // Display threads sorted by importance
                int num = 1;
                foreach (var thread in threadList.OrderBy(t => t.forumId == "22" ? 1 : 0).ThenByDescending(t => t.replies))
                {
                    string marker = thread.forumId == "10" ? "[MAIN]" :
                                   thread.forumId == "22" ? "[SKIP]" : "[OTHER]";

                    Console.WriteLine($"{num}. {marker} \"{thread.title}\"");
                    Console.WriteLine($"   Forum: f={thread.forumId}, Thread: t={thread.threadId}, Replies: {thread.replies}");
                    Console.WriteLine($"   URL: {thread.url}");
                    Console.WriteLine();
                    num++;
                }

                // PICK THE BEST THREAD
                Console.WriteLine("[SELECTING BEST THREAD]");
                Console.WriteLine("-" * 50);

                var mainForumThreads = threadList.Where(t => t.forumId == "10").ToList();

                if (mainForumThreads.Count == 0)
                {
                    Console.WriteLine("❌ No Main Forum (f=10) threads found!");
                    return;
                }

                var bestThread = mainForumThreads.OrderByDescending(t => t.replies).First();
                Console.WriteLine($"✅ Selected: \"{bestThread.title}\"");
                Console.WriteLine($"   Replies: {bestThread.replies}");
                Console.WriteLine($"   Thread ID: {bestThread.threadId}\n");

                // FIND ALL PAGE LINKS FOR THIS THREAD
                Console.WriteLine("[FINDING PAGE LINKS]");
                Console.WriteLine("-" * 50);

                var pagePattern = $@"viewtopic\.php\?f=10&amp;t={bestThread.threadId}[^""]*?&amp;start=(\d+)";
                var pageLinks = Regex.Matches(html, pagePattern);

                Console.WriteLine($"Found {pageLinks.Count} page links for this thread:");

                var startValues = new List<int>();
                foreach (Match pageLink in pageLinks)
                {
                    int start = int.Parse(pageLink.Groups[1].Value);
                    startValues.Add(start);
                    int pageNum = (start / 15) + 1;
                    Console.WriteLine($"  Page {pageNum}: start={start}");
                }

                // Find the highest start value (last page)
                int maxStart = startValues.Count > 0 ? startValues.Max() : 0;

                if (maxStart > 0)
                {
                    int lastPage = (maxStart / 15) + 1;
                    Console.WriteLine($"\n📄 LAST PAGE: Page {lastPage} (start={maxStart})");
                }
                else
                {
                    Console.WriteLine("\n📄 Single page thread (no pagination found)");
                }

                // BUILD FINAL URL
                string finalUrl = $"https://cs.rin.ru/forum/{HttpUtility.HtmlDecode(bestThread.url)}";
                if (maxStart > 0)
                {
                    finalUrl += $"&start={maxStart}";
                }

                Console.WriteLine($"\n[FINAL URL TO SCRAPE]");
                Console.WriteLine("-" * 50);
                Console.WriteLine(finalUrl);

                // STEP 2: SCRAPE THE THREAD
                Console.WriteLine("\n[STEP 2] SCRAPING THREAD");
                Console.WriteLine("=" * 50);

                Console.WriteLine("Fetching thread page...");
                var threadHtml = await client.GetStringAsync(finalUrl);
                Console.WriteLine($"Got {threadHtml.Length:N0} bytes\n");

                // Save for inspection
                System.IO.File.WriteAllText($"thread_{bestThread.threadId}.html", threadHtml);

                // Check what page we're on
                var pageMatch = Regex.Match(threadHtml, @"Page <strong>(\d+)</strong> of <strong>(\d+)</strong>");
                if (pageMatch.Success)
                {
                    Console.WriteLine($"Currently on page {pageMatch.Groups[1].Value} of {pageMatch.Groups[2].Value}");
                }

                // Find posts
                var posts = Regex.Matches(threadHtml, @"<div class=""postbody"">.*?</div>\s*</div>\s*</div>", RegexOptions.Singleline);
                Console.WriteLine($"Found {posts.Count} posts on this page\n");

                Console.WriteLine("[SCANNING POSTS FOR CLEAN FILES]");
                Console.WriteLine("-" * 50);

                int postNum = 0;
                foreach (Match post in posts)
                {
                    postNum++;
                    var content = post.Value;

                    // Check for clean files indicators
                    bool hasClean = Regex.IsMatch(content, @"Clean\s*(Steam\s*)?Files?", RegexOptions.IgnoreCase);
                    bool hasVersion = Regex.IsMatch(content, @"(Version:|Uploaded\s*version:).*Build\s*\d+", RegexOptions.IgnoreCase);
                    bool hasLinks = content.Contains("[[Please login to see this link.]]");

                    if (hasClean || hasVersion || hasLinks)
                    {
                        Console.WriteLine($"\nPost #{postNum}:");
                        if (hasClean) Console.WriteLine("  ✓ Has 'Clean Files' text");
                        if (hasVersion) Console.WriteLine("  ✓ Has Version/Build format");
                        if (hasLinks)
                        {
                            var linkCount = Regex.Matches(content, @"\[\[Please login").Count;
                            Console.WriteLine($"  ✓ Has {linkCount} hidden links");
                        }

                        // Try to extract build
                        var buildMatch = Regex.Match(content, @"\[?Build\s*[:=]?\s*(\d{5,})\]?", RegexOptions.IgnoreCase);
                        if (buildMatch.Success)
                        {
                            Console.WriteLine($"  🎯 BUILD FOUND: {buildMatch.Groups[1].Value}");
                        }

                        // Show preview
                        var text = Regex.Replace(content, "<[^>]+>", " ");
                        text = Regex.Replace(text, @"\s+", " ").Trim();
                        if (text.Length > 200) text = text.Substring(0, 200) + "...";
                        Console.WriteLine($"  Preview: {text}");

                        if ((hasClean && hasLinks) || hasVersion)
                        {
                            Console.WriteLine("  ⭐ THIS IS A CLEAN FILES POST!");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}