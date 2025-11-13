using System;
using SAPbobsCOM;

namespace ContractManagement.Infrastructure.Sap
{
    public class SettingsService
    {
        private readonly Company _company;
        private const string Table = "@RVCM_CFG";
        private const string Key = "DEFAULT";

        public SettingsService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        public void EnsureDefault()
        {
            var rs = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
            try
            {
                rs.DoQuery($"SELECT 1 FROM \"{Table}\" WHERE \"Code\"='{Key}'");
                if (rs.RecordCount == 0)
                {
                    var insert = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                    try
                    {
                        insert.DoQuery($"INSERT INTO \"{Table}\" (\"Code\", \"Name\", \"U_DeductAccount\", \"U_PostAsDraft\") VALUES ('{Key}', '{Key}', '400000', 'N')");
                    }
                    finally
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(insert);
                    }
                }
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
            }
        }

        public AddonSettings Get()
        {
            var rs = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
            try
            {
                rs.DoQuery($"SELECT \"U_DeductAccount\", \"U_PostAsDraft\" FROM \"{Table}\" WHERE \"Code\"='{Key}'");
                if (rs.RecordCount == 0) return new AddonSettings();
                var acct = Convert.ToString(rs.Fields.Item("U_DeductAccount").Value);
                var draft = string.Equals(Convert.ToString(rs.Fields.Item("U_PostAsDraft").Value), "Y", StringComparison.OrdinalIgnoreCase);
                return new AddonSettings { DeductionAccount = acct, PostAsDraft = draft };
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
            }
        }

        public void Save(AddonSettings s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var rs = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
            try
            {
                var y = s.PostAsDraft ? "Y" : "N";
                rs.DoQuery($"UPDATE \"{Table}\" SET \"U_DeductAccount\"='{s.DeductionAccount.Replace("'","''")}', \"U_PostAsDraft\"='{y}' WHERE \"Code\"='{Key}'");
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
            }
        }
    }

    public class AddonSettings
    {
        public string DeductionAccount { get; set; }
        public bool PostAsDraft { get; set; }
    }
}

