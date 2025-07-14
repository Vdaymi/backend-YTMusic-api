namespace YTMusicApi.Model.Track
{
    public interface ITrackOrchestrator
    {
        Task<TrackDto> GetByIdTrackAsync(string trackId);
    }
}
