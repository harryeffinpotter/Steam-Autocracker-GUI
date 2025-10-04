using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SteamAppIdIdentifier
{
    public class RinSeleniumScraper : IDisposable
    {
        private ChromeDriver driver;
        private bool isLoggedIn = false;
        private readonly string profilePath;

        public RinSeleniumScraper()
        {
            // Persistent profile path
            profilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SteamAutoCrackerRIN"
            );
        }

        public async Task<bool> Initialize(Form parentForm)
        {
            try
            {
                Directory.CreateDirectory(profilePath);

                var options = new ChromeOptions();

                // PERSISTENT PROFILE - Saves login!
                options.AddArgument($"--user-data-dir={profilePath}");
                options.AddArgument("--profile-directory=Default");

                // Make it less detectable
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");
                options.AddUserProfilePreference("credentials_enable_service", false);

                var service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;

                driver = new ChromeDriver(service, options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                // Navigate to CS.RIN.RU
                driver.Navigate().GoToUrl("https://cs.rin.ru/forum/");
                await Task.Delay(2000);

                // Check if already logged in
                try
                {
                    driver.FindElement(By.LinkText("Logout"));
                    isLoggedIn = true;

                    MessageBox.Show(parentForm,
                        "✅ Already logged into CS.RIN.RU from previous session!\n\nClick OK to start checking games.",
                        "Ready to Share",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return true;
                }
                catch
                {
                    // Not logged in - show login page
                    var result = MessageBox.Show(parentForm,
                        "Please login to CS.RIN.RU in the Chrome window that just opened.\n\n" +
                        "After logging in, click OK to continue.\n\n" +
                        "Your login will be saved for future use!",
                        "CS.RIN.RU Login Required",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Cancel)
                        return false;

                    driver.Navigate().GoToUrl("https://cs.rin.ru/forum/ucp.php?mode=login");

                    // Wait for user to confirm they logged in
                    MessageBox.Show(parentForm,
                        "Click OK after you've logged in.",
                        "Waiting for Login",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Verify login
                    driver.Navigate().GoToUrl("https://cs.rin.ru/forum/");
                    await Task.Delay(2000);

                    try
                    {
                        driver.FindElement(By.LinkText("Logout"));
                        isLoggedIn = true;

                        MessageBox.Show(parentForm,
                            "✅ Login successful! Your session is saved.\n\nStarting game checks...",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return true;
                    }
                    catch
                    {
                        MessageBox.Show(parentForm,
                            "❌ Login verification failed. Please try again.",
                            "Login Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(parentForm,
                    $"Error initializing Chrome: {ex.Message}\n\nMake sure Chrome is installed.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }

        public async Task<RinGameInfo> SearchAndScrapeGame(string gameName)
        {
            if (!isLoggedIn || driver == null)
                return null;

            try
            {
                // Search for the game
                var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords={Uri.EscapeDataString(gameName)}&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

                driver.Navigate().GoToUrl(searchUrl);
                await Task.Delay(2000);

                // Find all thread links
                var threads = driver.FindElements(By.CssSelector("a.topictitle"));

                string mainForumUrl = null;
                string threadId = null;

                // Find Main Forum thread (f=10), skip Content Sharing (f=22)
                foreach (var thread in threads)
                {
                    var href = thread.GetAttribute("href");

                    if (href.Contains("f=10"))
                    {
                        mainForumUrl = href;
                        var match = Regex.Match(href, @"t=(\d+)");
                        if (match.Success)
                            threadId = match.Groups[1].Value;
                        break;
                    }
                }

                if (mainForumUrl == null)
                    return new RinGameInfo { GameName = gameName, Status = "Not on RIN" };

                // Find last page
                var allLinks = driver.FindElements(By.CssSelector($"a[href*='t={threadId}']"));
                int maxStart = 0;

                foreach (var link in allLinks)
                {
                    var href = link.GetAttribute("href");
                    var startMatch = Regex.Match(href, @"start=(\d+)");
                    if (startMatch.Success)
                    {
                        int start = int.Parse(startMatch.Groups[1].Value);
                        if (start > maxStart)
                            maxStart = start;
                    }
                }

                // Go to last page
                string lastPageUrl = maxStart > 0
                    ? $"{mainForumUrl}&start={maxStart}"
                    : mainForumUrl;

                driver.Navigate().GoToUrl(lastPageUrl);
                await Task.Delay(2000);

                // Check up to 20 pages back for clean files
                string latestBuild = null;
                int pagesChecked = 0;
                int maxPages = 20;

                while (pagesChecked < maxPages && string.IsNullOrEmpty(latestBuild))
                {
                    pagesChecked++;

                    // Look for Clean Files posts
                    var posts = driver.FindElements(By.ClassName("postbody"));

                    foreach (var post in posts.Reverse()) // Check newest first
                    {
                        var text = post.Text;

                        if (text.IndexOf("Clean Files", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            text.IndexOf("Clean Steam Files", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            // Extract build number
                            var buildMatch = Regex.Match(text, @"Build\s*(\d{5,})", RegexOptions.IgnoreCase);
                            if (buildMatch.Success)
                            {
                                latestBuild = buildMatch.Groups[1].Value;
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(latestBuild) && pagesChecked < maxPages)
                    {
                        // Try to go to previous page
                        try
                        {
                            var prevLink = driver.FindElement(By.LinkText("Previous"));
                            prevLink.Click();
                            await Task.Delay(1000); // Small delay between pages
                        }
                        catch
                        {
                            break; // No more pages
                        }
                    }
                }

                if (!string.IsNullOrEmpty(latestBuild))
                {
                    return new RinGameInfo
                    {
                        GameName = gameName,
                        BuildNumber = latestBuild,
                        Status = "Has Clean Files",
                        ThreadUrl = mainForumUrl
                    };
                }
                else if (pagesChecked >= maxPages)
                {
                    return new RinGameInfo
                    {
                        GameName = gameName,
                        Status = "NEEDS CLEAN FILES!",
                        ThreadUrl = mainForumUrl
                    };
                }
                else
                {
                    return new RinGameInfo
                    {
                        GameName = gameName,
                        Status = "No clean files found",
                        ThreadUrl = mainForumUrl
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scraping {gameName}: {ex.Message}");
                return new RinGameInfo { GameName = gameName, Status = "Error" };
            }
        }

        public void Dispose()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        public class RinGameInfo
        {
            public string GameName { get; set; }
            public string BuildNumber { get; set; }
            public string Status { get; set; }
            public string ThreadUrl { get; set; }
        }
    }
}