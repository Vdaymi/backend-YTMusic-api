using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMusicApi.Model.Track;

namespace YTMusicApi.Model.PlaylistTrack
{
    public interface IPlaylistTrackOrchestrator
    {
        Task<PlaylistTrackDto> PostTrackToPlaylistAsync(string playlistId, string trackId);
        Task<PlaylistTrackDto> DeleteTrackFromPlaylistAsync(string playlistId, string trackId);
        Task<List<TrackDto>> GetTracksForPlaylistAsync(string playlistId);
    }
}
