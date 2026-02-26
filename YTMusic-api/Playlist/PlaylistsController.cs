using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.Track;
using YTMusicApi.Orchestrator.Playlist;
using YTMusicApi.Playlist.Contracts;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Playlist
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PlaylistsController : ControllerBase
    {
        private readonly IPlaylistOrchestrator _orchestrator;

        public PlaylistsController(
            IPlaylistOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> PostPlaylistAsync([FromBody] PlaylistIdRequest request)
        {
            var userIdClaim = User.FindFirst("userId");

            var userId = Guid.Parse(userIdClaim.Value);

            var playlistDto = await _orchestrator.PostPlaylistAsync(request.PlaylistId, userId);
            return Ok(playlistDto);
        }

        [HttpGet("{playlistId}"), Authorize]
        public async Task<IActionResult> GetByIdPlaylistAsync([FromRoute] PlaylistIdRequest request)
        {
            var playlistDto = await _orchestrator.GetByIdPlaylistAsync(request.PlaylistId);
            return Ok(playlistDto);
        }

        [HttpPut("{playlistId}"), Authorize, EnableRateLimiting("PerUserUpdatePlaylistPolicy")]
        public async Task<IActionResult> UpdatePlaylistAsync([FromRoute] PlaylistIdRequest request)
        {
            var updatedPlaylist = await _orchestrator.UpdatePlaylistAsync(request.PlaylistId);
            return Ok(updatedPlaylist);
        }

        [HttpPost("{playlistId}/optimize")]
        public async Task<ActionResult<List<TrackDto>>> OptimizePlaylistAsync(string playlistId, [FromQuery] OptimizationRequest requestSettings)
        {
            var optimizedTracks = await _orchestrator.GetOptimizedTracksAsync(playlistId, requestSettings.TimeLimit,
                                                                              requestSettings.MaxTracks, requestSettings.Algorithm,
                                                                              requestSettings.GenreWeight, 
                                                                              requestSettings.StartTrackId);
            return Ok(optimizedTracks);
        }
    }
} 