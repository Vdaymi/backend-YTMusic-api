using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using YTMusicApi.Model.Optimization;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Shared.MessageBroker;

namespace YTMusicApi.Orchestrator.Optimization
{
    public class OptimizationTaskOrchestrator : IOptimizationTaskOrchestrator
    {
        private readonly IOptimizationRepository _repository;
        private readonly IDistributedCache _cache;
        private readonly IPlaylistTrackOrchestrator _playlistTrackOrchestrator;

        public OptimizationTaskOrchestrator(
            IOptimizationRepository repository,
            IDistributedCache cache,
            IPlaylistTrackOrchestrator playlistTrackOrchestrator)
        {
            _repository = repository;
            _cache = cache;
            _playlistTrackOrchestrator = playlistTrackOrchestrator;
        }
        
        public async Task HandleOptimizationResultAsync(OptimizationCompletedEvent @event)
        {
            var status = @event.Success ? OptimizationTaskStatus.Completed : OptimizationTaskStatus.Failed;
            await _repository.UpdateTaskStatusAsync(@event.TaskId, status, @event.ErrorMessage);

            if (@event.Success)
            {
                var cacheKey = $"optimization:result:{@event.TaskId}";
                var serializedResult = JsonSerializer.Serialize(@event);
                
                await _cache.SetStringAsync(cacheKey, serializedResult, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
            }
        }
        
        public async Task<OptimizationStatusResponseDto> GetOptimizationStatusAsync(Guid taskId, Guid userId)
        {
            var task = await _repository.GetTaskByIdAsync(taskId);
            if (task == null || task.UserId != userId)
                throw new KeyNotFoundException("Task not found or access denied.");

            var response = new OptimizationStatusResponseDto { Status = task.Status };

            if (task.Status == OptimizationTaskStatus.Completed)
            {
                var cacheKey = $"optimization:result:{taskId}";
                var cachedString = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedString))
                {
                    var completedEvent = JsonSerializer.Deserialize<OptimizationCompletedEvent>(cachedString);
                    if (completedEvent != null)
                    {
                        var sourceTracks = await _playlistTrackOrchestrator.GetTracksForPlaylistAsync(task.PlaylistId);
                        var trackDictionary = sourceTracks.ToDictionary(t => t.TrackId);
                        var optimizedTracks = new List<Model.Track.TrackDto>();

                        foreach (var trackId in completedEvent.OrderedTrackIds)
                            if (trackDictionary.TryGetValue(trackId, out var track))
                                optimizedTracks.Add(track);

                        response.Result = new OptimizedPlaylistResultDto
                        {
                            Tracks = optimizedTracks,
                            TotalScore = completedEvent.TotalScore,
                            ExecutionTime = completedEvent.ExecutionTime
                        };
                    }
                }
            }
            return response;
        }
    }
}