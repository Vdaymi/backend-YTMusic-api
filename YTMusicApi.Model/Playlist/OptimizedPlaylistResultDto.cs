using System;
using System.Collections.Generic;
using YTMusicApi.Model.Track;

namespace YTMusicApi.Model.Playlist
{
    public class OptimizedPlaylistResultDto
    {
        public List<TrackDto> Tracks { get; set; } = new List<TrackDto>();
        public double TotalScore { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }
}