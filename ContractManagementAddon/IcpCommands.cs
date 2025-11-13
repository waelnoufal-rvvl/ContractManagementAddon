using System;
using UIApp = SAPbouiCOM;
using DI = SAPbobsCOM;
using ContractManagement.Infrastructure.Data;
using ContractManagement.Infrastructure.Sap;

namespace ContractManagementAddon
{
    internal static class IcpCommands
    {
        private static string TryGetFromActiveForm(string tableName, string field)
        {
            try
            {
                var frm = UIApp.SBO_Application.Forms.ActiveForm;
                if (frm == null) return null;
                var dbs = frm.DataSources.DBDataSources;
                for (int i = 0; i < dbs.Count; i++)
                {
                    var ds = dbs.Item(i);
                    if (string.Equals(ds.TableName, tableName, StringComparison.OrdinalIgnoreCase))
                    {
                        var code = ds.GetValue(field, 0);
                        return code?.Trim();
                    }
                }
            }
            catch { }
            return null;
        }

        public static string TryGetActiveIcpCode()
        {
            return TryGetFromActiveForm("@RVCM_ICP", "Code");
        }

        public static string TryGetActiveContractCode()
        {
            // Prefer contract from active ICP form
            var fromIcp = TryGetFromActiveForm("@RVCM_ICP", "U_ContractCode");
            if (!string.IsNullOrEmpty(fromIcp)) return fromIcp;
            // Fallback: contract form itself
            var fromCntr = TryGetFromActiveForm("@RVCM_CNTRCT", "Code");
            return fromCntr;
        }

        public static void RecalculateActiveIcp()
        {
            var icpCode = TryGetActiveIcpCode();
            if (string.IsNullOrEmpty(icpCode))
            {
                UIApp.SBO_Application.SetStatusBarMessage("No active ICP form detected.", SAPbouiCOM.BoMessageTime.bmt_Short, true);
                return;
            }
            DI.Company di = null;
            try
            {
                di = (DI.Company)UIApp.SBO_Application.Company.GetDICompany();
                var db = new ContractManagement.Infrastructure.Data.DiRecordsetHanaDb(di);
                db.ExecuteNonQuery($"CALL RVCM_CALC_ICP('{icpCode.Replace("'", "''")}')");
                db.ExecuteNonQuery($"CALL RVCM_VALIDATE_ICP('{icpCode.Replace("'", "''")}')");
                UIApp.SBO_Application.SetStatusBarMessage("ICP recalculated and validated.", SAPbouiCOM.BoMessageTime.bmt_Short, false);
            }
            catch (Exception ex)
            {
                UIApp.SBO_Application.SetStatusBarMessage($"ICP calc error: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Long, true);
            }
        }

        public static void GenerateInvoiceForActiveIcp()
        {
            var icpCode = TryGetActiveIcpCode();
            if (string.IsNullOrEmpty(icpCode))
            {
                UIApp.SBO_Application.SetStatusBarMessage("No active ICP form detected.", SAPbouiCOM.BoMessageTime.bmt_Short, true);
                return;
            }
            DI.Company di = null;
            try
            {
                di = (DI.Company)UIApp.SBO_Application.Company.GetDICompany();
                var setSvc = new ContractManagement.Infrastructure.Sap.SettingsService(di);
                var cfg = setSvc.Get();
                var svc = new ContractManagement.Infrastructure.Sap.ArInvoiceService(di);
                var docEntry = svc.CreateInvoiceForIcp(icpCode, draft: cfg.PostAsDraft, deductionAccountCode: cfg.DeductionAccount);
                UIApp.SBO_Application.SetStatusBarMessage($"AR Invoice created. DocEntry={docEntry}", SAPbouiCOM.BoMessageTime.bmt_Short, false);
            }
            catch (Exception ex)
            {
                UIApp.SBO_Application.SetStatusBarMessage($"AR creation error: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Long, true);
            }
        }
    }
}
