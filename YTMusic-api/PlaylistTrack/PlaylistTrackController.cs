using Microsoft.AspNetCore.Mvc;
using YTMusicApi.Model.PlaylistTrack;

namespace YTMusicApi.PlaylistTrack
{
    public class PlaylistTrackController : ControllerBase
    {
        private readonly IPlaylistTrackOrchestrator _orchestrator;
        public PlaylistTrackController(
            IPlaylistTrackOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("{playlistId}/tracks/{trackId}")]
        public async Task<IActionResult> AddTrackToPlaylistAsync(string playlistId, string trackId)
        {
            var addedTrackToplaylist = await _orchestrator.PostTrackToPlaylistAsync(playlistId, trackId);
            return Ok(addedTrackToplaylist);
        }

        [HttpGet("{playlistId}/tracks")]
        public async Task<IActionResult> GetTracksForPlaylistAsync(string playlistId)
        {
            var tracks = await _orchestrator.GetTracksForPlaylistAsync(playlistId);
            return Ok(tracks);
        }

        [HttpDelete("{playlistId}/tracks/{trackId}")]
        public async Task<IActionResult> DeleteTrack(string playlistId, string trackId)
        {
            var deletedTrackFromplaylist = await _orchestrator.DeleteTrackFromPlaylistAsync(playlistId, trackId);
            return Ok(deletedTrackFromplaylist);
        }
    }
}
