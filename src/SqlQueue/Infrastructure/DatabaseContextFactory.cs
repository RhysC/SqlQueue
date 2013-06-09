using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using log4net;

namespace SqlQueue.Infrastructure
{
    public class DatabaseContextFactory : IDbContextFactory<QueueDataContext>
    {
        private readonly string _connectionString = "QueueDataContext";
        private static readonly ILog Log = LogManager.GetLogger(typeof(DatabaseContextFactory));

        public DatabaseContextFactory()
        {
            Log.Error("Using the default DatabaseContextFactory, this should not be used in a deployed environment");
        }

        public DatabaseContextFactory(IConnectionStringProvider connectionStringProvider)
        {
            // connection string builder will throw a useful error if connection string format is bogus
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionStringProvider.ConnectionString);
            _connectionString = sqlConnectionStringBuilder.ToString();
        }

        public QueueDataContext Create()
        {
            return new QueueDataContext(_connectionString);
        }
    }
}