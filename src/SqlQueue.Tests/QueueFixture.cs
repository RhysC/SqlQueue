﻿using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using SqlQueue.Infrastructure;
using SqlQueue.Migrations;
using Xunit;

namespace SqlQueue.Tests
{

    public class QueueFixture
    {
        private readonly SqlQueueImpl _sut;

        public QueueFixture()
        {
            try
            {
                var configuration = new DbMigrationsConfiguration
                {
                    ContextType = typeof(QueueDataContext),
                    MigrationsAssembly = typeof(QueueDataContext).Assembly,
                    TargetDatabase = new DbConnectionInfo("QueueDataContext"),
                    MigrationsNamespace = typeof(InitialCreate).Namespace,
                    AutomaticMigrationDataLossAllowed = true
                };

                var migrator = new DbMigrator(configuration);
                //Update / rollback to "MigrationName"
                migrator.Update("0");
                migrator.Update();
                _sut = new SqlQueueImpl(new Infrastructure.DatabaseContextFactory());
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) Debugger.Break();
                Console.WriteLine(ex);
                throw;
            }
        }

        [Fact]
        public void CanEnqueueAndDequeueMessage()
        {
            var message = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            _sut.Enqueue(message);
            var record = _sut.Dequeue().GetPayload<SampleCommand>();
            Assert.NotNull(record);
            Assert.Equal(message.SomeContents, record.SomeContents);
        }
        [Fact]
        public void DequeuesFirstMessage()
        {
            var message1 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            var message2 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            _sut.Enqueue(message1);
            _sut.Enqueue(message2);
            var record = _sut.Dequeue();
            Assert.NotNull(record);
            Assert.Equal(message1.SomeContents, record.GetPayload<SampleCommand>().SomeContents);
        }
        [Fact]
        public void OnlyDequeuesMessageOnce()
        {
            var message1 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            var message2 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            _sut.Enqueue(message1);
            _sut.Enqueue(message2);
            using (var trans = new TransactionScope())
            {
                var record = _sut.Dequeue().GetPayload<SampleCommand>();
                Assert.NotNull(record);
                Assert.Equal(message1.SomeContents, record.SomeContents);
                trans.Complete();
            }
            SampleCommand lastMessage;
            using (var trans = new TransactionScope())
            {
                lastMessage = _sut.Dequeue().GetPayload<SampleCommand>();
                trans.Complete();
            }
            Assert.NotNull(lastMessage);
            Assert.Equal(message2.SomeContents, lastMessage.SomeContents);
        }
        [Fact]
        public void NotCommitingTrasactionDoesNotDequeueMessage()
        {
            var message1 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            var message2 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            _sut.Enqueue(message1);
            _sut.Enqueue(message2);
            using (new TransactionScope())
            {
                var record = _sut.Dequeue().GetPayload<SampleCommand>();
                Assert.NotNull(record);
                Assert.Equal(message1.SomeContents, record.SomeContents);
                //do not commmit trans
            }
            SampleCommand lastMessage;
            using (var trans = new TransactionScope())
            {
                lastMessage = _sut.Dequeue().GetPayload<SampleCommand>();
                trans.Complete();
            }
            Assert.NotNull(lastMessage);
            Assert.Equal(message1.SomeContents, lastMessage.SomeContents);
        }

        [Fact]
        public void MessageMarkedAsFailedWillNotBeDequeuedAgain()
        {
            var message1 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            var message2 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            _sut.Enqueue(message1);
            _sut.Enqueue(message2);
            using (var trans = new TransactionScope())
            {
                var record = _sut.Dequeue();
                record.MarkAsFailed("The reason for failure");
                trans.Complete();
            }
            SampleCommand lastMessage;
            using (var trans = new TransactionScope())
            {
                lastMessage = _sut.Dequeue().GetPayload<SampleCommand>();
                trans.Complete();
            }
            Assert.NotNull(lastMessage);
            Assert.Equal(message2.SomeContents, lastMessage.SomeContents);
        }
        [Fact]
        public void MessageMarkedAsFailedWillBeInTheFailedMessageRepository()
        {
            var message1 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            _sut.Enqueue(message1);
            const string theReasonForFailure = "The reason for failure";
            using (var trans = new TransactionScope())
            {
                var record = _sut.Dequeue();
                _sut.MarkAsFailed(record, theReasonForFailure);
                trans.Complete();
            }

            var failedMessage = _sut.GetFailedMessages().First();

            Assert.NotNull(failedMessage);
            Assert.Equal(message1, failedMessage.GetPayload());
            Assert.Equal(theReasonForFailure, failedMessage.FailureMessage);
        }
        [Fact]
        public void MessageNotMarkedAsFailedWillNotBeInTheFailedMessageRepository()
        {
            var message1 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            var message2 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            var message3 = new SampleCommand { SomeContents = Guid.NewGuid().ToString() };
            _sut.Enqueue(message1);
            _sut.Enqueue(message2);
            _sut.Enqueue(message3);
            using (var trans = new TransactionScope())
            {
                _sut.Dequeue();
                trans.Complete();
            }
            using (var trans = new TransactionScope())
            {
                _sut.MarkAsFailed( _sut.Dequeue(), "Oh, my failures...");
                trans.Complete();
            }
            using (var trans = new TransactionScope())
            {
                _sut.Dequeue();
                trans.Complete();
            }
            var failedMessages = _sut.GetFailedMessages().Select(m=>m.GetPayload()).ToArray();
            Assert.DoesNotContain(message1, failedMessages);
            Assert.Contains(message2, failedMessages);
            Assert.DoesNotContain(message3, failedMessages);
        }

    }
}
