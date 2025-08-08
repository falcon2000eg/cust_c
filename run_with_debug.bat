@echo off
echo Starting Customer Issues Manager with debug output...
echo.

REM Set environment variable for debug output
set DOTNET_ENVIRONMENT=Development

REM Run the application with debug output
cd CustomerIssuesManager\bin\Debug\net8.0-windows
CustomerIssuesManager.exe

echo.
echo Application closed. Check the logs folder for detailed information.
pause 