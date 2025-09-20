using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;

class SeleniumScraper
{
    static void Main()
    {
        Console.WriteLine("Starting Chrome browser...");

        var options = new ChromeOptions();
        options.AddArgument("--headless"); // Remove this to see the browser

        using (var driver = new ChromeDriver(options))
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            Console.WriteLine("Navigating to CS.RIN.RU search...");
            driver.Navigate().GoToUrl("https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search");

            // Wait a bit for page to load
            System.Threading.Thread.Sleep(2000);

            // Find all thread links
            var threadLinks = driver.FindElements(By.CssSelector("a.topictitle"));

            Console.WriteLine($"\nFound {threadLinks.Count} threads:");

            foreach (var link in threadLinks)
            {
                var href = link.GetAttribute("href");
                var title = link.Text;

                if (href.Contains("f=10"))
                    Console.WriteLine($"[MAIN] {title}");
                else if (href.Contains("f=22"))
                    Console.WriteLine($"[SKIP] {title}");

                Console.WriteLine($"  {href}\n");
            }

            // Find highest page number
            var allLinks = driver.FindElements(By.CssSelector("a[href*='viewtopic.php']"));
            int maxStart = 0;

            foreach (var link in allLinks)
            {
                var href = link.GetAttribute("href");
                var match = Regex.Match(href, @"start=(\d+)");
                if (match.Success)
                {
                    int start = int.Parse(match.Groups[1].Value);
                    if (start > maxStart) maxStart = start;
                }
            }

            if (maxStart > 0)
            {
                int lastPage = (maxStart / 15) + 1;
                Console.WriteLine($"Thread has {lastPage} pages (max start={maxStart})");
            }
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}