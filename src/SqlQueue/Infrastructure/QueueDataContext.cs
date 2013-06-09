using System.Data.Entity;

namespace SqlQueue.Infrastructure
{
    public class QueueDataContext : System.Data.Entity.DbContext
    {
        public QueueDataContext(string connectionString)
            : base(connectionString)
        { }

        public DbSet<Message> Messages { get; set; }
      
    }
}