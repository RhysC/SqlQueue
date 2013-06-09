using System;
using System.ComponentModel.DataAnnotations;

namespace SqlQueue
{
    public class Message : IDisposable
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ReadDate { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
        public void Dispose()
        {
            ReadDate = DateTime.UtcNow;
        }
    }
}