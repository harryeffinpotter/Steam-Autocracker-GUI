@echo off
echo Building project...
dotnet build "HFP's APPID Finder.csproj" -c Debug

echo.
echo Obfuscating with ConfuserEx...
ConfuserEx\Confuser.CLI.exe ConfuserEx.crproj

echo.
echo Done! Obfuscated files are in: bin\Obfuscated
pause
