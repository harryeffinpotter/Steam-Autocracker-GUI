using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class RinLoginTest
{
    static async Task Main()
    {
        Console.WriteLine("CS.RIN.RU Login + Scraper Test");
        Console.WriteLine("===============================\n");

        Console.Write("Enter CS.RIN.RU username: ");
        var username = Console.ReadLine();

        Console.Write("Enter CS.RIN.RU password: ");
        var password = ReadPassword();

        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AllowAutoRedirect = true
        };

        using (var client = new HttpClient(handler))
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            try
            {
                // Step 1: Get login page to fetch session
                Console.WriteLine("\nGetting login page...");
                var loginPageUrl = "https://cs.rin.ru/forum/ucp.php?mode=login";
                var loginPage = await client.GetStringAsync(loginPageUrl);

                // Extract sid (session ID) from form
                var sidMatch = Regex.Match(loginPage, @"name=""sid"" value=""([^""]+)""");
                var sid = sidMatch.Success ? sidMatch.Groups[1].Value : "";

                // Step 2: Login
                Console.WriteLine("Logging in...");
                var loginData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("login", "Login"),
                    new KeyValuePair<string, string>("sid", sid),
                    new KeyValuePair<string, string>("redirect", "./index.php"),
                    new KeyValuePair<string, string>("autologin", "on")
                });

                var loginResponse = await client.PostAsync(loginPageUrl, loginData);
                var loginResult = await loginResponse.Content.ReadAsStringAsync();

                // Check if login worked
                if (loginResult.Contains("Logout") || loginResult.Contains($"Logout [ {username} ]"))
                {
                    Console.WriteLine("✅ Login successful!\n");

                    // Step 3: Now search with logged-in session
                    Console.WriteLine("Searching for Barony...");
                    var searchUrl = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
                    var searchResult = await client.GetStringAsync(searchUrl);

                    Console.WriteLine($"Got {searchResult.Length} bytes\n");

                    // Parse results
                    var threads = Regex.Matches(searchResult, @"class=""topictitle"">([^<]+)</a>");
                    Console.WriteLine($"Found {threads.Count} threads:");
                    foreach (Match t in threads)
                        Console.WriteLine($"  - {t.Groups[1].Value}");

                    // Save cookies for future use
                    Console.WriteLine("\n✅ Cookies that can be reused:");
                    foreach (Cookie cookie in cookieContainer.GetCookies(new Uri("https://cs.rin.ru")))
                    {
                        if (cookie.Name.StartsWith("phpbb") || cookie.Name == "k" || cookie.Name == "sid")
                            Console.WriteLine($"  {cookie.Name}={cookie.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Login failed - check credentials");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    static string ReadPassword()
    {
        var password = new StringBuilder();
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Length--;
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password.ToString();
    }
}