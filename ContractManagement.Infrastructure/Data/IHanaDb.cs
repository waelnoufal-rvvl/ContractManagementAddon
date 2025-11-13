using System.Data;

namespace ContractManagement.Infrastructure.Data
{
    public interface IHanaDb
    {
        IDbConnection CreateConnection();
        void ExecuteNonQuery(string sql);
    }
}

