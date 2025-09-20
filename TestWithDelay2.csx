using System.Net.Http;
using System.Text.RegularExpressions;

var client = new HttpClient();
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

// Test 1: Direct visit to main page
Console.WriteLine("Test 1: Visiting main page first...");
try {
    var mainPage = await client.GetAsync("https://cs.rin.ru/forum/");
    Console.WriteLine($"Main page: {mainPage.StatusCode}");

    if (mainPage.IsSuccessStatusCode)
    {
        Console.WriteLine("Waiting 5 seconds before search...");
        await Task.Delay(5000);

        Console.WriteLine("\nTest 2: Now searching for Barony...");
        var searchUrl = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
        var searchPage = await client.GetStringAsync(searchUrl);

        Console.WriteLine($"Search page: Got {searchPage.Length} bytes");

        var matches = Regex.Matches(searchPage, @"viewtopic\.php\?f=(\d+)");
        Console.WriteLine($"Found {matches.Count} viewtopic links");

        if (matches.Count > 0)
        {
            Console.WriteLine("\n✅ SUCCESS! Scraper works with delays!");
        }
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Failed: {ex.Message}");
    Console.WriteLine("\nEven with delays, CS.RIN.RU is blocking HttpClient");
    Console.WriteLine("Need to use a browser automation tool or find another approach");
}