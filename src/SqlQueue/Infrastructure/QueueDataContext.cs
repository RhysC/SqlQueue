using System.Data.Entity;

namespace SqlQueue.Infrastructure
{
    public class QueueDataContext : DbContext
    {
        public QueueDataContext(string connectionString)
            : base(connectionString)
        { }

        public DbSet<PersistedMessage> Messages { get; set; }
      
    }
}