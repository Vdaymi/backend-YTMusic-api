using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.YouTube;

namespace YTMusicApi.Orchestrator
{
    public class PlaylistTrackOrchestrator : IPlaylistTrackOrchestrator
    {
        private readonly IPlaylistTrackRepository _playlitTrackRepository;
        private readonly IYouTubeRepository _youTubeRepository;
        public PlaylistTrackOrchestrator(
            IPlaylistTrackRepository playlitTrackRepository,
            IYouTubeRepository youTubeRepository)
        {
            _playlitTrackRepository = playlitTrackRepository;
            _youTubeRepository = youTubeRepository;
        }
        public async Task<PlaylistTrackDto> PostTrackToPlaylistAsync(string playlistId, string trackId)
        {
            var playlistTrackDto = new PlaylistTrackDto();
            playlistTrackDto.PlaylistId = playlistId;
            playlistTrackDto.TrackId = trackId;
            if (playlistTrackDto == null)
            {
                throw new ArgumentNullException("Track not found in the databese.");
            }

            return await _playlitTrackRepository.PostTrackToPlaylistAsync(playlistTrackDto);
        }
        public async Task<PlaylistTrackDto> DeleteTrackFromPlaylistAsync(string playlistId, string trackId)
        {
            var playlistTrackDto = new PlaylistTrackDto();
            playlistTrackDto.PlaylistId = playlistId;
            playlistTrackDto.TrackId = trackId;
            if (playlistTrackDto == null)
            {
                throw new ArgumentNullException("Track not found in the databese.");
            }
            return await _playlitTrackRepository.DeleteTrackFromPlaylistAsync(playlistTrackDto);
        }
        public async Task<List<TrackDto>> GetTracksForPlaylistAsync(string playlistId)
        {
            var ids = await _playlitTrackRepository.GetTrackIdsByPlaylistAsync(playlistId);
            return await _youTubeRepository.GetTracksAsync(ids);
        }
    }
}
