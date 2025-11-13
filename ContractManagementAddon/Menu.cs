using System;
using System.Collections.Generic;
using System.Text;
using Application = SAPbouiCOM.Framework.Application;

namespace ContractManagementAddon
{
    class Menu
    {
        public void AddMenuItems()
        {
            SAPbouiCOM.Menus oMenus = null;
            SAPbouiCOM.MenuItem oMenuItem = null;

            oMenus = Application.SBO_Application.Menus;

            SAPbouiCOM.MenuCreationParams oCreationPackage = null;
            oCreationPackage = ((SAPbouiCOM.MenuCreationParams)(Application.SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)));
            oMenuItem = Application.SBO_Application.Menus.Item("43520"); // moudles'

            oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_POPUP;
            oCreationPackage.UniqueID = "ContractManagementAddon";
            oCreationPackage.String = "ContractManagementAddon";
            oCreationPackage.Enabled = true;
            oCreationPackage.Position = -1;

            oMenus = oMenuItem.SubMenus;

            try
            {
                //  If the manu already exists this code will fail
                oMenus.AddEx(oCreationPackage);
            }
            catch (Exception)
            {

            }

            try
            {
                // Get the menu collection of the newly added pop-up item
                oMenuItem = Application.SBO_Application.Menus.Item("ContractManagementAddon");
                oMenus = oMenuItem.SubMenus;

                // Create sub menus
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "ContractManagementAddon.Form1";
                oCreationPackage.String = "Form1";
                oMenus.AddEx(oCreationPackage);

                oCreationPackage.UniqueID = "ContractManagementAddon.RecalcICP";
                oCreationPackage.String = "Recalculate ICP";
                oMenus.AddEx(oCreationPackage);

                oCreationPackage.UniqueID = "ContractManagementAddon.GenerateAR";
                oCreationPackage.String = "Generate AR Invoice";
                oMenus.AddEx(oCreationPackage);

                oCreationPackage.UniqueID = "ContractManagementAddon.Settings";
                oCreationPackage.String = "Settings";
                oMenus.AddEx(oCreationPackage);

                oCreationPackage.UniqueID = "ContractManagementAddon.ReleaseRetention";
                oCreationPackage.String = "Release Retention";
                oMenus.AddEx(oCreationPackage);
            }
            catch (Exception)
            { //  Menu already exists
                Application.SBO_Application.SetStatusBarMessage("Menu Already Exists", SAPbouiCOM.BoMessageTime.bmt_Short, true);
            }
        }

        public void SBO_Application_MenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

            try
            {
                if (pVal.BeforeAction)
                {
                    if (pVal.MenuUID == "ContractManagementAddon.Form1")
                    {
                        Form1 activeForm = new Form1();
                        activeForm.Show();
                    }
                    else if (pVal.MenuUID == "ContractManagementAddon.RecalcICP")
                    {
                        IcpCommands.RecalculateActiveIcp();
                    }
                    else if (pVal.MenuUID == "ContractManagementAddon.GenerateAR")
                    {
                        IcpCommands.GenerateInvoiceForActiveIcp();
                    }
                    else if (pVal.MenuUID == "ContractManagementAddon.Settings")
                    {
                        var f = new ConfigForm();
                        f.Show();
                    }
                    else if (pVal.MenuUID == "ContractManagementAddon.ReleaseRetention")
                    {
                        string contractCode = IcpCommands.TryGetActiveContractCode();
                        var f = new RetentionReleaseForm(contractCode);
                        f.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                Application.SBO_Application.MessageBox(ex.ToString(), 1, "Ok", "", "");
            }
        }

    }
}
