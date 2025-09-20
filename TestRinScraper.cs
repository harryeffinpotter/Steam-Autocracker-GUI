using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestScraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== RIN SCRAPER TEST ===\n");
            Console.WriteLine("Set breakpoints now if needed!");
            Console.WriteLine("Press ENTER to start testing...");
            Console.ReadLine();

            // Test games - mix of released and unreleased
            string[] testGames = {
                "Hollow Knight Silksong",  // Not released - should find nothing
                "Hogwarts Legacy",          // Popular game - should have clean files
                "Baldur's Gate 3",          // Popular game - should have clean files
                "Half Life 3"               // Doesn't exist - should find nothing
            };

            bool stepThrough = true;
            if (stepThrough)
            {
                Console.WriteLine("\n[DEBUG MODE] Will pause after each game");
                Console.WriteLine("You can set breakpoints at any Console.WriteLine(\"BREAKPOINT:\")");
            }

            foreach (var game in testGames)
            {
                Console.WriteLine($"\n{'═' * 60}");
                Console.WriteLine($"TESTING: {game}");
                Console.WriteLine($"{'═' * 60}");

                await TestGameScrape(game);

                if (stepThrough)
                {
                    Console.WriteLine("\nPress ENTER to continue to next game...");
                    Console.ReadLine();
                }
            }

            Console.WriteLine("\n=== TEST COMPLETE ===");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task TestGameScrape(string gameName)
        {
            try
            {
                // Step 1: Search for the game
                Console.WriteLine($"\n[1] Searching cs.rin.ru for '{gameName}'...");
                var threadUrl = await SearchRinForGameAsync(gameName);

                if (string.IsNullOrEmpty(threadUrl))
                {
                    Console.WriteLine("   ❌ No thread found on cs.rin.ru");
                    Console.WriteLine($"   STATUS: 🚨 NEEDED! (No clean files exist)");
                    return;
                }

                Console.WriteLine($"   ✅ Found thread: {threadUrl}");

                // Step 2: Scrape the thread
                Console.WriteLine($"\n[2] Scraping thread for Clean Files posts...");
                var cleanInfo = await ScrapeRinThreadAsync(threadUrl, gameName);

                if (cleanInfo == null)
                {
                    Console.WriteLine("   ❌ No Clean Files posts found");
                    Console.WriteLine($"   STATUS: 🚨 NEEDED! (No clean files in thread)");
                }
                else
                {
                    Console.WriteLine($"   ✅ Found Clean Files!");
                    Console.WriteLine($"   Build ID: {cleanInfo.BuildId ?? "Unknown"}");
                    Console.WriteLine($"   Post Date: {cleanInfo.PostDate}");
                    Console.WriteLine($"   Post URL: {cleanInfo.PostUrl}");

                    // Compare with local build (simulate)
                    string localBuild = "12345678"; // Simulated local build
                    Console.WriteLine($"\n[3] Comparing builds:");
                    Console.WriteLine($"   Local Build: {localBuild}");
                    Console.WriteLine($"   RIN Build: {cleanInfo.BuildId ?? "Unknown"}");

                    if (string.IsNullOrEmpty(cleanInfo.BuildId))
                    {
                        Console.WriteLine($"   STATUS: 📋 Has Clean (can't compare versions)");
                    }
                    else if (long.TryParse(localBuild, out long local) && long.TryParse(cleanInfo.BuildId, out long rin))
                    {
                        if (local > rin)
                            Console.WriteLine($"   STATUS: 💎 NEWER! (Share your files!)");
                        else if (local == rin)
                            Console.WriteLine($"   STATUS: ✅ Current");
                        else
                            Console.WriteLine($"   STATUS: 📦 Old (RIN has newer)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️ ERROR: {ex.Message}");
            }
        }

        static async Task<string> SearchRinForGameAsync(string gameName)
        {
            try
            {
                Console.WriteLine("BREAKPOINT: About to create HttpClient with proxy");
                var handler = new HttpClientHandler();

                // Use proxy to avoid rate limiting
                Console.WriteLine("   Using Thunder proxy for search...");
                var proxyUrl = "http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959";
                handler.Proxy = new WebProxy(proxyUrl);
                handler.UseProxy = true;

                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                    var sanitized = SanitizeGameNameForURL(gameName);
                    var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords={sanitized}&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

                    Console.WriteLine($"   Search URL: {searchUrl}");

                    Console.WriteLine("BREAKPOINT: About to make HTTP request to cs.rin.ru");
                    var searchResult = await client.GetStringAsync(searchUrl);
                    Console.WriteLine($"   Response size: {searchResult.Length} bytes");

                    Console.WriteLine("BREAKPOINT: Parsing search results HTML");

                    // Save HTML for debugging if needed
                    System.IO.File.WriteAllText($"rin_search_{sanitized}.html", searchResult);
                    Console.WriteLine($"   Saved HTML to: rin_search_{sanitized}.html");

                    // Extract first topic URL from search results
                    var topicMatch = Regex.Match(searchResult, @"<a href=""(viewtopic\.php\?[^""]+)"" class=""topictitle""");
                    if (topicMatch.Success)
                    {
                        Console.WriteLine("BREAKPOINT: Found matching thread!");
                        return $"https://cs.rin.ru/forum/{topicMatch.Groups[1].Value}";
                    }
                    else
                    {
                        Console.WriteLine("BREAKPOINT: No thread match found in HTML");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Search error: {ex.Message}");
            }
            return null;
        }

        static async Task<RinCleanInfo> ScrapeRinThreadAsync(string threadUrl, string gameName)
        {
            try
            {
                var handler = new HttpClientHandler();

                // Use proxy for scraping too
                Console.WriteLine("   Using Thunder proxy for scraping...");
                var proxyUrl = "http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959";
                handler.Proxy = new WebProxy(proxyUrl);
                handler.UseProxy = true;

                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                    // First, get the thread and find total pages
                    Console.WriteLine($"   Fetching thread: {threadUrl}");
                    var response = await client.GetStringAsync(threadUrl);

                    // Extract page count from pagination
                    var pageMatch = Regex.Match(response, @"Page \d+ of (\d+)");
                    int totalPages = 1;
                    if (pageMatch.Success)
                    {
                        totalPages = int.Parse(pageMatch.Groups[1].Value);
                    }
                    Console.WriteLine($"   Total pages in thread: {totalPages}");

                    // Start from last page and work backwards
                    for (int page = totalPages; page > Math.Max(1, totalPages - 5); page--) // Check last 5 pages
                    {
                        Console.WriteLine($"   Checking page {page}/{totalPages}...");

                        string pageUrl = threadUrl;
                        if (page > 1)
                        {
                            var startIndex = (page - 1) * 15;
                            pageUrl = threadUrl.Contains("?")
                                ? $"{threadUrl}&start={startIndex}"
                                : $"{threadUrl}?start={startIndex}";
                        }

                        var pageContent = await client.GetStringAsync(pageUrl);

                        // Look for posts containing "Clean Files"
                        var postDivs = Regex.Matches(pageContent,
                            @"<div class=""postbody"">.*?</div>\s*</div>\s*</div>",
                            RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        Console.WriteLine($"      Found {postDivs.Count} posts");

                        foreach (Match postDiv in postDivs)
                        {
                            var postContent = postDiv.Value;

                            // Check if post contains "Clean Files"
                            bool hasCleanFiles = Regex.IsMatch(postContent,
                                @"Clean\s*Files?",
                                RegexOptions.IgnoreCase);

                            // Check for hidden links
                            bool hasHiddenLinks = postContent.Contains("[[Please login to see this link.]]") ||
                                                postContent.Contains("Please login to see this") ||
                                                postContent.Contains("login to see");

                            if (hasCleanFiles)
                            {
                                Console.WriteLine($"      Found post with 'Clean Files'!");
                                Console.WriteLine($"      Has hidden links: {hasHiddenLinks}");
                            }

                            if (hasCleanFiles && hasHiddenLinks)
                            {
                                string buildId = null;

                                // Try to extract build number
                                var buildMatch = Regex.Match(postContent,
                                    @"Build\s*[:=]?\s*(\d{5,})",
                                    RegexOptions.IgnoreCase);

                                if (buildMatch.Success)
                                {
                                    buildId = buildMatch.Groups[1].Value;
                                    Console.WriteLine($"      Extracted Build ID: {buildId}");
                                }
                                else
                                {
                                    // Try other patterns
                                    buildMatch = Regex.Match(postContent, @"\b(\d{7,})\b");
                                    if (buildMatch.Success)
                                    {
                                        buildId = buildMatch.Groups[1].Value;
                                        Console.WriteLine($"      Extracted Build ID (fallback): {buildId}");
                                    }
                                }

                                // Extract post date
                                var dateMatch = Regex.Match(postContent,
                                    @"Posted:\s*</span>\s*<span[^>]*>([^<]+)</span>",
                                    RegexOptions.IgnoreCase);

                                DateTime postDate = DateTime.Now;
                                if (dateMatch.Success)
                                {
                                    string dateStr = dateMatch.Groups[1].Value.Trim();
                                    if (DateTime.TryParse(dateStr, out postDate))
                                    {
                                        Console.WriteLine($"      Post date: {postDate}");
                                    }
                                }

                                return new RinCleanInfo
                                {
                                    BuildId = buildId,
                                    PostDate = postDate,
                                    PostUrl = pageUrl
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Scrape error: {ex.Message}");
            }

            return null;
        }

        static string SanitizeGameNameForURL(string gameName)
        {
            var result = gameName.Replace(" ", "+");

            // Remove special characters
            var charsToRemove = new[] { '-', ':', '_', '&', '!', '?', ',', '.', '(', ')', '$', '#', '%', ';', '\'', '`', '"' };
            foreach (var c in charsToRemove)
            {
                result = result.Replace(c.ToString(), "");
            }

            return result;
        }

        class RinCleanInfo
        {
            public string BuildId { get; set; }
            public DateTime PostDate { get; set; }
            public string PostUrl { get; set; }
        }
    }
}