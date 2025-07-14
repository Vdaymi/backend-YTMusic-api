using Microsoft.AspNetCore.Mvc;
using YTMusicApi.Model.Track;
using YTMusicApi.Track.Contracts;

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

        [HttpGet("{trackId}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] TrackIdRequest request)
        {
            var trackDto = await _orchestrator.GetByIdTrackAsync(request.TrackId);
            return Ok(trackDto);
        }
    }
}
