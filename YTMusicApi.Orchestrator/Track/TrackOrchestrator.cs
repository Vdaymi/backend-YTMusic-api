using YTMusicApi.Model.Track;
using YTMusicApi.Model.YouTube;

namespace YTMusicApi.Orchestrator.Track
{
    public class TrackOrchestrator : ITrackOrchestrator
    {
        private readonly ITrackRepository _trackRepository;
        private readonly IYouTubeRepository _youTubeRepository;

        public TrackOrchestrator(
            ITrackRepository trackRepository,
            IYouTubeRepository youTubeRepository)
        {
            _trackRepository = trackRepository;
            _youTubeRepository = youTubeRepository;
        }

        public async Task<TrackDto> GetByIdTrackAsync(string trackId)
        {
            var track = await _trackRepository.GetByIdTrackAsync(trackId);
            if (track == null) 
            {
                throw new ArgumentNullException("Track not found in the database.");
            }
            return track;
        }
    }
}
