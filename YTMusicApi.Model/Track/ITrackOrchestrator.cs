namespace YTMusicApi.Model.Track
{
    public interface ITrackOrchestrator
    {
        Task<TrackDto> PostTrackAsync(string trackId);
        Task<TrackDto> GetByIdTrackAsync(string trackId);
        Task<TrackDto> UpdateTrackAsync(string trackId);
    }
}
