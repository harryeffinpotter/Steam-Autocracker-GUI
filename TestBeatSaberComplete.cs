using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

class TestBeatSaberComplete
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🎮 BEAT SABER COMPLETE SCRAPER TEST");
        Console.WriteLine("====================================\n");
        Console.WriteLine("This will:");
        Console.WriteLine("1. Search for Beat Saber");
        Console.WriteLine("2. Find the Main Forum thread (f=10)");
        Console.WriteLine("3. Get the highest &start= value from search page");
        Console.WriteLine("4. Go to that page");
        Console.WriteLine("5. Look for Clean Files");
        Console.WriteLine("6. If not found, click Previous and repeat\n");

        Console.WriteLine("Press ENTER to start...");
        Console.ReadLine();

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

                // ==========================================
                // STEP 1: SEARCH
                // ==========================================
                Console.WriteLine($"\n{'='*70}");
                Console.WriteLine($"STEP 1: SEARCHING FOR {gameName}");
                Console.WriteLine($"{'='*70}\n");

                var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords=Beat%20Saber&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

                Console.WriteLine("Fetching search results...");
                var html = await client.GetStringAsync(searchUrl);
                Console.WriteLine($"✅ Got {html.Length:N0} bytes\n");

                // Find all threads
                var threadPattern = @"<a href=""(viewtopic\.php\?f=(\d+)&amp;t=(\d+)[^""]*)"" class=""topictitle"">([^<]+)</a>";
                var threads = Regex.Matches(html, threadPattern);

                Console.WriteLine($"Found {threads.Count} threads:\n");

                string mainForumThreadId = null;
                string mainForumTitle = null;

                foreach (Match thread in threads)
                {
                    var forumId = thread.Groups[2].Value;
                    var threadId = thread.Groups[3].Value;
                    var title = thread.Groups[4].Value;

                    string status = forumId == "10" ? "✅ MAIN FORUM" :
                                   forumId == "22" ? "❌ CONTENT SHARING" :
                                   "❓ OTHER";

                    Console.WriteLine($"{status} - \"{title}\" (f={forumId}, t={threadId})");

                    if (forumId == "10" && mainForumThreadId == null)
                    {
                        mainForumThreadId = threadId;
                        mainForumTitle = title;
                    }
                }

                if (mainForumThreadId == null)
                {
                    Console.WriteLine("\n❌ No Main Forum thread found!");
                    return;
                }

                Console.WriteLine($"\n🎯 Selected Main Forum thread: \"{mainForumTitle}\" (t={mainForumThreadId})");

                // ==========================================
                // STEP 2: FIND ALL PAGE LINKS
                // ==========================================
                Console.WriteLine($"\n{'='*70}");
                Console.WriteLine("STEP 2: FINDING ALL PAGE LINKS IN SEARCH RESULTS");
                Console.WriteLine($"{'='*70}\n");

                // Find ALL links for this thread to get page positions
                var pagePattern = $@"viewtopic\.php\?f=10&amp;t={mainForumThreadId}[^""]*?(&amp;start=(\d+))?";
                var pageLinks = Regex.Matches(html, pagePattern);

                var startValues = new HashSet<int> { 0 }; // Always include page 1 (start=0)

                foreach (Match link in pageLinks)
                {
                    if (link.Groups[2].Success)
                    {
                        startValues.Add(int.Parse(link.Groups[2].Value));
                    }
                }

                var sortedStarts = startValues.OrderBy(s => s).ToList();

                Console.WriteLine($"Found {sortedStarts.Count} unique page positions:");
                foreach (var start in sortedStarts)
                {
                    int page = (start / 15) + 1;
                    Console.WriteLine($"  Page {page}: start={start}");
                }

                int maxStart = sortedStarts.Last();
                int lastPage = (maxStart / 15) + 1;

                Console.WriteLine($"\n🎯 HIGHEST START VALUE: {maxStart} (Page {lastPage})");

                // ==========================================
                // STEP 3: GO TO LAST PAGE AND SEARCH
                // ==========================================
                Console.WriteLine($"\n{'='*70}");
                Console.WriteLine("STEP 3: NAVIGATING TO LAST PAGE AND SEARCHING");
                Console.WriteLine($"{'='*70}\n");

                string currentUrl = $"https://cs.rin.ru/forum/viewtopic.php?f=10&t={mainForumThreadId}&start={maxStart}";
                int pagesChecked = 0;
                int maxPagesToCheck = 10; // Don't check more than 10 pages back
                bool foundCleanFiles = false;
                string latestBuild = null;

                while (pagesChecked < maxPagesToCheck && !foundCleanFiles)
                {
                    pagesChecked++;

                    Console.WriteLine($"\n[PAGE CHECK #{pagesChecked}]");
                    Console.WriteLine($"URL: {currentUrl}");
                    Console.WriteLine("Fetching page...");

                    var pageHtml = await client.GetStringAsync(currentUrl);
                    Console.WriteLine($"✅ Got {pageHtml.Length:N0} bytes");

                    // Confirm what page we're on
                    var pageMatch = Regex.Match(pageHtml, @"Page <strong>(\d+)</strong> of <strong>(\d+)</strong>");
                    if (pageMatch.Success)
                    {
                        Console.WriteLine($"📄 On page {pageMatch.Groups[1].Value} of {pageMatch.Groups[2].Value}");
                    }

                    // Find all posts
                    var posts = Regex.Matches(pageHtml, @"<div class=""postbody"">.*?</div>\s*</div>\s*</div>", RegexOptions.Singleline);
                    Console.WriteLine($"Found {posts.Count} posts on this page");

                    // Scan posts from bottom up (newest first)
                    Console.WriteLine("\nScanning for Clean Files posts...");

                    int postNum = 0;
                    foreach (Match post in posts.Cast<Match>().Reverse())
                    {
                        postNum++;
                        var content = post.Value;

                        // Check all indicators
                        bool hasClean = Regex.IsMatch(content, @"Clean\s*(Steam\s*)?Files?", RegexOptions.IgnoreCase);
                        bool hasVersion = Regex.IsMatch(content, @"(Version:|Uploaded\s*version:)", RegexOptions.IgnoreCase);
                        bool hasBuild = Regex.IsMatch(content, @"Build\s*\d{5,}", RegexOptions.IgnoreCase);
                        bool hasLinks = content.Contains("[[Please login to see this link.]]");

                        if (hasClean || hasVersion || hasBuild)
                        {
                            Console.WriteLine($"\n  📝 Post #{posts.Count - postNum + 1}:");
                            if (hasClean) Console.WriteLine("    ✓ Has 'Clean Files'");
                            if (hasVersion) Console.WriteLine("    ✓ Has Version info");
                            if (hasBuild) Console.WriteLine("    ✓ Has Build number");
                            if (hasLinks) Console.WriteLine($"    ✓ Has hidden links");

                            // Try to extract build
                            var buildPatterns = new[] {
                                @"\[Build\s*(\d{5,})\]",
                                @"Build\s*(\d{5,})",
                                @"Build:\s*(\d{5,})",
                                @"Uploaded\s*version:.*\[Build\s*(\d{5,})\]",
                                @"Build\s*(\d{8,})" // Sometimes builds are 8+ digits
                            };

                            foreach (var pattern in buildPatterns)
                            {
                                var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
                                if (match.Success)
                                {
                                    latestBuild = match.Groups[1].Value;
                                    Console.WriteLine($"\n    🎯 BUILD FOUND: {latestBuild}");
                                    foundCleanFiles = true;
                                    break;
                                }
                            }

                            if (foundCleanFiles)
                            {
                                // Show a preview of what we found
                                var text = Regex.Replace(content, "<[^>]+>", " ");
                                text = Regex.Replace(text, @"\s+", " ").Trim();
                                if (text.Length > 200) text = text.Substring(0, 200) + "...";
                                Console.WriteLine($"\n    Preview: {text}");
                                break;
                            }
                        }
                    }

                    if (!foundCleanFiles)
                    {
                        Console.WriteLine("\n❌ No Clean Files found on this page");

                        // Look for Previous link
                        var prevMatch = Regex.Match(pageHtml, @"<a href=""\./?(viewtopic\.php\?[^""]+)""[^>]*>Previous</a>");

                        if (prevMatch.Success && pagesChecked < maxPagesToCheck)
                        {
                            var prevUrl = HttpUtility.HtmlDecode(prevMatch.Groups[1].Value);
                            currentUrl = $"https://cs.rin.ru/forum/{prevUrl}";
                            Console.WriteLine("⬅️  Clicking 'Previous' to go to earlier page...");
                        }
                        else if (!prevMatch.Success)
                        {
                            Console.WriteLine("❌ No 'Previous' link found - we're at the beginning");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"⚠️  Reached max pages to check ({maxPagesToCheck})");
                            break;
                        }
                    }
                }

                // ==========================================
                // RESULTS
                // ==========================================
                Console.WriteLine($"\n{'='*70}");
                Console.WriteLine("FINAL RESULTS");
                Console.WriteLine($"{'='*70}\n");

                if (foundCleanFiles && latestBuild != null)
                {
                    Console.WriteLine($"✅ SUCCESS!");
                    Console.WriteLine($"   Game: {gameName}");
                    Console.WriteLine($"   Latest Build on RIN: {latestBuild}");
                    Console.WriteLine($"   Pages checked: {pagesChecked}");
                }
                else
                {
                    Console.WriteLine($"❌ NO CLEAN FILES FOUND");
                    Console.WriteLine($"   Checked {pagesChecked} pages");
                    Console.WriteLine($"   Might need to check more pages or thread might not have clean files");
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