using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YTMusicApi.Data.Track
{
    [Table("tracks")]
    public class TrackDao
    {
        [Column("track_id"), Key]
        public string TrackId { get; set; }
        [Column("category_id")]
        public int CategoryId { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("channel_title")]
        public string ChannelTitle { get; set; }
        [Column("view_count")]
        public long? ViewCount { get; set; }
        [Column("like_count")]
        public long? LikeCount { get; set; }
        [Column("duration")]
        public TimeSpan Duration { get; set; }
        [Column("image_url")]
        public string ImageUrl { get; set; }
    }
}
