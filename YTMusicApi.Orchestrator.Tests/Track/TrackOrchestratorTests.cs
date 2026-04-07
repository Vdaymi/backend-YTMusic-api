using FluentAssertions;
using Moq;
using Xunit;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.YouTube;
using YTMusicApi.Orchestrator.Track;

namespace YTMusicApi.Orchestrator.Tests.Track
{
    public class TrackOrchestratorTests
    {
        private readonly Mock<ITrackRepository> _trackRepositoryMock;
        private readonly Mock<IYouTubeRepository> _youTubeRepositoryMock;
        private readonly TrackOrchestrator _orchestrator;

        public TrackOrchestratorTests()
        {
            _trackRepositoryMock = new Mock<ITrackRepository>();
            _youTubeRepositoryMock = new Mock<IYouTubeRepository>();

            _orchestrator = new TrackOrchestrator(
                _trackRepositoryMock.Object, 
                _youTubeRepositoryMock.Object);
        }

        [Fact]
        public async Task GetByIdTrackAsync_WhenTrackExists_ReturnsTrackDto()
        {
            // Arrange
            var trackId = Guid.NewGuid().ToString("N").Substring(0, 11);
            var expectedTrack = new TrackDto { TrackId = trackId, Title = "Test Track" };

            _trackRepositoryMock
                .Setup(repo => repo.GetByIdTrackAsync(trackId))
                .ReturnsAsync(expectedTrack);

            // Act
            var result = await _orchestrator.GetByIdTrackAsync(trackId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(expectedTrack);
        }

        [Fact]
        public async Task GetByIdTrackAsync_WhenTrackDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var trackId = "invalid_track_id";

            _trackRepositoryMock
                .Setup(repo => repo.GetByIdTrackAsync(trackId))
                .ReturnsAsync((TrackDto?)null);

            // Act
            Func<Task> act = async () => await _orchestrator.GetByIdTrackAsync(trackId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Track not found in the database.");
        }
    }
}