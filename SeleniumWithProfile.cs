using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

class SeleniumWithProfile
{
    static void Main()
    {
        // Create a persistent Chrome profile directory
        string profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RinScraperProfile");
        Directory.CreateDirectory(profilePath);

        var options = new ChromeOptions();

        // USE A PERSISTENT PROFILE - This saves cookies/login!
        options.AddArgument($"user-data-dir={profilePath}");

        // Optional: run headless after you've logged in once
        // options.AddArgument("--headless");

        options.AddArgument("--disable-blink-features=AutomationControlled");

        Console.WriteLine($"Using Chrome profile at: {profilePath}");
        Console.WriteLine("First run: Log into CS.RIN.RU manually");
        Console.WriteLine("Future runs: Will remember your login!\n");

        using (var driver = new ChromeDriver(options))
        {
            driver.Navigate().GoToUrl("https://cs.rin.ru/forum/");

            Console.WriteLine("Press ENTER after you've logged in (first time only)...");
            Console.ReadLine();

            // Now search - will work even after rate limit because you're logged in
            driver.Navigate().GoToUrl("https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search");

            System.Threading.Thread.Sleep(2000);

            var threads = driver.FindElements(By.CssSelector("a.topictitle"));
            Console.WriteLine($"\nFound {threads.Count} threads:");

            foreach (var thread in threads)
            {
                Console.WriteLine($"  - {thread.Text}");
            }

            Console.WriteLine("\nLogin saved! Next time it will auto-login.");
        }
    }
}