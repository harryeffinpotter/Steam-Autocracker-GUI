using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

class TestBeatSaberCLI
{
    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════╗
║        BEAT SABER CS.RIN.RU CLEAN FILES FINDER           ║
╚═══════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.WriteLine("\nThis will find the LATEST clean files for Beat Saber");
        Console.WriteLine("It will start from the last page and work backwards");
        Console.WriteLine("With 5-second delays between pages to be respectful\n");

        Console.WriteLine("Press ENTER to start...");
        Console.ReadLine();

        await FindBeatSaberCleanFiles();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static async Task FindBeatSaberCleanFiles()
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

                // ========== SEARCH ==========
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[1] SEARCHING FOR BEAT SABER");
                Console.ResetColor();
                Console.WriteLine("────────────────────────────────");

                var searchUrl = "https://cs.rin.ru/forum/search.php?keywords=Beat+Saber&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

                Console.Write("Fetching search results... ");
                var searchHtml = await client.GetStringAsync(searchUrl);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ ({searchHtml.Length:N0} bytes)");
                Console.ResetColor();

                // Find threads
                var threadPattern = @"<a href=""(viewtopic\.php\?f=(\d+)&amp;t=(\d+)[^""]*)"" class=""topictitle"">([^<]+)</a>";
                var threads = Regex.Matches(searchHtml, threadPattern);

                string mainThreadId = null;
                string mainThreadTitle = null;

                Console.WriteLine($"\nFound {threads.Count} threads:");
                foreach (Match thread in threads)
                {
                    var forumId = thread.Groups[2].Value;
                    var threadId = thread.Groups[3].Value;
                    var title = thread.Groups[4].Value;

                    if (forumId == "10")
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ [MAIN FORUM] {title}");
                        Console.ResetColor();
                        if (mainThreadId == null)
                        {
                            mainThreadId = threadId;
                            mainThreadTitle = title;
                        }
                    }
                    else if (forumId == "22")
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"  × [CONTENT SHARING] {title}");
                        Console.ResetColor();
                    }
                }

                if (mainThreadId == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n❌ No Main Forum thread found!");
                    Console.ResetColor();
                    return;
                }

                // ========== FIND LAST PAGE ==========
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[2] ANALYZING THREAD: {mainThreadTitle}");
                Console.ResetColor();
                Console.WriteLine("────────────────────────────────");

                // Find all page links for this thread
                var pagePattern = $@"viewtopic\.php\?f=10&amp;t={mainThreadId}[^""]*?(&amp;start=(\d+))?";
                var pageLinks = Regex.Matches(searchHtml, pagePattern);

                var startValues = new HashSet<int> { 0 };
                foreach (Match link in pageLinks)
                {
                    if (link.Groups[2].Success)
                    {
                        startValues.Add(int.Parse(link.Groups[2].Value));
                    }
                }

                int maxStart = startValues.Max();
                int totalPages = (maxStart / 15) + 1;

                Console.WriteLine($"Thread has {totalPages} pages");
                Console.WriteLine($"Last page starts at: {maxStart}");

                // ========== SCAN PAGES ==========
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[3] SCANNING FOR CLEAN FILES");
                Console.ResetColor();
                Console.WriteLine("────────────────────────────────");
                Console.WriteLine("Will check up to 10 pages backwards");
                Console.WriteLine("5 second delay between pages\n");

                string currentUrl = $"https://cs.rin.ru/forum/viewtopic.php?f=10&t={mainThreadId}&start={maxStart}";
                int pagesChecked = 0;
                int maxPages = 10;
                bool foundCleanFiles = false;
                string latestBuild = null;
                string foundOnPage = "";

                while (pagesChecked < maxPages && !foundCleanFiles)
                {
                    pagesChecked++;
                    int currentPage = totalPages - pagesChecked + 1;

                    Console.Write($"Page {currentPage}/{totalPages}: ");

                    var pageHtml = await client.GetStringAsync(currentUrl);
                    Console.Write($"({pageHtml.Length:N0} bytes) ");

                    // Find posts
                    var posts = Regex.Matches(pageHtml, @"<div class=""postbody"">.*?</div>\s*</div>\s*</div>", RegexOptions.Singleline);
                    Console.Write($"[{posts.Count} posts] ");

                    // Scan posts from bottom up (newest first)
                    foreach (Match post in posts.Cast<Match>().Reverse())
                    {
                        var content = post.Value;

                        // Check for clean files indicators
                        bool hasClean = Regex.IsMatch(content, @"Clean\s*(Steam\s*)?Files?|\(Clean\s*Steam\s*Files\)", RegexOptions.IgnoreCase);
                        bool hasVersion = Regex.IsMatch(content, @"(Version:|Uploaded\s*version:).*\d{4}", RegexOptions.IgnoreCase);
                        bool hasBuild = Regex.IsMatch(content, @"Build\s*\d{5,}", RegexOptions.IgnoreCase);
                        bool hasLinks = content.Contains("[[Please login to see this link.]]");

                        if (hasClean || hasVersion || hasBuild)
                        {
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"  → Found potential clean files post!");
                            Console.ResetColor();

                            if (hasClean) Console.WriteLine($"    ✓ Has 'Clean Files' text");
                            if (hasVersion) Console.WriteLine($"    ✓ Has Version info");
                            if (hasBuild) Console.WriteLine($"    ✓ Has Build number");
                            if (hasLinks) Console.WriteLine($"    ✓ Has hidden links");

                            // Extract build number
                            var buildPatterns = new[] {
                                @"\[Build\s*(\d{5,})\]",
                                @"Build\s*(\d{5,})",
                                @"Build:\s*(\d{5,})",
                                @"Uploaded\s*version:.*\[Build\s*(\d{5,})\]",
                                @"Build\s*(\d{8,})"
                            };

                            foreach (var pattern in buildPatterns)
                            {
                                var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
                                if (match.Success)
                                {
                                    latestBuild = match.Groups[1].Value;
                                    foundCleanFiles = true;
                                    foundOnPage = $"Page {currentPage}";

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"\n    🎯 BUILD NUMBER: {latestBuild}");
                                    Console.ResetColor();

                                    // Extract text preview
                                    var text = Regex.Replace(content, "<[^>]+>", " ");
                                    text = Regex.Replace(text, @"\s+", " ").Trim();

                                    // Look for the version line specifically
                                    var versionMatch = Regex.Match(text, @"Version:.*?UTC.*?\[Build \d+\]", RegexOptions.IgnoreCase);
                                    if (versionMatch.Success)
                                    {
                                        Console.WriteLine($"    Version info: {versionMatch.Value}");
                                    }
                                    else if (text.Length > 200)
                                    {
                                        text = text.Substring(0, 200) + "...";
                                        Console.WriteLine($"    Preview: {text}");
                                    }

                                    break;
                                }
                            }

                            if (foundCleanFiles) break;
                        }
                    }

                    if (!foundCleanFiles)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("No clean files found");
                        Console.ResetColor();

                        // Look for Previous link
                        var prevMatch = Regex.Match(pageHtml, @"<a href=""\./?(viewtopic\.php\?[^""]+)""[^>]*>Previous</a>");

                        if (prevMatch.Success && pagesChecked < maxPages)
                        {
                            var prevUrl = HttpUtility.HtmlDecode(prevMatch.Groups[1].Value);
                            currentUrl = $"https://cs.rin.ru/forum/{prevUrl}";

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"  → Going to previous page (waiting 5 seconds)... ");
                            Console.ResetColor();

                            // Respectful delay
                            for (int i = 5; i > 0; i--)
                            {
                                Console.Write($"{i}... ");
                                Thread.Sleep(1000);
                            }
                            Console.WriteLine();
                        }
                        else if (!prevMatch.Success)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n  × No more pages to check (reached beginning)");
                            Console.ResetColor();
                            break;
                        }
                    }
                }

                // ========== RESULTS ==========
                Console.WriteLine();
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                if (foundCleanFiles && latestBuild != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ SUCCESS! FOUND LATEST CLEAN FILES");
                    Console.ResetColor();
                    Console.WriteLine($"   Game: Beat Saber");
                    Console.WriteLine($"   Latest Build: {latestBuild}");
                    Console.WriteLine($"   Found on: {foundOnPage}");
                    Console.WriteLine($"   Pages checked: {pagesChecked}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ NO CLEAN FILES FOUND");
                    Console.ResetColor();
                    Console.WriteLine($"   Checked {pagesChecked} pages");
                    Console.WriteLine("   Either no clean files exist or they're older than 10 pages");
                    Console.WriteLine("\n   This means CS.RIN.RU NEEDS clean files for this game!");
                }

                Console.WriteLine("═══════════════════════════════════════════════════════════");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n\n❌ ERROR: {ex.Message}");
            Console.ResetColor();

            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
            }
        }
    }
}