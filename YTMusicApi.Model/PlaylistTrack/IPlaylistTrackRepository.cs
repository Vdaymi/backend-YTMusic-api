using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMusicApi.Model.PlaylistTrack
{
    public interface IPlaylistTrackRepository
    {
        Task<PlaylistTrackDto> PostTrackToPlaylistAsync(PlaylistTrackDto playlistTrackDto);
        Task<PlaylistTrackDto> DeleteTrackFromPlaylistAsync(PlaylistTrackDto playlistTrackDto);
        Task<List<string>> GetTrackIdsByPlaylistAsync(string playlistId);
    }
}
