using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Newtonsoft.Json;
using SqlQueue.Infrastructure;

namespace SqlQueue
{
    public class SqlQueueImpl : IEnqueue, IDequeue, IDisposable
    {
        private readonly QueueDataContext _dataContext;

        public SqlQueueImpl(IDbContextFactory<QueueDataContext> queueContextFactory)
        {
            _dataContext = queueContextFactory.Create();
        }

        public void Enqueue<T>(T message)
        {
            var pesistedMessage = new Message
                {
                    CreatedOn = DateTime.UtcNow,
                    Type = typeof(T).AssemblyQualifiedName,
                    Payload = JsonConvert.SerializeObject(message)
                };
            _dataContext.Messages.Add(pesistedMessage);
            _dataContext.SaveChanges();
        }

        public object Dequeue()
        {
            var message = _dataContext.Messages.Where(m => m.ReadDate == null)
                                               .OrderBy(m => m.CreatedOn)
                                               .ThenBy(m => m.Id)
                                               .FirstOrDefault(m => m.ReadDate == null);
            if (message == null) return null;
            message.ReadDate = DateTime.UtcNow;
            _dataContext.SaveChanges();
            return JsonConvert.DeserializeObject(message.Payload, Type.GetType(message.Type));
        }

        public void Dispose()
        {
            _dataContext.Dispose();
        }
    }

    public interface IDequeue
    {
    }

    public interface IEnqueue
    {

    }
}

