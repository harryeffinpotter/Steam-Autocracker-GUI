using System.Net.Http;
using System.Text.RegularExpressions;

var client = new HttpClient();
var html = await client.GetStringAsync("https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search");
foreach (Match m in Regex.Matches(html, @"viewtopic\.php\?f=\d+"))
    Console.WriteLine(m.Value);