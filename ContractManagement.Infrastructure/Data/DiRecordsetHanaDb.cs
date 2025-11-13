using System;
using System.Data;
using SAPbobsCOM;

namespace ContractManagement.Infrastructure.Data
{
    public class DiRecordsetHanaDb : IHanaDb
    {
        private readonly Company _company;

        public DiRecordsetHanaDb(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        public IDbConnection CreateConnection()
        {
            throw new NotSupportedException("Direct connection not supported; use ExecuteNonQuery via DI Recordset.");
        }

        public void ExecuteNonQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return;
            Recordset rs = null;
            try
            {
                rs = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(sql);
            }
            finally
            {
                if (rs != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
            }
        }
    }
}

