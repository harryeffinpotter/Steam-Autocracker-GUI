using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

// Use cookies to appear legitimate
var handler = new HttpClientHandler();
handler.CookieContainer = new CookieContainer();
handler.AllowAutoRedirect = true;
handler.UseCookies = true;

var client = new HttpClient(handler);

// First, visit the main page to get a session
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

try {
    // Get session cookie
    Console.WriteLine("Getting session from main page...");
    var mainPage = await client.GetAsync("https://cs.rin.ru/forum/");
    Console.WriteLine($"Main page status: {mainPage.StatusCode}");

    // Now try search
    Console.WriteLine("\nSearching for Barony...");
    var searchUrl = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
    var html = await client.GetStringAsync(searchUrl);

    Console.WriteLine($"SUCCESS! Got {html.Length} bytes");

    var titles = Regex.Matches(html, @"class=""topictitle"">([^<]+)</a>");
    Console.WriteLine($"\nFound {titles.Count} threads:");
    foreach (Match m in titles)
        Console.WriteLine($"  - {m.Groups[1].Value}");
}
catch (Exception ex) {
    Console.WriteLine($"FAILED: {ex.Message}");

    // If this doesn't work, we need Selenium or Playwright
    Console.WriteLine("\nCS.RIN.RU is blocking HttpClient completely.");
    Console.WriteLine("Would need to use Selenium WebDriver or similar to automate a real browser.");
}