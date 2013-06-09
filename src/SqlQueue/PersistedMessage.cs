using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SqlQueue
{
    [Table("Messages")]
    public class PersistedMessage : IMessage, IFailedMessage
    {
        protected PersistedMessage() { }

        public PersistedMessage(object message)
        {
            CreatedOn = DateTime.UtcNow;
            Type = message.GetType().AssemblyQualifiedName;
            Payload = JsonConvert.SerializeObject(message);
        }

        [Key]
        public int Id { get; protected set; }
        public DateTime CreatedOn { get; protected set; }
        public DateTime? ReadDate { get; protected set; }
        public string Type { get; protected set; }
        [MaxLength]
        public string Payload { get; protected set; }
        [MaxLength]
        public string FailureMessage { get; protected set; }
        public T GetPayload<T>() where T : class
        {
            return GetPayload() as T;
        }

        public void MarkAsFailed(string theReasonForFailure)
        {
            FailureMessage = theReasonForFailure;
        }

        public object GetPayload()
        {
            return JsonConvert.DeserializeObject(Payload, System.Type.GetType(Type));
        }

        public void MarkAsRead()
        {
            ReadDate = DateTime.UtcNow;
        }

        protected bool Equals(PersistedMessage other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PersistedMessage) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}