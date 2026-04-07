using AutoMapper;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using YTMusicApi.Model.Integration;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Model.UserPlaylist;
using YTMusicApi.Model.YouTube;
using YTMusicApi.Orchestrator.Playlist;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Orchestrator.Tests.Playlist
{
    public class PlaylistOrchestratorTests
    {
        private readonly Mock<IYouTubeRepository> _youTubeRepositoryMock;
        private readonly Mock<IPlaylistRepository> _playlistRepositoryMock;
        private readonly Mock<IPlaylistTrackOrchestrator> _playlistTrackOrchestratorMock;
        private readonly Mock<IUserPlaylistOrchestrator> _userPlaylistOrchestratorMock;
        private readonly Mock<IOptimizerClient> _optimizerClientMock;
        private readonly Mock<IMapper> _mapperMock;
        
        private readonly PlaylistOrchestrator _orchestrator;

        public PlaylistOrchestratorTests()
        {
            _youTubeRepositoryMock = new Mock<IYouTubeRepository>();
            _playlistRepositoryMock = new Mock<IPlaylistRepository>();
            _playlistTrackOrchestratorMock = new Mock<IPlaylistTrackOrchestrator>();
            _userPlaylistOrchestratorMock = new Mock<IUserPlaylistOrchestrator>();
            _optimizerClientMock = new Mock<IOptimizerClient>();
            _mapperMock = new Mock<IMapper>();

            _orchestrator = new PlaylistOrchestrator(
                _youTubeRepositoryMock.Object,
                _playlistRepositoryMock.Object,
                _playlistTrackOrchestratorMock.Object,
                _userPlaylistOrchestratorMock.Object,
                _optimizerClientMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task GetByIdPlaylistAsync_WhenExists_ReturnsDto()
        {
            // Arrange
            var playlistId = "PL" + Guid.NewGuid().ToString("N");
            var expectedDto = new PlaylistDto { PlaylistId = playlistId };

            _playlistRepositoryMock.Setup(x => x.GetByIdPlaylistAsync(playlistId)).ReturnsAsync(expectedDto);

            // Act
            var result = await _orchestrator.GetByIdPlaylistAsync(playlistId);

            // Assert
            result.Should().BeSameAs(expectedDto);
        }

        [Fact]
        public async Task GetByIdPlaylistAsync_WhenDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var playlistId = "PL" + Guid.NewGuid().ToString("N");
            _playlistRepositoryMock.Setup(x => x.GetByIdPlaylistAsync(playlistId)).ReturnsAsync((PlaylistDto?)null);

            // Act
            Func<Task> act = async () => await _orchestrator.GetByIdPlaylistAsync(playlistId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Playlist not found in the database.");
        }

        [Fact]
        public async Task PostPlaylistAsync_IfAlreadyInDatabase_LinksToUserAndReturnsExisting()
        {
            // Arrange
            var playlistId = "PL" + Guid.NewGuid().ToString("N");
            var userId = Guid.NewGuid();
            var existingPlaylist = new PlaylistDto { PlaylistId = playlistId };

            _playlistRepositoryMock.Setup(x => x.GetByIdPlaylistAsync(playlistId)).ReturnsAsync(existingPlaylist);

            // Act
            var result = await _orchestrator.PostPlaylistAsync(playlistId, userId);

            // Assert
            result.Should().BeSameAs(existingPlaylist);
            
            _userPlaylistOrchestratorMock.Verify(x => x.PostPlaylistToUserAsync(userId, playlistId), Times.Once);
            _youTubeRepositoryMock.Verify(x => x.GetPlaylistAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task PostPlaylistAsync_IfNotInDatabaseButOnYouTube_SavesLinksAndUpdatesTracks()
        {
            // Arrange
            var playlistId = "PL" + Guid.NewGuid().ToString("N");
            var userId = Guid.NewGuid();
            var ytPlaylist = new PlaylistDto { PlaylistId = playlistId };
            var savedPlaylist = new PlaylistDto { PlaylistId = playlistId, Title = "Saved" };

            _playlistRepositoryMock.Setup(x => x.GetByIdPlaylistAsync(playlistId)).ReturnsAsync((PlaylistDto?)null);
            _youTubeRepositoryMock.Setup(x => x.GetPlaylistAsync(playlistId)).ReturnsAsync(ytPlaylist);
            _playlistRepositoryMock.Setup(x => x.PostPlaylistAsync(ytPlaylist)).ReturnsAsync(savedPlaylist);

            // Act
            var result = await _orchestrator.PostPlaylistAsync(playlistId, userId);

            // Assert
            result.Should().BeSameAs(savedPlaylist);
            
            _playlistRepositoryMock.Verify(x => x.PostPlaylistAsync(ytPlaylist), Times.Once);
            _userPlaylistOrchestratorMock.Verify(x => x.PostPlaylistToUserAsync(userId, playlistId), Times.Once);
            _playlistTrackOrchestratorMock.Verify(x => x.UpdateTracksFromPlaylistAsync(playlistId), Times.Once);
        }

        [Fact]
        public async Task UpdatePlaylistAsync_IfSourceIsOptimized_ReturnsExistingWithoutCallingYouTube()
        {
            // Arrange
            var playlistId = "PL" + Guid.NewGuid().ToString("N");
            var existingPlaylist = new PlaylistDto { PlaylistId = playlistId, Source = PlaylistSource.Optimized };

            _playlistRepositoryMock.Setup(x => x.GetByIdPlaylistAsync(playlistId)).ReturnsAsync(existingPlaylist);

            // Act
            var result = await _orchestrator.UpdatePlaylistAsync(playlistId);

            // Assert
            result.Should().BeSameAs(existingPlaylist);
            _youTubeRepositoryMock.Verify(x => x.GetPlaylistAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePlaylistAsync_IfSourceIsYouTube_UpdatesFromYouTubeAndUpdatesTracks()
        {
            // Arrange
            var playlistId = "PL" + Guid.NewGuid().ToString("N");
            var existingPlaylist = new PlaylistDto { PlaylistId = playlistId, Source = PlaylistSource.YouTube };
            var ytPlaylist = new PlaylistDto { PlaylistId = playlistId, Title = "Updated Title" };

            _playlistRepositoryMock.Setup(x => x.GetByIdPlaylistAsync(playlistId)).ReturnsAsync(existingPlaylist);
            _youTubeRepositoryMock.Setup(x => x.GetPlaylistAsync(playlistId)).ReturnsAsync(ytPlaylist);
            _playlistRepositoryMock.Setup(x => x.UpdatePlaylistAsync(ytPlaylist)).ReturnsAsync(ytPlaylist);

            // Act
            var result = await _orchestrator.UpdatePlaylistAsync(playlistId);

            // Assert
            result.Should().BeSameAs(ytPlaylist);
            _playlistTrackOrchestratorMock.Verify(x => x.UpdateTracksFromPlaylistAsync(playlistId), Times.Once);
        }

        [Fact]
        public async Task GetOptimizedTracksAsync_WhenSourcePlaylistIsEmpty_ThrowsKeyNotFoundException()
        {
            // Arrange
            var playlistId = "PL_123";
            _playlistTrackOrchestratorMock.Setup(x => x.GetTracksForPlaylistAsync(playlistId)).ReturnsAsync(new List<TrackDto>());

            // Act
            Func<Task> act = async () => await _orchestrator.GetOptimizedTracksAsync(playlistId, TimeSpan.FromMinutes(30), 10, Shared.Optimization.OptimizationAlgorithmType.Greedy, 0.5, null);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Source playlist is empty or not found.");
                
            _optimizerClientMock.Verify(x => x.OptimizePlaylistAsync(It.IsAny<OptimizationSettingsDto>()), Times.Never);
        }

        [Fact]
        public async Task GetOptimizedTracksAsync_WhenValid_ReturnsOptimizedPlaylistResultDto()
        {
            // Arrange
            var playlistId = "PL_123";
            var track1 = new TrackDto { TrackId = "T1", Title = "Track 1" };
            var track2 = new TrackDto { TrackId = "T2", Title = "Track 2" };
            var sourceTracks = new List<TrackDto> { track1, track2 };

            var optTrack1 = new TrackOptimizationDto { TrackId = "T1" };
            var optTrack2 = new TrackOptimizationDto { TrackId = "T2" };
            var mappedTracks = new List<TrackOptimizationDto> { optTrack1, optTrack2 };

            var optResponse = new OptimizationResponse
            {
                Success = true,
                OrderedTrackIds = new List<string> { "T2", "T1" },
                TotalScore = 150.5,
                ExecutionTime = TimeSpan.FromMilliseconds(500)
            };

            _playlistTrackOrchestratorMock.Setup(x => x.GetTracksForPlaylistAsync(playlistId)).ReturnsAsync(sourceTracks);
            _mapperMock.Setup(x => x.Map<List<TrackOptimizationDto>>(sourceTracks)).Returns(mappedTracks);
            _optimizerClientMock.Setup(x => x.OptimizePlaylistAsync(It.IsAny<OptimizationSettingsDto>())).ReturnsAsync(optResponse);

            // Act
            var result = await _orchestrator.GetOptimizedTracksAsync(playlistId, TimeSpan.FromMinutes(30), 10, OptimizationAlgorithmType.Greedy, 0.5, null);

            // Assert
            result.Should().NotBeNull();
            result.TotalScore.Should().Be(optResponse.TotalScore);
            result.ExecutionTime.Should().Be(optResponse.ExecutionTime);
            
            result.Tracks.Should().HaveCount(2);
            result.Tracks[0].TrackId.Should().Be("T2");
            result.Tracks[1].TrackId.Should().Be("T1");
        }

        [Fact]
        public async Task PostOptimizedPlaylistAsync_WithValidData_SavesAllEntitiesAndReturnsSavedPlaylist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var title = "My AI Playlist";
            var channelTitle = "Test Channel";
            var trackIds = new List<string> { "T1", "T2" };
            var targetDuration = TimeSpan.FromMinutes(45);
            var algorithm = OptimizationAlgorithmType.AntColony;
            var genreWeight = 0.8;
            
            _playlistRepositoryMock.Setup(x => x.PostPlaylistAsync(It.IsAny<PlaylistDto>()))
                .ReturnsAsync((PlaylistDto p) => p);

            // Act
            var result = await _orchestrator.PostOptimizedPlaylistAsync(userId, title, channelTitle, trackIds, targetDuration, algorithm, genreWeight);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(title);
            result.ChannelTitle.Should().Be(channelTitle);
            result.ItemCount.Should().Be(2);
            result.Source.Should().Be(PlaylistSource.Optimized);
            result.PlaylistId.Should().StartWith("OP");
            
            _playlistRepositoryMock.Verify(x => x.PostPlaylistAsync(It.IsAny<PlaylistDto>()), Times.Once);
            _userPlaylistOrchestratorMock.Verify(x => x.PostPlaylistToUserAsync(userId, result.PlaylistId), Times.Once);
            _playlistTrackOrchestratorMock.Verify(x => x.PostOptimizedTracksAsync(result.PlaylistId, trackIds), Times.Once);
            
            _playlistRepositoryMock.Verify(x => x.PostPlaylistSettingsAsync(It.Is<PlaylistSettingDto>(s => 
                s.PlaylistId == result.PlaylistId &&
                s.TargetDuration == targetDuration &&
                s.Algorithm == algorithm &&
                s.GenreWeight == genreWeight
            )), Times.Once);
        }

        [Fact]
        public async Task GetCsvExportAsync_WhenPlaylistIsEmpty_ThrowsKeyNotFoundException()
        {
            // Arrange
            var playlistId = "PL_123";
            _playlistTrackOrchestratorMock.Setup(x => x.GetTracksForPlaylistAsync(playlistId)).ReturnsAsync(new List<TrackDto>());

            // Act
            Func<Task> act = async () => await _orchestrator.GetCsvExportAsync(playlistId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Playlist is empty or not found.");
        }

        [Fact]
        public async Task GetCsvExportAsync_WithValidTracks_ReturnsCorrectlyFormattedCsvBytes()
        {
            // Arrange
            var playlistId = "PL_123";
            var tracks = new List<TrackDto>
            {
                new() { Title = "Title", ChannelTitle = "Artist 1" },
                new() { Title = "Title with \"Quotes\"", ChannelTitle = "Artist 2" },
                new() { Title = null, ChannelTitle = null }
            };

            _playlistTrackOrchestratorMock.Setup(x => x.GetTracksForPlaylistAsync(playlistId)).ReturnsAsync(tracks);

            // Act
            var resultBytes = await _orchestrator.GetCsvExportAsync(playlistId);

            // Assert
            resultBytes.Should().NotBeNull();
            resultBytes.Should().NotBeEmpty();

            var csvString = System.Text.Encoding.UTF8.GetString(resultBytes);
            csvString.Should().Contain("title,artist,album,isrc");
            csvString.Should().Contain("\"Title\",\"Artist 1\",,"); 
            csvString.Should().Contain("\"Title with \"\"Quotes\"\"\",\"Artist 2\",,");
            csvString.Should().Contain("\"Unknown\",\"Unknown\",,");
        }
    }
}