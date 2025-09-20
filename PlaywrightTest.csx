#r "nuget: Microsoft.Playwright, 1.40.0"

using Microsoft.Playwright;

// Install browser if needed
var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false }); // Show browser
var page = await browser.NewPageAsync();

await page.GotoAsync("https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search");

// Wait a bit for page to load
await page.WaitForTimeoutAsync(3000);

// Get all links
var links = await page.EvaluateAsync<string[]>(@"() => {
    return Array.from(document.querySelectorAll('a.topictitle')).map(a => a.innerText);
}");

if (links.Length > 0)
{
    Console.WriteLine($"FOUND {links.Length} threads:");
    foreach (var link in links)
        Console.WriteLine($"  - {link}");
}
else
{
    Console.WriteLine("No threads found - check the browser window");
}