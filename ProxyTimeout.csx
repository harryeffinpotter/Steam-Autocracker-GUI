using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

var handler = new HttpClientHandler();

var proxy = new WebProxy("http://gw.thunderproxy.net:5959");
proxy.Credentials = new NetworkCredential("Hz38aabA8TX9Wo95Mr-dc-ANY", "gzhwSXYYZ1l76Xj");

handler.Proxy = proxy;
handler.UseProxy = true;

var client = new HttpClient(handler);
client.Timeout = TimeSpan.FromSeconds(10); // 10 second timeout
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

try
{
    Console.WriteLine("Testing CS.RIN.RU with 10 second timeout...");
    var url = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

    var response = await client.GetStringAsync(url);
    Console.WriteLine($"SUCCESS! Got {response.Length} bytes");
}
catch (TaskCanceledException)
{
    Console.WriteLine("TIMEOUT - CS.RIN.RU is not responding through proxy");
    Console.WriteLine("\nCS.RIN.RU is probably blocking proxy IPs");
    Console.WriteLine("Options:");
    Console.WriteLine("1. Use direct connection with delays between requests");
    Console.WriteLine("2. Use Selenium to control a real browser");
    Console.WriteLine("3. Manually grab cookies from your browser");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}