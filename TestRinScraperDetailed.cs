using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Linq;

namespace DetailedRinTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           DETAILED CS.RIN.RU SCRAPER TEST v2.0                ║");
            Console.WriteLine("║         Testing Forum ID Filtering & Page Navigation          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

            // Test games - mix that should have different results
            string[] testGames = {
                "Barony",           // Game you mentioned
                "Beat Saber",       // Has both content sharing and main forum threads
                "Baldur's Gate 3",  // Popular game
                "Hogwarts Legacy"   // Another popular one
            };

            Console.WriteLine("Press ENTER to start testing...");
            Console.ReadLine();

            foreach (var game in testGames)
            {
                Console.WriteLine($"\n{'═' * 80}");
                Console.WriteLine($"TESTING: {game}");
                Console.WriteLine($"{'═' * 80}");

                try
                {
                    await TestGameWithDetailedOutput(game);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                }

                Console.WriteLine("\nPress ENTER to test next game...");
                Console.ReadLine();
            }

            Console.WriteLine("\n=== TEST COMPLETE ===");
            Console.ReadKey();
        }

        static async Task TestGameWithDetailedOutput(string gameName)
        {
            Console.WriteLine($"\n┌─ PHASE 1: SEARCH ─────────────────────────────────────────────┐");

            var searchResults = await SearchRinForGameAsync(gameName);

            if (searchResults.Count == 0)
            {
                Console.WriteLine("│ ❌ No threads found!                                          │");
                Console.WriteLine("└────────────────────────────────────────────────────────────────┘");
                return;
            }

            Console.WriteLine($"│ ✅ Found best thread to check                                 │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────┘");

            Console.WriteLine($"\n┌─ PHASE 2: SCRAPE THREAD ──────────────────────────────────────┐");

            var cleanInfo = await ScrapeRinThreadAsync(searchResults[0], gameName);

            if (cleanInfo != null)
            {
                Console.WriteLine($"│ ✅ FOUND CLEAN FILES!                                         │");
                Console.WriteLine($"│ Build ID: {cleanInfo.BuildId ?? "Not found",-45} │");
                Console.WriteLine($"│ Post Date: {cleanInfo.PostDate.ToString("yyyy-MM-dd"),-44} │");
                Console.WriteLine($"│ URL: {(cleanInfo.PostUrl.Length > 50 ? cleanInfo.PostUrl.Substring(0, 47) + "..." : cleanInfo.PostUrl),-50} │");
            }
            else
            {
                Console.WriteLine($"│ ❌ No clean files found in thread                             │");
            }
            Console.WriteLine("└────────────────────────────────────────────────────────────────┘");
        }

        static async Task<List<string>> SearchRinForGameAsync(string gameName)
        {
            var results = new List<string>();

            Console.WriteLine($"│ Searching for: \"{gameName}\"");

            var handler = new HttpClientHandler();
            var proxyUrl = "http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959";
            handler.Proxy = new WebProxy(proxyUrl);
            handler.UseProxy = true;

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.Timeout = TimeSpan.FromSeconds(30);

                var sanitized = SanitizeGameNameForURL(gameName);
                var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords={sanitized}&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

                Console.WriteLine($"│ Fetching search results...");
                var searchResult = await client.GetStringAsync(searchUrl);
                Console.WriteLine($"│ Response size: {searchResult.Length:N0} bytes");

                // Save for debugging
                System.IO.File.WriteAllText($"search_{sanitized}.html", searchResult);

                // Parse all threads with details
                var topicPattern = @"<a href=""(viewtopic\.php\?f=(\d+)&amp;t=(\d+)[^""]*?)"" class=""topictitle"">([^<]+)</a>";
                var topicMatches = Regex.Matches(searchResult, topicPattern);

                Console.WriteLine($"│");
                Console.WriteLine($"│ Found {topicMatches.Count} total threads:");
                Console.WriteLine($"│");

                var threads = new List<ThreadInfo>();

                foreach (Match match in topicMatches)
                {
                    var relativeUrl = match.Groups[1].Value;
                    var forumId = match.Groups[2].Value;
                    var threadId = match.Groups[3].Value;
                    var threadTitle = match.Groups[4].Value;

                    // Find reply count
                    var replyPattern = $@"t={threadId}.*?<td class=""row2""[^>]*>.*?<span[^>]*>.*?<strong>(\d+)</strong>";
                    var replyMatch = Regex.Match(searchResult, replyPattern, RegexOptions.Singleline);
                    int replies = 0;
                    if (replyMatch.Success)
                    {
                        replies = int.Parse(replyMatch.Groups[1].Value);
                    }

                    threads.Add(new ThreadInfo
                    {
                        Url = HttpUtility.HtmlDecode(relativeUrl),
                        ForumId = forumId,
                        ThreadId = threadId,
                        Title = threadTitle,
                        Replies = replies
                    });
                }

                // Display all threads found
                int index = 1;
                foreach (var thread in threads.OrderByDescending(t => t.ForumId == "10").ThenByDescending(t => t.Replies))
                {
                    string forumName = thread.ForumId == "10" ? "MAIN FORUM" :
                                      thread.ForumId == "22" ? "CONTENT SHARING" :
                                      $"Forum #{thread.ForumId}";

                    string marker = thread.ForumId == "22" ? "⚠️ " :
                                   thread.ForumId == "10" ? "✅ " : "❓ ";

                    Console.WriteLine($"│ {marker}Thread #{index++}:");
                    Console.WriteLine($"│   Title: {thread.Title}");
                    Console.WriteLine($"│   Forum: {forumName} (f={thread.ForumId})");
                    Console.WriteLine($"│   Replies: {thread.Replies}");
                    Console.WriteLine($"│   Thread ID: {thread.ThreadId}");

                    if (thread.ForumId == "22")
                    {
                        Console.WriteLine($"│   [SKIPPED - Steam Content Sharing]");
                    }
                    Console.WriteLine($"│");
                }

                // Pick the best thread (Main Forum with most replies)
                var bestThread = threads
                    .Where(t => t.ForumId == "10")
                    .OrderByDescending(t => t.Replies)
                    .FirstOrDefault();

                if (bestThread == null)
                {
                    // No main forum thread, try any non-content-sharing thread
                    bestThread = threads
                        .Where(t => t.ForumId != "22")
                        .OrderByDescending(t => t.Replies)
                        .FirstOrDefault();
                }

                if (bestThread != null)
                {
                    Console.WriteLine($"│ 🎯 SELECTED: {bestThread.Title}");
                    Console.WriteLine($"│    Forum: Main Forum (f={bestThread.ForumId})");
                    Console.WriteLine($"│    Replies: {bestThread.Replies}");

                    // Look for last page link
                    var pagePattern = $@"viewtopic\.php\?f={bestThread.ForumId}&amp;t={bestThread.ThreadId}[^""]*?&amp;start=(\d+)";
                    var pageMatches = Regex.Matches(searchResult, pagePattern);

                    int highestStart = 0;
                    foreach (Match pageMatch in pageMatches)
                    {
                        int startValue = int.Parse(pageMatch.Groups[1].Value);
                        if (startValue > highestStart)
                        {
                            highestStart = startValue;
                        }
                    }

                    string finalUrl = $"https://cs.rin.ru/forum/{bestThread.Url}";
                    if (highestStart > 0)
                    {
                        finalUrl = $"{finalUrl}&start={highestStart}";
                        int lastPage = (highestStart / 15) + 1;
                        Console.WriteLine($"│    📄 Jumping directly to page {lastPage} (start={highestStart})");
                    }
                    else
                    {
                        Console.WriteLine($"│    📄 Single page thread");
                    }

                    results.Add(finalUrl);
                }
            }

            return results;
        }

        static async Task<RinCleanInfo> ScrapeRinThreadAsync(string threadUrl, string gameName)
        {
            Console.WriteLine($"│ Fetching thread: {threadUrl}");

            var handler = new HttpClientHandler();
            var proxyUrl = "http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959";
            handler.Proxy = new WebProxy(proxyUrl);
            handler.UseProxy = true;

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.Timeout = TimeSpan.FromSeconds(30);

                var response = await client.GetStringAsync(threadUrl);
                Console.WriteLine($"│ Response size: {response.Length:N0} bytes");

                // Extract thread title
                var titleMatch = Regex.Match(response, @"<h1[^>]*>([^<]+)</h1>");
                string threadTitle = titleMatch.Success ? titleMatch.Groups[1].Value : "Unknown";
                Console.WriteLine($"│ Thread title: \"{threadTitle}\"");

                // Check what page we're on
                var currentPageMatch = Regex.Match(response, @"Page <strong>(\d+)</strong> of <strong>(\d+)</strong>");
                if (currentPageMatch.Success)
                {
                    Console.WriteLine($"│ Currently on page {currentPageMatch.Groups[1].Value} of {currentPageMatch.Groups[2].Value}");
                }

                // Look for posts
                var postDivs = Regex.Matches(response,
                    @"<div class=""postbody"">.*?</div>\s*</div>\s*</div>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);

                Console.WriteLine($"│ Found {postDivs.Count} posts on this page");
                Console.WriteLine($"│");

                int postNum = 0;
                foreach (Match postDiv in postDivs)
                {
                    postNum++;
                    var postContent = postDiv.Value;

                    // Extract poster name
                    var userMatch = Regex.Match(postContent, @"<a[^>]*><b[^>]*>([^<]+)</b></a>");
                    string poster = userMatch.Success ? userMatch.Groups[1].Value : "Unknown";

                    // Look for clean files indicators
                    bool hasCleanFiles = Regex.IsMatch(postContent,
                        @"(Clean\s*(Steam\s*)?Files?)|(\(Clean\s*Steam\s*Files\))",
                        RegexOptions.IgnoreCase);

                    bool hasVersionFormat = Regex.IsMatch(postContent,
                        @"(Version:|Uploaded\s*version:).*\d{4}.*Build\s*\d+",
                        RegexOptions.IgnoreCase);

                    bool hasHiddenLinks = postContent.Contains("[[Please login to see this link.]]") ||
                                        postContent.Contains("Please login to see this");

                    if (hasCleanFiles || hasVersionFormat || hasHiddenLinks)
                    {
                        Console.WriteLine($"│ 📝 Post #{postNum} by {poster}:");

                        if (hasCleanFiles)
                            Console.WriteLine($"│    ✓ Contains \"Clean Files\" text");
                        if (hasVersionFormat)
                            Console.WriteLine($"│    ✓ Contains version/build format");
                        if (hasHiddenLinks)
                        {
                            var linkCount = Regex.Matches(postContent, @"\[\[Please login").Count;
                            Console.WriteLine($"│    ✓ Has {linkCount} hidden link(s)");
                        }

                        // Extract text preview
                        var textOnly = Regex.Replace(postContent, "<[^>]+>", " ").Trim();
                        textOnly = Regex.Replace(textOnly, @"\s+", " ");
                        if (textOnly.Length > 150)
                            textOnly = textOnly.Substring(0, 150) + "...";
                        Console.WriteLine($"│    Preview: \"{textOnly}\"");

                        // Try to extract build number
                        string buildId = null;

                        // Try different patterns
                        var patterns = new[] {
                            @"\[?Build\s*[:=]?\s*(\d{5,})\]?",
                            @"Build\s*(\d{5,})",
                            @"Uploaded\s*version:.*\[Build\s*(\d{5,})\]",
                            @"\b(\d{7,})\b"
                        };

                        foreach (var pattern in patterns)
                        {
                            var buildMatch = Regex.Match(postContent, pattern, RegexOptions.IgnoreCase);
                            if (buildMatch.Success)
                            {
                                buildId = buildMatch.Groups[1].Value;
                                Console.WriteLine($"│    🎯 FOUND BUILD: {buildId}");
                                break;
                            }
                        }

                        if ((hasCleanFiles && hasHiddenLinks) || hasVersionFormat)
                        {
                            Console.WriteLine($"│    ⭐ THIS IS A CLEAN FILES POST!");

                            return new RinCleanInfo
                            {
                                BuildId = buildId,
                                PostDate = DateTime.Now,
                                PostUrl = threadUrl
                            };
                        }

                        Console.WriteLine($"│");
                    }
                }

                // Check if we should look at previous pages
                var prevMatch = Regex.Match(response, @"<a href=""\./?(viewtopic\.php\?[^""]+)""[^>]*>Previous</a>");
                if (prevMatch.Success)
                {
                    Console.WriteLine($"│ 📌 Previous page available - would check if needed");
                }
            }

            return null;
        }

        static string SanitizeGameNameForURL(string gameName)
        {
            var result = gameName.Replace(" ", "%20");
            var charsToRemove = new[] { '-', ':', '_', '&', '!', '?', ',', '.', '(', ')', '$', '#', '%', '+', ';', '\'', '`', '"' };
            foreach (var c in charsToRemove)
            {
                result = result.Replace(c.ToString(), "");
            }
            return result;
        }

        class ThreadInfo
        {
            public string Url { get; set; }
            public string ForumId { get; set; }
            public string ThreadId { get; set; }
            public string Title { get; set; }
            public int Replies { get; set; }
        }

        class RinCleanInfo
        {
            public string BuildId { get; set; }
            public DateTime PostDate { get; set; }
            public string PostUrl { get; set; }
        }
    }
}