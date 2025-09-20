#r "nuget: HtmlAgilityPack, 1.11.54"

using HtmlAgilityPack;

var web = new HtmlWeb();
var doc = web.Load("https://cs.rin.ru/forum/search.php?keywords=Barony");

var titles = doc.DocumentNode.SelectNodes("//a[@class='topictitle']");
if (titles != null)
{
    foreach (var t in titles)
        Console.WriteLine(t.InnerText);
}
else
{
    Console.WriteLine("Blocked or no results");
}