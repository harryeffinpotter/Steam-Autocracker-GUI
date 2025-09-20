@echo off
echo Creating Selenium test project...
cd C:\Users\ysg\source\repos
mkdir RinSelenium
cd RinSelenium

echo Creating new console app...
dotnet new console -f net8.0

echo Installing Selenium packages...
dotnet add package Selenium.WebDriver
dotnet add package Selenium.WebDriver.ChromeDriver

echo Creating scraper code...
(
echo using OpenQA.Selenium;
echo using OpenQA.Selenium.Chrome;
echo.
echo var options = new ChromeOptions();
echo options.AddArgument("--disable-blink-features=AutomationControlled");
echo options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
echo.
echo using var driver = new ChromeDriver(options);
echo driver.Navigate().GoToUrl("https://cs.rin.ru/forum/search.php?keywords=Barony&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search");
echo.
echo Thread.Sleep(3000);
echo.
echo var threads = driver.FindElements(By.CssSelector("a.topictitle"));
echo Console.WriteLine($"Found {threads.Count} threads:");
echo.
echo foreach (var thread in threads)
echo     Console.WriteLine($"  - {thread.Text}");
echo.
echo Console.ReadKey();
echo driver.Quit();
) > Program.cs

echo.
echo Running Selenium scraper...
dotnet run

pause