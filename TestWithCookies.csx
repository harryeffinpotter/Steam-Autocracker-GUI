#r "nuget: RestSharp, 110.2.0"

using RestSharp;

var client = new RestClient("https://cs.rin.ru/forum/");
var request = new RestRequest("search.php", Method.Get);

request.AddQueryParameter("keywords", "Barony");
request.AddQueryParameter("terms", "all");
request.AddQueryParameter("author", "");
request.AddQueryParameter("sc", "1");
request.AddQueryParameter("sf", "titleonly");
request.AddQueryParameter("sk", "t");
request.AddQueryParameter("sd", "d");
request.AddQueryParameter("sr", "topics");
request.AddQueryParameter("st", "0");
request.AddQueryParameter("ch", "300");
request.AddQueryParameter("t", "0");
request.AddQueryParameter("submit", "Search");

request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/119.0");
request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

var response = await client.ExecuteAsync(request);

Console.WriteLine($"Status: {response.StatusCode}");
Console.WriteLine($"Size: {response.Content?.Length ?? 0} bytes");

if (response.IsSuccessful && response.Content != null)
{
    if (response.Content.Contains("viewtopic.php?f=10"))
        Console.WriteLine("✓ Found Main Forum links!");
    if (response.Content.Contains("viewtopic.php?f=22"))
        Console.WriteLine("✓ Found Content Sharing links!");

    var matches = System.Text.RegularExpressions.Regex.Matches(response.Content, @"viewtopic\.php\?f=(\d+)&amp;t=(\d+)");
    Console.WriteLine($"\nFound {matches.Count} thread links");
}
else
{
    Console.WriteLine("Failed to get content");
}