using SAPbouiCOM;

namespace ContractManagementAddon
{
    /// <summary>
    /// Static proxy to provide access to the SAP Business One Application instance.
    /// This is set by the SAP Business One addon loader when the addon is initialized.
    /// </summary>
    internal static class UIApp
    {
        public static Application SBO_Application { get; set; }
    }
}
