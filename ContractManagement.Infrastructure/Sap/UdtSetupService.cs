using System;
using SAPbobsCOM;

namespace ContractManagement.Infrastructure.Sap
{
    public class UdtSetupService
    {
        private readonly Company _company;

        public UdtSetupService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        public void EnsureTables()
        {
            EnsureHeaderWithLines("RVCM_CNTRCT", "Contract Management", BoUTBTableType.bott_MasterData, BoUTBTableType.bott_MasterDataLines);
            EnsureHeaderWithLines("RVCM_ICP", "Interim Payment Certificate", BoUTBTableType.bott_Document, BoUTBTableType.bott_DocumentLines);
            EnsureSimpleTable("RVCM_DEDUCT", "Deductions");
            EnsureSimpleTable("RVCM_RETEN", "Retention Tracking");
            EnsureSimpleTable("RVCM_AMEND", "Contract Amendments");
            EnsureSimpleTable("RVCM_PYMSCH", "Payment Schedule");
            EnsureSimpleTable("RVCM_CFG", "Contract Management Config");

            // Contract fields (minimal set to support workflows/calculations)
            UdfHelper.EnsureField(_company, "@RVCM_CNTRCT", "U_CardCode", "Business Partner Code", BoFieldTypes.db_Alpha, 15);
            UdfHelper.EnsureField(_company, "@RVCM_CNTRCT", "U_CardName", "Business Partner Name", BoFieldTypes.db_Alpha, 100);
            UdfHelper.EnsureField(_company, "@RVCM_CNTRCT", "U_Currency", "Currency", BoFieldTypes.db_Alpha, 3);
            UdfHelper.EnsureField(_company, "@RVCM_CNTRCT", "U_TaxCode", "Tax Code", BoFieldTypes.db_Alpha, 8);
            UdfHelper.EnsureField(_company, "@RVCM_CNTRCT", "U_TotalValue", "Total Value", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_CNTRCT", "U_RetentionPct", "Retention %", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Percentage);

            // Contract lines
            UdfHelper.EnsureField(_company, "@RVCM_CNTRCT1", "U_ItemCode", "Item Code", BoFieldTypes.db_Alpha, 50);
            UdfHelper.EnsureField(_company, "@RVCM_CNTRCT1", "U_ItemDesc", "Description", BoFieldTypes.db_Alpha, 254);
            UdfHelper.EnsureField(_company, "@RVCM_CNTRCT1", "U_Quantity", "Quantity", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Quantity);
            UdfHelper.EnsureField(_company, "@RVCM_CNTRCT1", "U_UnitPrice", "Unit Price", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Price);

            // ICP header
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_ContractCode", "Contract Code", BoFieldTypes.db_Alpha, 20);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_CertDate", "Certificate Date", BoFieldTypes.db_Date);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_Currency", "Currency", BoFieldTypes.db_Alpha, 3);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_RetentionPct", "Retention %", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Percentage);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_PeriodValue", "This Period Value", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_AdvDeduct", "Advance Deduction", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_MaterialDeduct", "Material Deduction", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_OtherDeduct", "Other Deduction", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_VATAmount", "VAT Amount", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_NetPayment", "Net Payment", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_Status", "Status", BoFieldTypes.db_Alpha, 1);
            UdfHelper.EnsureField(_company, "@RVCM_ICP", "U_InvoiceEntry", "AR Invoice Entry", BoFieldTypes.db_Numeric);

            // ICP lines
            UdfHelper.EnsureField(_company, "@RVCM_ICP1", "U_ThisQty", "This Period Qty", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Quantity);
            UdfHelper.EnsureField(_company, "@RVCM_ICP1", "U_UnitPrice", "Unit Price", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Price);
            UdfHelper.EnsureField(_company, "@RVCM_ICP1", "U_ItemCode", "Item Code", BoFieldTypes.db_Alpha, 50);
            UdfHelper.EnsureField(_company, "@RVCM_ICP1", "U_Descr", "Description", BoFieldTypes.db_Alpha, 254);

            // Deductions table
            UdfHelper.EnsureField(_company, "@RVCM_DEDUCT", "U_ICPCode", "ICP Code", BoFieldTypes.db_Alpha, 20);
            UdfHelper.EnsureField(_company, "@RVCM_DEDUCT", "U_DeductType", "Type", BoFieldTypes.db_Alpha, 20);
            UdfHelper.EnsureField(_company, "@RVCM_DEDUCT", "U_Amount", "Amount", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_DEDUCT", "U_Description", "Description", BoFieldTypes.db_Alpha, 254);

            // Retention table
            UdfHelper.EnsureField(_company, "@RVCM_RETEN", "U_ContractCode", "Contract Code", BoFieldTypes.db_Alpha, 20);
            UdfHelper.EnsureField(_company, "@RVCM_RETEN", "U_ICPCode", "ICP Code", BoFieldTypes.db_Alpha, 20);
            UdfHelper.EnsureField(_company, "@RVCM_RETEN", "U_RetentionPct", "Retention %", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Percentage);
            UdfHelper.EnsureField(_company, "@RVCM_RETEN", "U_RetentionAmt", "Retention Amount", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_RETEN", "U_ReleasedAmt", "Released Amount", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_RETEN", "U_Balance", "Balance", BoFieldTypes.db_Float, 0, BoFldSubTypes.st_Sum);
            UdfHelper.EnsureField(_company, "@RVCM_RETEN", "U_Status", "Status", BoFieldTypes.db_Alpha, 20);

            // Config table
            UdfHelper.EnsureField(_company, "@RVCM_CFG", "U_DeductAccount", "Deduction Account", BoFieldTypes.db_Alpha, 20);
            UdfHelper.EnsureField(_company, "@RVCM_CFG", "U_PostAsDraft", "Post As Draft (Y/N)", BoFieldTypes.db_Alpha, 1);
        }

        private void EnsureSimpleTable(string tableNameNoAt, string description)
        {
            UserTablesMD udt = null;
            try
            {
                udt = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                if (udt.GetByKey(tableNameNoAt)) return;
                udt.TableName = tableNameNoAt;
                udt.TableDescription = description ?? tableNameNoAt;
                udt.TableType = BoUTBTableType.bott_NoObject;
                var res = udt.Add();
                if (res != 0)
                {
                    _company.GetLastError(out var code, out var msg);
                    throw new InvalidOperationException($"Failed to create UDT @{tableNameNoAt}: {code} - {msg}");
                }
            }
            finally
            {
                if (udt != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(udt);
            }
        }

        private void EnsureHeaderWithLines(string headerName, string description, BoUTBTableType headerType, BoUTBTableType lineType)
        {
            UserTablesMD udt = null;
            try
            {
                udt = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                if (!udt.GetByKey(headerName))
                {
                    udt.TableName = headerName;
                    udt.TableDescription = description ?? headerName;
                    udt.TableType = headerType;
                    var res = udt.Add();
                    if (res != 0)
                    {
                        _company.GetLastError(out var code, out var msg);
                        throw new InvalidOperationException($"Failed to create UDT @{headerName}: {code} - {msg}");
                    }
                }

                // Lines table
                if (!udt.GetByKey(headerName + "1"))
                {
                    udt.TableName = headerName + "1";
                    udt.TableDescription = description + " Lines";
                    udt.TableType = lineType;
                    var res2 = udt.Add();
                    if (res2 != 0)
                    {
                        _company.GetLastError(out var code2, out var msg2);
                        throw new InvalidOperationException($"Failed to create UDT @{headerName}1: {code2} - {msg2}");
                    }
                }
            }
            finally
            {
                if (udt != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(udt);
            }
        }
    }
}
