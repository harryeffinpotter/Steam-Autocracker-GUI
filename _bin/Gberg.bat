echo off
set "_null=1>nul 2>nul"
chcp 65001 %_null%

setlocal EnableDelayedExpansion
setlocal Enableextensions
mkdir "TEMP" %_null%

"%~dp0curl\curl.exe" "https://mr_goldberg.gitlab.io/goldberg_emulator/" -s > "TEMP\1.tmp" 
findstr /I /R /C:"https://gitlab.com/Mr_Goldberg/goldberg_emulator/-/jobs/.*/artifacts/download" "TEMP\1.tmp" > "TEMP\2.tmp"
for /f "tokens=7 delims=/" %%a in (TEMP\2.tmp) do ( set "JobID=%%a" )
del /f /s /q "TEMP\1.tmp"
del /f /s /q "TEMP\2.tmp"
set /p OldJobID=<Goldberg\job_id
IF /I !JobID! == !OldJobID! (
echo Goldberg Emulator Already Updated to Latest Version.
goto :Menu
)
echo Downloading update...
set URL=https://gitlab.com/Mr_Goldberg/goldberg_emulator/-/jobs/!JobID!/artifacts/download
"curl\curl.exe" -L "!URL!" --output "TEMP\Goldberg.zip"
echo Download Complete. Extracting files......
"%~dp07z\7za.exe" -o"TEMP\Goldberg" x "TEMP\Goldberg.zip"
del /f /s /q "TEMP\Goldberg.zip"
copy /Y "TEMP\Goldberg\steam_api.dll" "Goldberg\steam_api.dll"
copy /Y "TEMP\Goldberg\steam_api64.dll" "Goldberg\steam_api64.dll"
echo !JobID!> "Goldberg\job_id"
echo Update completed.
del /f /s /q "Temp\Goldberg"
rd /s /q "Temp\Goldberg"
echo.
ENDLOCAL
:Menu