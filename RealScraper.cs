using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class RealScraper
{
    static async Task Main()
    {
        // HtmlAgilityPack with proper configuration
        var web = new HtmlWeb();
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
        web.UseCookies = true;

        // Load the search page
        var url = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
        Console.WriteLine("Scraping with HtmlAgilityPack...");

        var doc = await web.LoadFromWebAsync(url);

        // Find all thread links
        var threadLinks = doc.DocumentNode.SelectNodes("//a[@class='topictitle']");

        if (threadLinks != null)
        {
            Console.WriteLine($"Found {threadLinks.Count} threads:\n");

            foreach (var link in threadLinks)
            {
                var href = link.GetAttributeValue("href", "");
                var title = link.InnerText;

                // Check if Main Forum (f=10) or Content Sharing (f=22)
                if (href.Contains("f=10"))
                    Console.WriteLine($"[MAIN] {title}");
                else if (href.Contains("f=22"))
                    Console.WriteLine($"[SKIP] {title}");
                else
                    Console.WriteLine($"[OTHER] {title}");

                Console.WriteLine($"  -> {href}\n");
            }
        }
        else
        {
            Console.WriteLine("No threads found or blocked.");
        }
    }
}