using System;
using System.Collections.Generic;
using System.Reflection;
using System.Configuration;
using System.IO;
using SAPbouiCOM;
using SAPbobsCOM;

namespace ContractManagementAddon
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Log.StartupBanner();
                AppDomain.CurrentDomain.AssemblyResolve += ResolveSapAssemblies;

                // This addon is designed to be loaded by SAP Business One as a plugin.
                // It cannot be run as a standalone application. The SAP Business One
                // application will provide the SAPbouiCOM.Application instance through
                // its addon loader mechanism.

                System.Windows.Forms.MessageBox.Show(
                    "ContractManagementAddon must be installed and loaded through SAP Business One.\n\n" +
                    "Please install this addon in SAP Business One and enable it from Tools > Add-ons Manager.",
                    "ContractManagementAddon - Setup Required",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception in Main");
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

                private static System.Reflection.Assembly ResolveSapAssemblies(object sender, ResolveEventArgs args)
        {
            try
            {
                var name = new System.Reflection.AssemblyName(args.Name).Name;
                // Support both original and Interop assembly names
                if (name != "SAPbouiCOM" && name != "SAPbobsCOM" &&
                    name != "Interop.SAPbouiCOM" && name != "Interop.SAPbobsCOM") return null;

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string cfgDir = null;
                try { cfgDir = System.Configuration.ConfigurationManager.AppSettings["SapSdkDir"]; } catch { }

                string[] candidates = BuildCandidates(name, baseDir, cfgDir);
                foreach (var path in candidates)
                {
                    if (System.IO.File.Exists(path))
                    {
                        try
                        {
                            Log.Info($"AssemblyResolve loading '{name}' from '{path}'");
                            return System.Reflection.Assembly.LoadFrom(path);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Failed to load '{name}' from '{path}'");
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static string[] BuildCandidates(string name, string baseDir, string cfgDir)
        {
            var list = new System.Collections.Generic.List<string>();

            // Map assembly names to DLL names (support both Interop and non-Interop names)
            string dllName = name + ".dll";
            string interopDllName = null;

            // For non-Interop names, also try the Interop versions
            if (name == "SAPbouiCOM")
                interopDllName = "Interop.SAPbouiCOM.dll";
            else if (name == "SAPbobsCOM")
                interopDllName = "Interop.SAPbobsCOM.dll";

            if (!string.IsNullOrEmpty(baseDir))
            {
                list.Add(System.IO.Path.Combine(baseDir, dllName));
                list.Add(System.IO.Path.Combine(baseDir, "UI API", "Lib", dllName));
                list.Add(System.IO.Path.Combine(baseDir, "DI API", "Lib", dllName));
                list.Add(System.IO.Path.Combine(baseDir, "sap-sdk", "UI API", "Lib", dllName));
                list.Add(System.IO.Path.Combine(baseDir, "sap-sdk", "DI API", "Lib", dllName));

                if (interopDllName != null)
                {
                    list.Add(System.IO.Path.Combine(baseDir, interopDllName));
                    list.Add(System.IO.Path.Combine(baseDir, "UI API", "Lib", interopDllName));
                    list.Add(System.IO.Path.Combine(baseDir, "DI API", "Lib", interopDllName));
                    list.Add(System.IO.Path.Combine(baseDir, "sap-sdk", "UI API", "Lib", interopDllName));
                    list.Add(System.IO.Path.Combine(baseDir, "sap-sdk", "DI API", "Lib", interopDllName));
                }
            }
            if (!string.IsNullOrEmpty(cfgDir))
            {
                list.Add(System.IO.Path.Combine(cfgDir, dllName));
                list.Add(System.IO.Path.Combine(cfgDir, "UI API", "Lib", dllName));
                list.Add(System.IO.Path.Combine(cfgDir, "DI API", "Lib", dllName));

                if (interopDllName != null)
                {
                    list.Add(System.IO.Path.Combine(cfgDir, interopDllName));
                    list.Add(System.IO.Path.Combine(cfgDir, "UI API", "Lib", interopDllName));
                    list.Add(System.IO.Path.Combine(cfgDir, "DI API", "Lib", interopDllName));
                }
            }
            return list.ToArray();
        }
        private static void SBO_Application_AppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                    //Exit Add-On
                    System.Windows.Forms.Application.Exit();
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_FontChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                    break;
                default:
                    break;
            }
        }
    }
}

