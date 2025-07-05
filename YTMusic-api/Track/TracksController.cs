using Microsoft.AspNetCore.Mvc;
using YTMusicApi.Model.Track;

namespace YTMusicApi.Track
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TracksController : ControllerBase
    {
        private readonly ITrackOrchestrator _orchestrator;

        public TracksController(
            ITrackOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost]
        public async Task<IActionResult> PostTrackAsync(string trackId)
        {
            var trackDto = await _orchestrator.PostTrackAsync(trackId);
            return Ok(trackDto);
        }

        [HttpGet("{trackId}")]
        public async Task<IActionResult> GetByIdAsync(string trackId)
        {
            var trackDto = await _orchestrator.GetByIdTrackAsync(trackId);
            return Ok(trackDto);
        }

        [HttpPut("{trackId}")]
        public async Task<IActionResult> UpdateTrackAsync(string trackId)
        {
            var updatedTrack = await _orchestrator.UpdateTrackAsync(trackId);
            return Ok(updatedTrack);
        }
    }
}
