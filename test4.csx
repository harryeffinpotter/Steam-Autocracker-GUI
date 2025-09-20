using System.Net.Http;
using System.Text.RegularExpressions;

var client = new HttpClient();
client.DefaultRequestHeaders.Clear();
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
client.DefaultRequestHeaders.Add("DNT", "1");
client.DefaultRequestHeaders.Add("Connection", "keep-alive");
client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

try {
    var html = await client.GetStringAsync("https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search");
    Console.WriteLine($"SUCCESS! Got {html.Length} bytes");

    // Show thread titles
    var titles = Regex.Matches(html, @"class=""topictitle"">([^<]+)</a>");
    Console.WriteLine($"\nFound {titles.Count} threads:");
    foreach (Match m in titles)
        Console.WriteLine($"  - {m.Groups[1].Value}");

    // Show forum IDs
    var links = Regex.Matches(html, @"viewtopic\.php\?f=(\d+)");
    var f10 = links.Cast<Match>().Count(m => m.Groups[1].Value == "10");
    var f22 = links.Cast<Match>().Count(m => m.Groups[1].Value == "22");

    Console.WriteLine($"\nMain Forum (f=10): {f10} links");
    Console.WriteLine($"Content Sharing (f=22): {f22} links");
}
catch (Exception ex) {
    Console.WriteLine($"FAILED: {ex.Message}");
    Console.WriteLine("\nMight need cookies or session. Open browser DevTools on cs.rin.ru");
    Console.WriteLine("and check Network tab for required headers.");
}