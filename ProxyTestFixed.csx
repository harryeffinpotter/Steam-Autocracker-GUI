using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

// Thunder proxy with PROPER authentication
var handler = new HttpClientHandler();

// Create proxy WITH credentials
var proxy = new WebProxy("http://gw.thunderproxy.net:5959");
proxy.Credentials = new NetworkCredential("Hz38aabA8TX9Wo95Mr-dc-ANY", "gzhwSXYYZ1l76Xj");

handler.Proxy = proxy;
handler.UseProxy = true;

var client = new HttpClient(handler);
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

try
{
    // Test proxy first
    Console.WriteLine("Testing proxy...");
    var test = await client.GetStringAsync("http://ip-api.com/json");
    Console.WriteLine($"Proxy works! Response: {test.Substring(0, Math.Min(100, test.Length))}...\n");

    // Now try CS.RIN.RU
    Console.WriteLine("Searching CS.RIN.RU for Barony...");
    var url = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

    var response = await client.GetStringAsync(url);
    Console.WriteLine($"Got {response.Length} bytes");

    var matches = Regex.Matches(response, @"viewtopic\.php\?f=(\d+)");
    Console.WriteLine($"Found {matches.Count} viewtopic links");

    if (matches.Count > 0)
    {
        Console.WriteLine("\n✅ SUCCESS! Proxy + scraper works!");

        // Show what we found
        var f10 = matches.Cast<Match>().Count(m => m.Groups[1].Value == "10");
        var f22 = matches.Cast<Match>().Count(m => m.Groups[1].Value == "22");
        Console.WriteLine($"Main Forum (f=10): {f10} links");
        Console.WriteLine($"Content Sharing (f=22): {f22} links");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Failed: {ex.Message}");
}