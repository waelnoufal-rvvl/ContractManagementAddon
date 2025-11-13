using System;
using SAPbobsCOM;

namespace ContractManagement.Infrastructure.Sap
{
    public class ArInvoiceService
    {
        private readonly Company _company;

        public ArInvoiceService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        public int CreateInvoiceForIcp(string icpCode, bool draft = false, string deductionAccountCode = null)
        {
            if (string.IsNullOrWhiteSpace(icpCode)) throw new ArgumentException("ICP code is required", nameof(icpCode));

            Recordset rs = null;
            try
            {
                rs = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery($@"SELECT i.""Code"", i.""U_ContractCode"", i.""U_CertDate"", i.""U_Currency"", i.""U_NetPayment"", c.""U_CardCode"", c.""U_TaxCode""
                             FROM ""@RVCM_ICP"" i
                             JOIN ""@RVCM_CNTRCT"" c ON c.""Code"" = i.""U_ContractCode""
                             WHERE i.""Code"" = '{icpCode.Replace("'", "''")}'");
                if (rs.RecordCount == 0)
                    throw new InvalidOperationException("ICP not found");

                var cardCode = Convert.ToString(rs.Fields.Item("U_CardCode").Value);
                var taxCode = Convert.ToString(rs.Fields.Item("U_TaxCode").Value);
                var certDate = Convert.ToDateTime(rs.Fields.Item("U_CertDate").Value);

                Documents doc = (Documents)_company.GetBusinessObject(draft ? BoObjectTypes.oDrafts : BoObjectTypes.oInvoices);
                if (draft)
                {
                    doc.DocObjectCode = BoObjectTypes.oInvoices;
                }

                doc.CardCode = cardCode;
                doc.DocDate = certDate;

                // Lines from ICP1
                var rsLines = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                try
                {
                    rsLines.DoQuery($@"SELECT ""U_ItemCode"", ""U_Descr"", ""U_ThisQty"", ""U_UnitPrice"" FROM ""@RVCM_ICP1"" WHERE ""Code"" = '{icpCode.Replace("'", "''")}' ORDER BY ""LineId""");
                    var first = true;
                    while (!rsLines.EoF)
                    {
                        if (!first) doc.Lines.Add();
                        first = false;
                        var itemCode = Convert.ToString(rsLines.Fields.Item("U_ItemCode").Value);
                        var descr = Convert.ToString(rsLines.Fields.Item("U_Descr").Value);
                        var qty = Convert.ToDouble(rsLines.Fields.Item("U_ThisQty").Value);
                        var price = Convert.ToDouble(rsLines.Fields.Item("U_UnitPrice").Value);

                        if (!string.IsNullOrEmpty(itemCode))
                        {
                            doc.Lines.ItemCode = itemCode;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(deductionAccountCode))
                                throw new InvalidOperationException("Missing ItemCode for ICP line; provide a service account code to post as service line.");
                            doc.Lines.AccountCode = deductionAccountCode; // use as service account for normal lines when no item
                            doc.Lines.ItemDescription = string.IsNullOrEmpty(descr) ? $"ICP {icpCode} Line" : descr;
                        }
                        doc.Lines.Quantity = qty;
                        doc.Lines.UnitPrice = price;
                        if (!string.IsNullOrEmpty(taxCode)) doc.Lines.VatGroup = taxCode;
                        rsLines.MoveNext();
                    }
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rsLines);
                }

                // Deductions as negative service lines
                var rsDed = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                try
                {
                    rsDed.DoQuery($@"SELECT ""U_DeductType"", ""U_Amount"" FROM ""@RVCM_DEDUCT"" WHERE ""U_ICPCode"" = '{icpCode.Replace("'", "''")}'");
                    while (!rsDed.EoF)
                    {
                        doc.Lines.Add();
                        var dtype = Convert.ToString(rsDed.Fields.Item("U_DeductType").Value);
                        var amt = Convert.ToDouble(rsDed.Fields.Item("U_Amount").Value);
                        if (string.IsNullOrEmpty(deductionAccountCode))
                            throw new InvalidOperationException("Deduction account code is required to post deduction lines.");
                        doc.Lines.AccountCode = deductionAccountCode;
                        doc.Lines.ItemDescription = $"Deduction - {dtype}";
                        doc.Lines.Quantity = 1;
                        doc.Lines.UnitPrice = -Math.Abs(amt);
                        if (!string.IsNullOrEmpty(taxCode)) doc.Lines.VatGroup = taxCode;
                        rsDed.MoveNext();
                    }
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rsDed);
                }

                var addRes = doc.Add();
                if (addRes != 0)
                {
                    _company.GetLastError(out var code, out var msg);
                    throw new InvalidOperationException($"Error creating {(draft ? "AR Draft" : "AR Invoice")}: {code} - {msg}");
                }

                // Get new DocEntry
                var rsNew = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                try
                {
                    rsNew.DoQuery("SELECT CURRENT_VALUE FROM OADM WHERE CODE = 'NEXTDOCE' "); // Fallback; better to use GetNewObjectCode
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rsNew);
                }

                // Update ICP with invoice entry
                string newKey;
                _company.GetNewObjectCode(out newKey);
                if (!string.IsNullOrEmpty(newKey))
                {
                    var rsUpd = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                    try
                    {
                        rsUpd.DoQuery($"UPDATE \"@RVCM_ICP\" SET \"U_InvoiceEntry\" = {newKey} WHERE \"Code\" = '{icpCode.Replace("'", "''")}'");
                    }
                    finally
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(rsUpd);
                    }
                }

                return int.TryParse(newKey, out var docEntry) ? docEntry : 0;
            }
            finally
            {
                if (rs != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
            }
        }
    }
}

