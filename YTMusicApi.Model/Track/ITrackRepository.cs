namespace YTMusicApi.Model.Track
{
    public interface ITrackRepository
    {
        Task<TrackDto> PostTrackAsync(TrackDto trackDto);
        Task<TrackDto> GetByIdTrackAsync(string id);
        Task<TrackDto> UpdateTrackAsync(TrackDto trackDto);
    }
}
