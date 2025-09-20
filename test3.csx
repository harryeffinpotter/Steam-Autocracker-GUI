using System.Net.Http;
using System.Text.RegularExpressions;

var client = new HttpClient();
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

try {
    var html = await client.GetStringAsync("https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search");
    Console.WriteLine($"SUCCESS! Got {html.Length} bytes");

    var links = Regex.Matches(html, @"viewtopic\.php\?f=(\d+)");
    foreach (Match m in links.Take(10))
        Console.WriteLine($"Found: {m.Value}");
}
catch (Exception ex) {
    Console.WriteLine($"FAILED: {ex.Message}");
    Console.WriteLine("CS.RIN.RU might be blocking direct access - need a working proxy");
}