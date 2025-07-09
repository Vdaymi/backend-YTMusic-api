using Microsoft.AspNetCore.Mvc;
using YTMusicApi.Model.Playlist;

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

        [HttpPost]
        public async Task<IActionResult> PostPlaylistAsync(string playlistId)
        {
            var playlistDto = await _orchestrator.PostPlaylistAsync(playlistId);
            return Ok(playlistDto);
        }

        [HttpGet("{playlistId}")]
        public async Task<IActionResult> GetByIdPlaylistAsync(string playlistId)
        {
            var playlistDto = await _orchestrator.GetByIdPlaylistAsync(playlistId);
            return Ok(playlistDto);
        }

        [HttpPut("{playlistId}")]
        public async Task<IActionResult> UpdatePlaylistAsync(string playlistId)
        {
            var updatedPlaylist = await _orchestrator.UpdatePlaylistAsync(playlistId);
            return Ok(updatedPlaylist);
        }
    } 
}
