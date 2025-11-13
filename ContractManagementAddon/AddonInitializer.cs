using System;
using System.Runtime.InteropServices;
using SAPbouiCOM;
using SAPbobsCOM;

namespace ContractManagementAddon
{
    /// <summary>
    /// Entry point for SAP Business One addon initialization.
    /// This class is called by SAP when the addon is loaded.
    /// </summary>
    [ComVisible(true)]
    [Guid("1A2B3C4D-5E6F-7A8B-9C0D-1E2F3A4B5C6D")]
    public class AddonInitializer
    {
        private static Application _sapApplication;
        private static Menu _menu;
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize the addon - called by SAP Business One
        /// </summary>
        public static void Initialize(Application sapApplication)
        {
            if (_isInitialized)
                return;

            try
            {
                Log.StartupBanner();
                Log.Info("AddonInitializer.Initialize called");

                // Store the SAP Application instance globally for access from other classes
                _sapApplication = sapApplication;
                UIApp.SBO_Application = _sapApplication;

                // Run startup tasks (UDT/UDO creation, etc.)
                Startup.RunSetup();

                // Initialize menu
                _menu = new Menu();
                _menu.AddMenuItems();

                // Register for menu events
                _sapApplication.RegisterMenuEventHandler(_menu.SBO_Application_MenuEvent);

                // Register for application events
                _sapApplication.AppEvent += SBO_Application_AppEvent;

                Log.Info("AddonInitializer.Initialize completed successfully");
                _sapApplication.SetStatusBarMessage("ContractManagementAddon loaded successfully", BoMessageTime.bmt_Short, false);
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize addon");
                if (_sapApplication != null)
                {
                    _sapApplication.SetStatusBarMessage($"ContractManagementAddon initialization failed: {ex.Message}", BoMessageTime.bmt_Long, true);
                }
                throw;
            }
        }

        private static void SBO_Application_AppEvent(BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case BoAppEventTypes.aet_ShutDown:
                    Log.Info("SAP Application shutting down");
                    System.Windows.Forms.Application.Exit();
                    break;
            }
        }
    }
}
