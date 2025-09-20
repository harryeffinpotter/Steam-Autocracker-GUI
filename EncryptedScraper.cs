using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

class EncryptedScraper
{
    // Your encrypted session cookie (encrypted with AES)
    // To get this:
    // 1. Open CS.RIN.RU in browser
    // 2. F12 -> Application -> Cookies -> find phpbb3_1fh61_sid
    // 3. Encrypt that value using EncryptString() below
    private static readonly string ENCRYPTED_SESSION = "PUT_ENCRYPTED_COOKIE_HERE";

    // Encryption key - CHANGE THIS to something unique
    private static readonly string KEY = "YourSuperSecretKey123!@#";

    static string EncryptString(string plainText, string key)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var msEncrypt = new System.IO.MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    array = msEncrypt.ToArray();
                }
            }
        }

        return Convert.ToBase64String(array);
    }

    static string DecryptString(string cipherText, string key)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (var msDecrypt = new System.IO.MemoryStream(buffer))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    static async System.Threading.Tasks.Task Main()
    {
        // Uncomment this to encrypt your cookie first time
        // Console.WriteLine("Encrypted cookie: " + EncryptString("YOUR_ACTUAL_COOKIE_HERE", KEY));
        // return;

        try
        {
            // Decrypt session at runtime
            string sessionId = DecryptString(ENCRYPTED_SESSION, KEY);

            var handler = new HttpClientHandler();
            handler.CookieContainer = new System.Net.CookieContainer();

            // Add the session cookie
            handler.CookieContainer.Add(new Uri("https://cs.rin.ru"), new System.Net.Cookie("phpbb3_1fh61_sid", sessionId));
            handler.CookieContainer.Add(new Uri("https://cs.rin.ru"), new System.Net.Cookie("phpbb3_1fh61_k", ""));
            handler.CookieContainer.Add(new Uri("https://cs.rin.ru"), new System.Net.Cookie("phpbb3_1fh61_u", "1"));

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            Console.WriteLine("Searching with encrypted session...");
            var url = "https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";

            var response = await client.GetStringAsync(url);
            Console.WriteLine($"Got {response.Length} bytes");

            var matches = Regex.Matches(response, @"viewtopic\.php\?f=(\d+)");
            Console.WriteLine($"Found {matches.Count} viewtopic links");

            if (response.Contains("not authorised"))
            {
                Console.WriteLine("Session expired or invalid");
            }
            else
            {
                Console.WriteLine("✅ SUCCESS! Session works!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}