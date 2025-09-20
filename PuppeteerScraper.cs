using System;
using System.Threading.Tasks;
using PuppeteerSharp;

class PuppeteerScraper
{
    static async Task Main()
    {
        Console.WriteLine("Downloading Chromium if needed...");
        await new BrowserFetcher().DownloadAsync();

        Console.WriteLine("Launching browser...");
        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true // Set to false to see the browser
        });

        using var page = await browser.NewPageAsync();

        Console.WriteLine("Navigating to CS.RIN.RU...");
        await page.GoToAsync("https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search");

        // Wait for results to load
        await page.WaitForSelectorAsync("a.topictitle");

        // Get all thread links
        var threads = await page.EvaluateFunctionAsync(@"() => {
            const links = document.querySelectorAll('a.topictitle');
            return Array.from(links).map(link => ({
                title: link.innerText,
                href: link.href,
                isMainForum: link.href.includes('f=10'),
                isContentSharing: link.href.includes('f=22')
            }));
        }");

        Console.WriteLine($"Found threads:");
        foreach (dynamic thread in threads)
        {
            string marker = thread.isMainForum ? "[MAIN]" : thread.isContentSharing ? "[SKIP]" : "[OTHER]";
            Console.WriteLine($"{marker} {thread.title}");
            Console.WriteLine($"  {thread.href}");
        }
    }
}