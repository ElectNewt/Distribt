using System;

namespace Distribt.Shared.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string MessageType { get; set; }
        public string Content { get; set; }
        public DateTime OccurredOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string? Error { get; set; }
        public int ProductId { get; set; }
    }
}