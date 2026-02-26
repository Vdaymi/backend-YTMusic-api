using Microsoft.AspNetCore.Mvc;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Optimizer.Optimization
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OptimizationController : ControllerBase
    {
        private readonly IOptimizationOrchestrator _orchestrator;

        public OptimizationController(IOptimizationOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("optimize")]
        public async Task<ActionResult<OptimizationResponse>> Optimize( OptimizationSettingsDto request)
        {
            var result = await _orchestrator.OptimizeAsync(request);

            return Ok(result);
        }
    }
}