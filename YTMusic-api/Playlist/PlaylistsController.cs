using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.Track;
using YTMusicApi.Playlist.Contracts;
using System.Security.Claims;
using YTMusicApi.Model.Optimization;

namespace YTMusicApi.Playlist
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PlaylistsController : ControllerBase
    {
        private readonly IPlaylistOrchestrator _orchestrator;
        private readonly IOptimizationTaskOrchestrator _taskOrchestrator;
        

        public PlaylistsController(
            IPlaylistOrchestrator orchestrator,
            IOptimizationTaskOrchestrator taskOrchestrator)
        {
            _orchestrator = orchestrator;
            _taskOrchestrator = taskOrchestrator;
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> PostPlaylistAsync([FromBody] PlaylistIdRequest request)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ?? User.FindFirst(ClaimTypes.NameIdentifier);

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

        [HttpPost("{playlistId}/optimize"), Authorize]
        public async Task<IActionResult> OptimizePlaylistAsync(string playlistId, [FromBody] OptimizationRequest requestSettings)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ?? User.FindFirst(ClaimTypes.NameIdentifier);

            var userId = Guid.Parse(userIdClaim.Value);
 
            var taskId = await _orchestrator.InitiateOptimizationAsync(playlistId, userId, requestSettings.TimeLimit,
                                                                            requestSettings.MaxTracks, requestSettings.Algorithm,
                                                                            requestSettings.GenreWeight, requestSettings.StartTrackId);
 
            return Accepted(new { TaskId = taskId, Message = "Optimization task has been successfully queued." });

        }

        [HttpGet("optimization/{taskId}"), Authorize]
        public async Task<IActionResult> GetOptimizationStatusAsync(Guid taskId)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ?? User.FindFirst(ClaimTypes.NameIdentifier);

            var userId = Guid.Parse(userIdClaim.Value);
            
            var status = await _taskOrchestrator.GetOptimizationStatusAsync(taskId, userId);
            return  Ok(status);
        }
        
        [HttpPost("optimized"), Authorize]
        public async Task<IActionResult> PostOptimizedPlaylistAsync([FromBody] SaveOptimizedPlaylistRequest request)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ?? User.FindFirst(ClaimTypes.NameIdentifier);

            var userId = Guid.Parse(userIdClaim.Value);
            
            var savedPlaylist = await _orchestrator.PostOptimizedPlaylistAsync(userId, request.Title, request.ChannelTitle,
                                                                               request.TrackIds, request.TargetDuration,
                                                                               request.Algorithm, request.GenreWeight);
            return Ok(savedPlaylist);
        }
        
        [HttpGet("{playlistId}/export/csv"), Authorize]
        public async Task<IActionResult> ExportToCsvAsync(PlaylistIdRequest request)
        {
            var fileBytes = await _orchestrator.GetCsvExportAsync(request.PlaylistId);
            
            return File(fileBytes, "text/csv", $"playlist_{request.PlaylistId}.csv");
        }
    }
} 