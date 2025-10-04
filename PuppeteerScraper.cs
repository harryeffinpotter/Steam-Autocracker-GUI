using System;
using System.Text.Json;
using System.Threading.Tasks;
using PuppeteerSharp;

class PuppeteerScraper
{
    static async Task Main()
    {
        Console.WriteLine("Downloading Chromium if needed...");
        await new BrowserFetcher().DownloadAsync();

        Console.WriteLine("Launching browser...");
        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true // Set to false to see the browser
        });

        try
        {
            var page = await browser.NewPageAsync();
            try
            {
                Console.WriteLine("Navigating to CS.RIN.RU...");
                await page.GoToAsync("https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search");

                // Wait for results to load
                await page.WaitForSelectorAsync("a.topictitle");

                // Get all thread links
                var threadsJson = await page.EvaluateFunctionAsync<string>(@"() => {
                    const links = document.querySelectorAll('a.topictitle');
                    return JSON.stringify(Array.from(links).map(link => ({
                        title: link.innerText,
                        href: link.href,
                        isMainForum: link.href.includes('f=10'),
                        isContentSharing: link.href.includes('f=22')
                    })));
                }");

                var threads = JsonSerializer.Deserialize<JsonElement>(threadsJson);

                Console.WriteLine($"Found threads:");
                foreach (var thread in threads.EnumerateArray())
                {
                    string marker = thread.GetProperty("isMainForum").GetBoolean() ? "[MAIN]" :
                                    thread.GetProperty("isContentSharing").GetBoolean() ? "[SKIP]" : "[OTHER]";
                    Console.WriteLine($"{marker} {thread.GetProperty("title").GetString()}");
                    Console.WriteLine($"  {thread.GetProperty("href").GetString()}");
                }
            }
            finally
            {
                if (page != null)
                    await page.DisposeAsync();
            }
        }
        finally
        {
            if (browser != null)
                await browser.DisposeAsync();
        }
    }
}