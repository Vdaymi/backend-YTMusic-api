using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Playlist.Contracts;

namespace YTMusicApi.PlaylistTrack
{
    [ApiController]
    [Route("api/v1/playlists")]
    public class PlaylistTrackController : ControllerBase
    {
        private readonly IPlaylistTrackOrchestrator _orchestrator;
        public PlaylistTrackController(
            IPlaylistTrackOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("{playlistId}/tracks"), Authorize]
        public async Task<IActionResult> GetTracksForPlaylistAsync([FromRoute] PlaylistIdRequest request)
        {
            var tracks = await _orchestrator.GetTracksForPlaylistAsync(request.PlaylistId);
            return Ok(tracks);
        }

        [HttpPut("{playlistId}/tracks"), Authorize]
        public async Task<IActionResult> UpdateTracksDataFromPlaylist([FromRoute] PlaylistIdRequest request)
        {
            var tracks = await _orchestrator.UpdateTracksDataFromPlaylist(request.PlaylistId);
            return Ok(tracks);
        }
    }
}
