namespace YTMusicApi.Model.Track
{
    public class TrackDto
    {
        public string TrackId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string ChannelTitle { get; set; }
        public long? ViewCount { get; set; }
        public long? LikeCount { get; set; }
        public TimeSpan Duration { get; set; }
        public string ImageUrl { get; set; }
    }
}
