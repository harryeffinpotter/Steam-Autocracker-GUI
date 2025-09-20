@echo off
echo Compiling and running link scraper...
cd /d "C:\Users\ysg\source\repos\Steam-Autocracker-GUI"

REM Try to find csc.exe in common locations
set CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC_PATH%" set CSC_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
if not exist "%CSC_PATH%" (
    echo CSC not found, trying dotnet...
    dotnet new console -n LinkTest -f net6.0 --force
    copy JustLinks.cs LinkTest\Program.cs /Y
    cd LinkTest
    dotnet run
    pause
    exit
)

"%CSC_PATH%" /out:JustLinks.exe JustLinks.cs
JustLinks.exe
pause