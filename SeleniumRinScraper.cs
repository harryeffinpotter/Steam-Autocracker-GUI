using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

public class SeleniumRinScraper
{
    private static readonly string PROFILE_PATH = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SteamAutoCrackerRinProfile"
    );

    private ChromeDriver driver;
    private bool isLoggedIn = false;

    public SeleniumRinScraper()
    {
        InitializeDriver();
    }

    private void InitializeDriver()
    {
        // Create persistent profile directory
        Directory.CreateDirectory(PROFILE_PATH);

        var options = new ChromeOptions();

        // THIS IS THE MAGIC - PERSISTENT PROFILE!
        options.AddArgument($"--user-data-dir={PROFILE_PATH}");
        options.AddArgument("--profile-directory=Default");

        // Make it less detectable
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);

        // Optional: Run headless after first login
        // options.AddArgument("--headless");

        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true; // Hide the console window

        driver = new ChromeDriver(service, options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    }

    public async Task<bool> EnsureLoggedIn()
    {
        try
        {
            // Navigate to forum
            driver.Navigate().GoToUrl("https://cs.rin.ru/forum/");
            await Task.Delay(2000);

            // Check if we're logged in by looking for logout link
            try
            {
                var logoutLink = driver.FindElement(By.LinkText("Logout"));
                if (logoutLink != null)
                {
                    Console.WriteLine("✅ Already logged in from previous session!");
                    isLoggedIn = true;
                    return true;
                }
            }
            catch
            {
                // Not logged in
            }

            // If not logged in, show login form
            Console.WriteLine("❌ Not logged in. Opening login page...");
            driver.Navigate().GoToUrl("https://cs.rin.ru/forum/ucp.php?mode=login");

            // Show the browser so user can login
            Console.WriteLine("\n⚠️ MANUAL ACTION REQUIRED:");
            Console.WriteLine("1. Login to CS.RIN.RU in the Chrome window");
            Console.WriteLine("2. Press ENTER here when done");
            Console.ReadLine();

            // Verify login worked
            driver.Navigate().GoToUrl("https://cs.rin.ru/forum/");
            try
            {
                driver.FindElement(By.LinkText("Logout"));
                Console.WriteLine("✅ Login successful! Session saved for future use.");
                isLoggedIn = true;
                return true;
            }
            catch
            {
                Console.WriteLine("❌ Login failed!");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<RinGameInfo> ScrapeGame(string gameName)
    {
        if (!isLoggedIn)
        {
            if (!await EnsureLoggedIn())
                return null;
        }

        try
        {
            // Search for game
            var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords={Uri.EscapeDataString(gameName)}&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
            driver.Navigate().GoToUrl(searchUrl);
            await Task.Delay(2000);

            // Find all thread links
            var threads = driver.FindElements(By.CssSelector("a.topictitle"));

            Console.WriteLine($"\n[{gameName}] Found {threads.Count} threads");

            string mainForumUrl = null;
            string mainForumTitle = null;

            // Find Main Forum thread (f=10)
            foreach (var thread in threads)
            {
                var href = thread.GetAttribute("href");
                var title = thread.Text;

                if (href.Contains("f=10"))
                {
                    mainForumUrl = href;
                    mainForumTitle = title;
                    Console.WriteLine($"  ✅ Main Forum: {title}");
                    break;
                }
                else if (href.Contains("f=22"))
                {
                    Console.WriteLine($"  ⚠️ Skipping Content Sharing: {title}");
                }
            }

            if (mainForumUrl == null)
            {
                Console.WriteLine($"  ❌ No Main Forum thread found");
                return null;
            }

            // Find highest page number
            var allLinks = driver.FindElements(By.CssSelector($"a[href*='viewtopic.php'][href*='t=']"));
            int maxStart = 0;
            string threadId = "";

            var threadIdMatch = System.Text.RegularExpressions.Regex.Match(mainForumUrl, @"t=(\d+)");
            if (threadIdMatch.Success)
            {
                threadId = threadIdMatch.Groups[1].Value;

                foreach (var link in allLinks)
                {
                    var href = link.GetAttribute("href");
                    if (href.Contains($"t={threadId}"))
                    {
                        var startMatch = System.Text.RegularExpressions.Regex.Match(href, @"start=(\d+)");
                        if (startMatch.Success)
                        {
                            int start = int.Parse(startMatch.Groups[1].Value);
                            if (start > maxStart)
                                maxStart = start;
                        }
                    }
                }
            }

            // Navigate to last page
            string lastPageUrl = mainForumUrl;
            if (maxStart > 0)
            {
                lastPageUrl = $"{mainForumUrl}&start={maxStart}";
                Console.WriteLine($"  📄 Going to page {(maxStart/15)+1}");
            }

            driver.Navigate().GoToUrl(lastPageUrl);
            await Task.Delay(2000);

            // Look for Clean Files posts
            var posts = driver.FindElements(By.ClassName("postbody"));
            string latestBuild = null;

            foreach (var post in posts)
            {
                var text = post.Text;
                if (text.Contains("Clean Files", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("Clean Steam Files", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract build number
                    var buildMatch = System.Text.RegularExpressions.Regex.Match(text, @"Build\s*(\d{5,})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (buildMatch.Success)
                    {
                        latestBuild = buildMatch.Groups[1].Value;
                        Console.WriteLine($"  🎯 Found Clean Files with Build: {latestBuild}");
                        break;
                    }
                }
            }

            return new RinGameInfo
            {
                GameName = gameName,
                ThreadUrl = mainForumUrl,
                BuildNumber = latestBuild,
                HasCleanFiles = !string.IsNullOrEmpty(latestBuild)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scraping {gameName}: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        driver?.Quit();
        driver?.Dispose();
    }
}

public class RinGameInfo
{
    public string GameName { get; set; }
    public string ThreadUrl { get; set; }
    public string BuildNumber { get; set; }
    public bool HasCleanFiles { get; set; }
}