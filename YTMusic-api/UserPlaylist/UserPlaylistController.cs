using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YTMusicApi.Model.UserPlaylist;
using YTMusicApi.Playlist.Contracts;

namespace YTMusicApi.UserPlaylist
{
    [ApiController]
    [Route("api/v1/playlists")]
    public class UserPlaylistController : ControllerBase
    {
        private readonly IUserPlaylistOrchestrator _orchestrator;

        public UserPlaylistController(
            IUserPlaylistOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetMyPlaylistsAsync()
        {
            var userIdClaim = User.FindFirst("userId");
            
            var userId = Guid.Parse(userIdClaim.Value);

            var playlists = await _orchestrator.GetPlaylistsByUserAsync(userId);
            return Ok(playlists);
        }

        [HttpDelete("{playlistId}"), Authorize]
        public async Task<IActionResult> DeletePlaylistAsync([FromRoute] PlaylistIdRequest request)
        {
            var userIdClaim = User.FindFirst("userId");
            
            var userId = Guid.Parse(userIdClaim.Value);
            
            var deletedPlaylist = await _orchestrator.DeletePlaylistFromUserAsync(userId, request.PlaylistId);
            return Ok(deletedPlaylist);
        }
    }
}
