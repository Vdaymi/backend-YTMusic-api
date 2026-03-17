using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Data.Playlist
{
    [Table("playlist_settings")]
    public class PlaylistSettingDao
    {
        [Key, Column("playlist_id"), StringLength(34)]
        public string PlaylistId { get; set; }

        [Column("target_duration")]
        public TimeSpan TargetDuration { get; set; }

        [Column("algorithm")]
        public OptimizationAlgorithmType Algorithm { get; set; }

        [Column("genre_weight")]
        public double GenreWeight { get; set; }

        public PlaylistDao Playlist { get; set; }
    }
}
