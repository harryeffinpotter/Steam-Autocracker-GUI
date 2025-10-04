using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

var handler = new HttpClientHandler();
handler.Proxy = new WebProxy("http://Hz38aabA8TX9Wo95Mr-dc-ANY:gzhwSXYYZ1l76Xj@gw.thunderproxy.net:5959");
handler.UseProxy = true;

using (var client = new HttpClient(handler))
{
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
    var html = await client.GetStringAsync("https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search");

    var links = Regex.Matches(html, @"href=""([^""]+)""");
    foreach (Match m in links)
        Console.WriteLine(m.Groups[1].Value);
}