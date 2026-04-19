using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YTMusicApi.Model.Optimization;

namespace YTMusicApi.Data.Optimization
{
    [Table("optimization_tasks")]
    public class OptimizationTaskDao
    {
        [Key, Column("task_id")]
        public Guid TaskId { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("playlist_id")]
        public string PlaylistId { get; set; } = string.Empty;

        [Column("status")]
        public OptimizationTaskStatus Status { get; set; }

        [Column("error_message")]
        public string? ErrorMessage { get; set; }
    }
}