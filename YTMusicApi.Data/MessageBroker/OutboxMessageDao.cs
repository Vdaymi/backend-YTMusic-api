using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YTMusicApi.Data.MessageBroker
{
    [Table("outbox_messages")]
    public class OutboxMessageDao
    {
        [Key, Column("id")]
        public Guid Id { get; set; }
        [Column("type"), Required, MaxLength(255)]
        public string Type { get; set; } = string.Empty;
        [Column("payload", TypeName = "jsonb"), Required]
        public string Payload { get; set; } = string.Empty;
        [Column("exchange"), Required, MaxLength(255)]
        public string Exchange { get; set; } = string.Empty;
        [Column("routing_key"), Required, MaxLength(255)]
        public string RoutingKey { get; set; } = string.Empty;
        [Column("occurred_on")]
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
        [Column("processed_on")]
        public DateTime? ProcessedOn { get; set; }
    }
}