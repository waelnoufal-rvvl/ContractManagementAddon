@echo off
REM Launch SAP Business One with addon debugging enabled

setlocal enabledelayedexpansion

REM Try to find SAP Business One executable
set "SAP_PATH="
set "SAP_EXE=SAPbobsClient.exe"

REM Check common SAP installation paths
if exist "C:\Program Files\SAP\SAP Business One\Client\SAPbobsClient.exe" (
    set "SAP_PATH=C:\Program Files\SAP\SAP Business One\Client\"
) else if exist "C:\Program Files (x86)\SAP\SAP Business One\Client\SAPbobsClient.exe" (
    set "SAP_PATH=C:\Program Files (x86)\SAP\SAP Business One\Client\"
) else (
    echo ERROR: Could not find SAP Business One installation
    echo Please install SAP Business One or update the path in this script
    pause
    exit /b 1
)

echo.
echo ========================================
echo ContractManagementAddon Debug Launcher
echo ========================================
echo.
echo SAP Business One path: !SAP_PATH!
echo.

REM Check if addon is registered
echo Checking if addon is registered in COM...
reg query "HKEY_CLASSES_ROOT\CLSID\{1A2B3C4D-5E6F-7A8B-9C0D-1E2F3A4B5C6D}" >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo WARNING: Addon does not appear to be registered
    echo Please run RegisterAddon.bat first
    echo.
    pause
    exit /b 1
)

echo Addon is registered
echo.
echo Launching SAP Business One...
echo.
echo IMPORTANT: To debug the addon:
echo 1. Visual Studio will show "Debug ^> Attach to Process"
echo 2. Find and select the SAP Business One process (SAPbobsClient.exe)
echo 3. Click Attach
echo 4. The addon will initialize when you log in to SAP
echo.
pause

REM Start SAP Business One
start "" "!SAP_PATH!!SAP_EXE!"

echo.
echo SAP Business One launched
echo Waiting for it to fully initialize...
timeout /t 10 /nobreak

echo.
echo You can now:
echo 1. Log in to SAP Business One
echo 2. The addon should load automatically
echo 3. Check the log file for any errors: ContractManagementAddon.log
echo.
