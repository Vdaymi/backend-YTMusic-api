namespace YTMusicApi.Model.Playlist
{
    public class PlaylistDto
    {
        public string PlaylistId { get; set; }
        public string Title { get; set; }
        public string ChannelTitle { get; set; }
        public int? ItemCount { get; set; }
        public PlaylistSource Source { get; set; }
        public PlaylistSettingDto? OptimizationSetting { get; set; }
    }
}
