@echo off
REM Register ContractManagementAddon as a COM component for SAP Business One

setlocal enabledelayedexpansion

REM Get the directory where this batch file is located
set "SCRIPT_DIR=%~dp0"
set "DEBUG_DLL=%SCRIPT_DIR%ContractManagementAddon\bin\Debug\ContractManagementAddon.exe"
set "RELEASE_DLL=%SCRIPT_DIR%ContractManagementAddon\bin\Release\ContractManagementAddon.exe"

echo.
echo ========================================
echo ContractManagementAddon COM Registration
echo ========================================
echo.

REM Determine which EXE to register (prefer Debug if it exists)
set "EXE_TO_REGISTER="
if exist "!DEBUG_DLL!" (
    set "EXE_TO_REGISTER=!DEBUG_DLL!"
    echo Using DEBUG build: !EXE_TO_REGISTER!
) else if exist "!RELEASE_DLL!" (
    set "EXE_TO_REGISTER=!RELEASE_DLL!"
    echo Using RELEASE build: !EXE_TO_REGISTER!
) else (
    echo ERROR: Could not find ContractManagementAddon.exe in Debug or Release directories
    echo Please build the project first.
    pause
    exit /b 1
)

REM Verify we're building for 64-bit
echo.
echo NOTE: This addon is designed for 64-bit SAP Business One
echo If registration fails, ensure your SAP Business One is 64-bit

REM Register the EXE with COM (for 64-bit)
echo.
echo Registering !EXE_TO_REGISTER! with COM...
regasm.exe "!EXE_TO_REGISTER!" /codebase /tlb

if %errorlevel% equ 0 (
    echo.
    echo SUCCESS: Addon registered successfully!
    echo.
    echo Next steps:
    echo 1. Start SAP Business One
    echo 2. Go to Tools ^> Add-ons Manager
    echo 3. The ContractManagementAddon should appear in the list
    echo 4. Install and enable it
    echo.
) else (
    echo.
    echo ERROR: Registration failed!
    echo Please ensure you are running this as Administrator.
    echo.
    pause
    exit /b 1
)

pause
