@echo off
echo Installing Selenium for Steam-Autocracker-GUI...
cd /d "C:\Users\ysg\source\repos\Steam-Autocracker-GUI"

echo Adding Selenium packages...
dotnet add package Selenium.WebDriver --version 4.16.2
dotnet add package Selenium.WebDriver.ChromeDriver --version 120.0.6099.10900

echo Done! Selenium installed.
pause