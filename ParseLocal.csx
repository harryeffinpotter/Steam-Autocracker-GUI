using System.IO;
using System.Text.RegularExpressions;

// Parse the HTML you saved from browser
var html = File.ReadAllText("barony.html");

Console.WriteLine($"Loaded {html.Length} bytes\n");

// Find all threads
var threads = Regex.Matches(html, @"<a href=""(viewtopic\.php\?f=(\d+)&amp;t=(\d+)[^""]*)"" class=""topictitle"">([^<]+)</a>");

Console.WriteLine($"Found {threads.Count} threads:\n");

foreach (Match t in threads)
{
    var url = t.Groups[1].Value;
    var forumId = t.Groups[2].Value;
    var threadId = t.Groups[3].Value;
    var title = t.Groups[4].Value;

    if (forumId == "10")
    {
        Console.WriteLine($"[MAIN FORUM] {title}");
        Console.WriteLine($"  Thread ID: {threadId}");

        // Find all page links for this thread
        var pagePattern = $@"viewtopic\.php\?f=10&amp;t={threadId}.*?&amp;start=(\d+)";
        var pages = Regex.Matches(html, pagePattern);

        int maxStart = 0;
        foreach (Match p in pages)
        {
            int start = int.Parse(p.Groups[1].Value);
            if (start > maxStart) maxStart = start;
        }

        if (maxStart > 0)
        {
            int lastPage = (maxStart / 15) + 1;
            Console.WriteLine($"  Has {lastPage} pages");
            Console.WriteLine($"  Last page: viewtopic.php?f=10&t={threadId}&start={maxStart}");
        }
        else
        {
            Console.WriteLine($"  Single page thread");
        }
    }
    else if (forumId == "22")
    {
        Console.WriteLine($"[CONTENT SHARING - SKIP] {title}");
    }
}