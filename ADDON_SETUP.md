# ContractManagementAddon - Setup and Debugging Guide

## Overview

This guide explains how to set up the ContractManagementAddon for automatic loading in SAP Business One and how to debug it in Visual Studio.

## Architecture

The addon uses a two-layer architecture:

1. **AddonInitializer class**: Entry point for SAP Business One. This class is called by SAP when the addon is loaded.
2. **UIApp static proxy**: Provides global access to the SAP Application instance for all addon components.

## Setup Steps

### Step 1: Build the Project

1. Open the solution in Visual Studio
2. Build the project in **Debug** configuration (this creates the executable SAP can load)
3. The addon files will be in: `ContractManagementAddon\bin\Debug\`

### Step 2: Register the Addon as a COM Component

Before SAP Business One can load the addon, it must be registered as a COM component:

#### Option A: Using the Batch Script (Recommended)

1. **Open Command Prompt as Administrator**
   - Right-click on Command Prompt and select "Run as Administrator"

2. **Navigate to the project directory:**
   ```
   cd C:\MyProjects\Revival\ContractManagementAddon
   ```

3. **Run the registration script:**
   ```
   RegisterAddon.bat
   ```

   This script will:
   - Locate your compiled addon (ContractManagementAddon.exe)
   - Register it as a COM component using regasm.exe
   - Create type library information for COM interop
   - Verify registration was successful

#### Option B: Manual Registration

If the batch script doesn't work, register manually:

```cmd
cd C:\MyProjects\Revival\ContractManagementAddon\ContractManagementAddon\bin\Debug
regasm.exe ContractManagementAddon.exe /codebase /tlb
```

### Step 3: Verify COM Registration

To verify the addon is registered:

1. Open Registry Editor (regedit.exe)
2. Navigate to: `HKEY_CLASSES_ROOT\CLSID\{1A2B3C4D-5E6F-7A8B-9C0D-1E2F3A4B5C6D}`
3. You should see registry entries for the addon

If the GUID doesn't exist, the registration failed. Check that:
- You ran as Administrator
- regasm.exe is available (usually at `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe` for 64-bit)
- The addon EXE exists in the build output directory
- Your SAP Business One installation is 64-bit

## Debugging the Addon

### Method 1: Attach to Process (Recommended for Development)

1. **Build the project** in Debug mode
2. **Register the addon** (if not already done)
3. **Start SAP Business One** manually or use the launcher script:
   ```
   LaunchWithDebug.bat
   ```
4. **In Visual Studio**, go to **Debug > Attach to Process**
5. **Find and select** `SAPbobsClient.exe` in the process list
6. **Click Attach**
7. **Log in to SAP Business One**
8. The addon will initialize and you can debug it

### Method 2: Set Visual Studio as Debug Target

1. **Right-click the ContractManagementAddon project** and select **Properties**
2. Go to the **Debug** tab
3. Select **Start external program** and browse to:
   ```
   C:\Program Files\SAP\SAP Business One\Client\SAPbobsClient.exe
   ```
   (adjust path if SAP is installed elsewhere)
4. **Press F5** to start debugging
5. SAP Business One will launch, and the addon will load

## How the Addon Loads

### Automatic Initialization Flow

1. **SAP Business One starts** and looks in the Windows Registry for registered COM addons
2. **SAP finds your addon** (CLSID: 1A2B3C4D-5E6F-7A8B-9C0D-1E2F3A4B5C6D)
3. **SAP creates an instance** of the `AddonInitializer` class
4. **SAP calls the `Initialize()` static method** and passes the SAP Application instance
5. **AddonInitializer**:
   - Stores the Application reference in the global `UIApp` class
   - Runs `Startup.RunSetup()` to create UDTs, UDOs, and HANA procedures
   - Initializes the `Menu` class and adds menu items
   - Registers event handlers
6. **Addon is now fully loaded** and listening for menu events

### Entry Point: AddonInitializer.cs

The `AddonInitializer` class is the key component:

```csharp
public class AddonInitializer
{
    public static void Initialize(Application sapApplication)
    {
        // Store SAP Application instance globally
        UIApp.SBO_Application = sapApplication;

        // Run setup (UDT/UDO creation)
        Startup.RunSetup();

        // Initialize menus
        _menu = new Menu();
        _menu.AddMenuItems();

        // Register event handlers
        sapApplication.RegisterMenuEventHandler(_menu.SBO_Application_MenuEvent);
    }
}
```

When you modify any of these classes, **rebuild the project** and:
- If you're attached with debugger: the debugger will show new breakpoints
- If using launcher: restart SAP Business One to load the updated addon

## Troubleshooting

### Addon Not Loading

**Symptoms**: Addon menu doesn't appear in SAP Business One

**Solutions**:
1. Verify COM registration:
   ```
   RegisterAddon.bat
   ```
2. Check the log file: `ContractManagementAddon.log`
3. Ensure SAP Business One was restarted after registration
4. Run as Administrator

### "Could not find type or namespace" Errors

**Symptoms**: Compilation errors about missing types

**Solutions**:
1. Ensure the sap-sdk folder exists with the Interop DLLs
2. Check Project > Properties > References for missing assemblies
3. Rebuild the entire solution (not just the addon project)

### Assembly Resolution Errors at Runtime

**Symptoms**: "Could not load assembly" when addon initializes

**Solutions**:
1. Verify that Interop.SAPbouiCOM.dll and Interop.SAPbobsCOM.dll are in the output directory
2. Check the log file for detailed error messages
3. Ensure the App.config file is copied to the output directory

### Debugger Won't Attach

**Symptoms**: "Cannot attach to process" error

**Solutions**:
1. Ensure you're running Visual Studio as Administrator
2. Make sure the addon was built in Debug mode (not Release)
3. Kill any existing SAPbobsClient.exe processes and restart
4. Try using the launcher batch script

## Important Notes

### x64 Architecture

⚠️ **Important**: SAP Business One is a 64-bit application. The project is configured to compile as **x64** only. Do NOT change the platform target to AnyCPU or x86.

### Rebuilding the Addon

After making code changes:

1. **Rebuild the project**
2. **Stop SAP Business One** completely (check Task Manager for any remaining processes)
3. **Re-register the addon** using `RegisterAddon.bat` (especially if DLL path changed)
4. **Start SAP Business One** again

### UIApp Static Class

All components access the SAP Application instance through the global `UIApp.SBO_Application` class:

```csharp
// Available anywhere in the addon code:
UIApp.SBO_Application.Forms.Add(...)
UIApp.SBO_Application.SetStatusBarMessage(...)
UIApp.SBO_Application.Menus.Item(...)
```

This provides a clean way to access SAP without passing the Application instance around.

## Configuration File

The `App.config` file contains addon settings:

```xml
<appSettings>
  <!-- Path to SAP SDK (leave empty to auto-detect) -->
  <add key="SapSdkDir" value="" />

  <!-- Enable debug logging -->
  <add key="EnableDebugLog" value="true" />

  <!-- Log file location -->
  <add key="LogFilePath" value="ContractManagementAddon.log" />
</appSettings>
```

## Project Structure

```
ContractManagementAddon/
├── ContractManagementAddon/          # Main addon project
│   ├── AddonInitializer.cs           # Entry point (called by SAP)
│   ├── UIApp.cs                      # Global SAP Application proxy
│   ├── Program.cs                    # Fallback entry point
│   ├── Menu.cs                       # Menu management
│   ├── ConfigForm.cs                 # Configuration UI
│   ├── Startup.cs                    # Initialization logic
│   ├── App.config                    # Configuration
│   └── bin/Debug/                    # Build output (where SAP loads from)
├── ContractManagement.Infrastructure/ # Data and SAP service layer
├── ContractManagement.Core/           # Business logic and models
├── RegisterAddon.bat                  # COM registration script
├── LaunchWithDebug.bat                # SAP launcher script
└── ADDON_SETUP.md                     # This file
```

## Next Steps

1. ✅ Build the project
2. ✅ Register the addon using `RegisterAddon.bat`
3. ✅ Start debugging using one of the debugging methods above
4. ✅ Check the log file for any issues: `ContractManagementAddon.log`

## Support

For issues or questions:
1. Check the `ContractManagementAddon.log` file for error messages
2. Review the output window in Visual Studio for compilation errors
3. Verify COM registration using Registry Editor
4. Ensure all required assemblies are present in the build output directory
