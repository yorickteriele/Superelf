@echo off
echo Google Calendar Integration Test
echo ================================
echo.

echo ⚠️  IMPORTANT: Google now requires an API key for ALL calendar access!
echo    Including public calendars. Please set up an API key first.
echo.

REM Check if the API key is configured
echo Checking API configuration...
findstr /C:"YOUR_GOOGLE_API_KEY_HERE" Superelf.API\appsettings.Development.json >nul
if %errorlevel%==0 (
    echo ❌ API key not configured! This is now REQUIRED.
    echo.
    echo 📝 To fix this:
    echo 1. Go to https://console.cloud.google.com/
    echo 2. Create a project or select existing one
    echo 3. Enable Google Calendar API
    echo 4. Create credentials ^(API Key^)
    echo 5. Edit Superelf.API\appsettings.Development.json
    echo 6. Replace "YOUR_GOOGLE_API_KEY_HERE" with your actual API key
    echo.
    pause
    exit /b 1
)

echo ✅ API key appears to be configured
echo.

REM Start the API if not running
echo Starting Superelf API...
start /B dotnet run --project Superelf.API\Superelf.API.csproj

REM Wait a bit for the API to start
timeout /t 5 /nobreak >nul

echo.
echo ================================
echo Test your calendar integration:
echo.
echo 1. Make sure your calendar is PUBLIC
echo 2. Open http://localhost:5000/swagger
echo 3. Navigate to Admin endpoints
echo 4. Use "POST /api/Admin/test-calendar-access"
echo 5. Enter your calendar ID
echo.
echo Calendar ID examples:
echo - primary ^(your main calendar^)
echo - your.email@gmail.com
echo - c_1234567890abcdef@group.calendar.google.com
echo.
echo Note: API key is now REQUIRED by Google
echo.
pause
