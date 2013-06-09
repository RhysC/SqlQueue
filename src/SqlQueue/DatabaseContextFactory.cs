using System.Data.SqlClient;
using SqlQueue.Infrastructure;

namespace SqlQueue
{
    public class DatabaseContextFactory
    {
        private string _connectionString;

        public DatabaseContextFactory(IConnectionStringProvider connectionStringProvider)
        {
            // connection string builder will throw a useful error if connection string format is bogus
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionStringProvider.ConnectionString);
            _connectionString = sqlConnectionStringBuilder.ToString();
        }
    }
}