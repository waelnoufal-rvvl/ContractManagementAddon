using System;
using UIApp = SAPbouiCOM.Framework.Application;
using DI = SAPbobsCOM;
using ContractManagement.Infrastructure.Sap;
using ContractManagement.Infrastructure.Data;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using ContractManagement.Core.Utils;

namespace ContractManagementAddon
{
    internal static class Startup
    {
        public static void RunSetup()
        {
            DI.Company diCompany = null;
            try
            {
                Log.StartupBanner();
                Log.Info($"AddonAssembly={typeof(Startup).Assembly.Location}");
                Log.DumpAssembly(typeof(Startup).Assembly, "Addon");
                Log.DumpAssembly(typeof(UdtSetupService).Assembly, "Infrastructure");
                Log.DumpAssembly(typeof(Money).Assembly, "Core");
                try { Log.DumpAssembly(typeof(SAPbouiCOM.Framework.Application).Assembly, "SAPbouiCOM.Framework"); } catch { }
                try { Log.DumpAssembly(typeof(SAPbouiCOM.Application).Assembly, "SAPbouiCOM"); } catch { }
                try { Log.DumpAssembly(typeof(DI.Company).Assembly, "SAPbobsCOM"); } catch { }
                diCompany = (DI.Company)UIApp.SBO_Application.Company.GetDICompany();
                if (diCompany == null || !diCompany.Connected)
                {
                    UIApp.SBO_Application.SetStatusBarMessage("DI Company not connected; setup skipped.", SAPbouiCOM.BoMessageTime.bmt_Short, true);
                    Log.Info("DI Company not connected; setup skipped.");
                    return;
                }
                else
                {
                    try
                    {
                        Log.Info($"DI Connected. CompanyDB={diCompany.CompanyDB}; Server={diCompany.Server}; LicenseServer={diCompany.LicenseServer}; DbServerType={diCompany.DbServerType}");
                    }
                    catch { }
                }

                try
                {
                    Log.Info("Creating UdtSetupService and ensuring tables...");
                    var udt = new UdtSetupService(diCompany);
                    udt.EnsureTables();
                    Log.Info("UDT setup complete.");

                    Log.Info("Creating UdoSetupService and ensuring UDOs...");
                    var udo = new UdoSetupService(diCompany);
                    udo.EnsureUdos();
                    Log.Info("UDO setup complete.");

                    Log.Info("Ensuring HANA procedures...");
                    var db = new ContractManagement.Infrastructure.Data.DiRecordsetHanaDb(diCompany);
                    var installer = new ContractManagement.Infrastructure.Data.HanaSqlInstaller(db);
                    installer.EnsureProcedures();
                    Log.Info("HANA procedures ensured.");

                    Log.Info("Ensuring default settings...");
                    var settings = new ContractManagement.Infrastructure.Sap.SettingsService(diCompany);
                    settings.EnsureDefault();
                    Log.Info("Default settings ensured.");
                }
                catch (MissingMethodException mme)
                {
                    // Typical when a stale ContractManagement.Infrastructure.dll is loaded at runtime
                    try
                    {
                        var infraAsm = typeof(UdtSetupService).Assembly;
                        Log.Error($"MissingMethodException during setup. InfraAsm={infraAsm.FullName}; Location={infraAsm.Location}");
                    }
                    catch { /* ignore */ }

                    Log.Error(mme, "Missing method while running setup");
                    UIApp.SBO_Application.SetStatusBarMessage($"Setup error (missing method). See log: {Log.PathToLog}", SAPbouiCOM.BoMessageTime.bmt_Long, true);
                    return;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Setup inner block error");
                    throw;
                }

                UIApp.SBO_Application.SetStatusBarMessage("Contract Management setup complete.", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                Log.Info("Setup complete");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled setup error");
                try
                {
                    // Also dump some quick diagnostics for easier troubleshooting
                    var loaded = string.Join("; ", AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => a.FullName.StartsWith("ContractManagement", StringComparison.OrdinalIgnoreCase) ||
                                    a.FullName.StartsWith("SAPbobsCOM", StringComparison.OrdinalIgnoreCase) ||
                                    a.FullName.StartsWith("SAPbouiCOM", StringComparison.OrdinalIgnoreCase))
                        .Select(a => $"{a.GetName().Name}@{(a.Location ?? "<dynamic>")}"));
                    Log.Info($"Loaded assemblies: {loaded}");
                }
                catch { /* ignore */ }
                UIApp.SBO_Application.SetStatusBarMessage($"Setup error. See log: {Log.PathToLog}", SAPbouiCOM.BoMessageTime.bmt_Long, true);
            }
        }
    }
}
