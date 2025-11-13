using System;
using SAPbobsCOM;

namespace ContractManagement.Infrastructure.Sap
{
    public static class UdfHelper
    {
        public static void EnsureField(Company company, string tableName, string fieldName, string description, BoFieldTypes type, int size = 0, BoFldSubTypes subType = BoFldSubTypes.st_None)
        {
            if (company == null) throw new ArgumentNullException(nameof(company));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            UserFieldsMD fields = null;
            try
            {
                fields = (UserFieldsMD)company.GetBusinessObject(BoObjectTypes.oUserFields);
                fields.TableName = tableName;
                // DI API expects Name without the U_ prefix
                var alias = fieldName.StartsWith("U_", StringComparison.OrdinalIgnoreCase) ? fieldName.Substring(2) : fieldName;
                fields.Name = alias;
                fields.Description = description ?? fieldName;
                fields.Type = type;
                if (size > 0) fields.EditSize = size;
                fields.SubType = subType;
                var res = fields.Add();
                if (res != 0)
                {
                    company.GetLastError(out var code, out var msg);
                    // -2035 (field exists) or message contains 'already exists' — treat as success
                    if (code == -2035 || (!string.IsNullOrEmpty(msg) && msg.IndexOf("exist", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return;
                    }
                    throw new InvalidOperationException($"Failed to add field {tableName}.{fieldName}: {code} - {msg}");
                }
            }
            finally
            {
                if (fields != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(fields);
            }
        }
    }
}
