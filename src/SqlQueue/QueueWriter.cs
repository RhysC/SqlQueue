using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using SqlQueue.Infrastructure;

namespace SqlQueue
{
    public class SqlQueueImpl : IEnqueue, IDequeue, IFailedMessageRepository, IDisposable
    {
        private readonly QueueDataContext _dataContext;

        public SqlQueueImpl(IDbContextFactory<QueueDataContext> queueContextFactory)
        {
            _dataContext = queueContextFactory.Create();
        }

        public void Enqueue<T>(T message)
        {
            _dataContext.Messages.Add(new PersistedMessage(message));
            _dataContext.SaveChanges();
        }

        public IMessage Dequeue()
        {
            var message = _dataContext.Messages.Where(m => m.ReadDate == null)
                                               .Where(m=>m.FailureMessage == null)
                                               .OrderBy(m => m.CreatedOn)
                                               .ThenBy(m => m.Id)
                                               .FirstOrDefault(m => m.ReadDate == null);
            if (message == null) return null;
            message.MarkAsRead();
            _dataContext.SaveChanges();
            return message;
        }

        public void MarkAsFailed(IMessage message, string theReasonForFailure)
        {
            message.MarkAsFailed(theReasonForFailure);
            _dataContext.SaveChanges();
        }

        public void Dispose()
        {
            _dataContext.Dispose();
        }


        public IEnumerable<IFailedMessage> GetFailedMessages()
        {
            var x = _dataContext.Messages
                               .Where(m => m.FailureMessage != null)
                               .ToArray();
            return x;
        }
    }

    public interface IDequeue
    {
        IMessage Dequeue();
        void MarkAsFailed(IMessage message, string theReasonForFailure);
    }

    public interface IEnqueue
    {
        void Enqueue<T>(T message);
    }

    public interface IFailedMessageRepository
    {
        IEnumerable<IFailedMessage> GetFailedMessages();
    }
}

