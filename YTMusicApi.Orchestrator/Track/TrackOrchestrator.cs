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

        public async Task<TrackDto> PostTrackAsync(string trackId)
        {
            var track = await _youTubeRepository.GetTrackAsync(trackId);
            if (track == null)
            {
                throw new ArgumentNullException("Track not found in YouTobe Music.");
            }
            return await _trackRepository.PostTrackAsync(track);
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
        public async Task<TrackDto> UpdateTrackAsync(string trackId)
        {
            var track = await _youTubeRepository.GetTrackAsync(trackId);
            if (track == null)
            {
                throw new ArgumentNullException("Track not found in YouTobe Music.");
            }
            return await _trackRepository.UpdateTrackAsync(track);
        }
    }
}
