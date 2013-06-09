using System.Configuration;

namespace SqlQueue.Infrastructure
{
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        public string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["QueueDataContext"].ConnectionString; }
        }
    }
}