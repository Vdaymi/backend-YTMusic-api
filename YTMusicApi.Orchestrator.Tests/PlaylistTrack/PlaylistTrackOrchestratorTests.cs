using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using YTMusicApi.Model.PlaylistTrack;
using YTMusicApi.Model.Track;
using YTMusicApi.Model.YouTube;
using YTMusicApi.Orchestrator.PlaylistTrack;

namespace YTMusicApi.Orchestrator.Tests.PlaylistTrack
{
    public class PlaylistTrackOrchestratorTests
    {
        private readonly Mock<IPlaylistTrackRepository> _playlistTrackRepoMock;
        private readonly Mock<IYouTubeRepository> _youTubeRepoMock;
        private readonly Mock<ITrackRepository> _trackRepoMock;
        private readonly PlaylistTrackOrchestrator _orchestrator;

        public PlaylistTrackOrchestratorTests()
        {
            _playlistTrackRepoMock = new Mock<IPlaylistTrackRepository>();
            _youTubeRepoMock = new Mock<IYouTubeRepository>();
            _trackRepoMock = new Mock<ITrackRepository>();

            _orchestrator = new PlaylistTrackOrchestrator(
                _playlistTrackRepoMock.Object,
                _youTubeRepoMock.Object,
                _trackRepoMock.Object);
        }

        [Fact]
        public async Task GetTracksForPlaylistAsync_WhenTracksExist_ReturnsTrackDtos()
        {
            // Arrange
            var playlistId = "PL_123";
            var expectedTracks = new List<TrackDto>
            {
                new() { TrackId = "T1", Title = "Track 1" },
                new() { TrackId = "T2", Title = "Track 2" }
            };

            _playlistTrackRepoMock.Setup(x => x.GetTracksByPlaylistAsync(playlistId))
                .ReturnsAsync(expectedTracks);

            // Act
            var result = await _orchestrator.GetTracksForPlaylistAsync(playlistId);

            // Assert
            result.Should().BeSameAs(expectedTracks);
            _playlistTrackRepoMock.Verify(x => x.GetTracksByPlaylistAsync(playlistId), Times.Once);
        }

        [Fact]
        public async Task GetTracksForPlaylistAsync_WhenNoTracksExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var playlistId = "PL_123";
            
            _playlistTrackRepoMock.Setup(x => x.GetTracksByPlaylistAsync(playlistId))
                .ReturnsAsync(new List<TrackDto>());

            // Act
            Func<Task> act = async () => await _orchestrator.GetTracksForPlaylistAsync(playlistId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Tracks not found for the given playlist.");
        }

        [Fact]
        public async Task PostTrackToPlaylistAsync_WithValidIds_CallsRepositoryWithCorrectDto()
        {
            // Arrange
            var playlistId = "PL_123";
            var trackId = "T_123";
            var expectedReturn = new PlaylistTrackDto { PlaylistId = playlistId, TrackId = trackId };

            _playlistTrackRepoMock.Setup(x => x.PostTrackToPlaylistAsync(It.IsAny<PlaylistTrackDto>()))
                .ReturnsAsync(expectedReturn);

            // Act
            var result = await _orchestrator.PostTrackToPlaylistAsync(playlistId, trackId);

            // Assert
            result.Should().BeSameAs(expectedReturn);
            _playlistTrackRepoMock.Verify(x => x.PostTrackToPlaylistAsync(It.Is<PlaylistTrackDto>(dto => 
                dto.PlaylistId == playlistId && dto.TrackId == trackId)), Times.Once);
        }

        [Fact]
        public async Task DeleteTrackFromPlaylistAsync_WithValidIds_CallsRepositoryWithCorrectDto()
        {
            // Arrange
            var playlistId = "PL_123";
            var trackId = "T_123";
            var expectedReturn = new PlaylistTrackDto { PlaylistId = playlistId, TrackId = trackId };

            _playlistTrackRepoMock.Setup(x => x.DeleteTrackFromPlaylistAsync(It.IsAny<PlaylistTrackDto>()))
                .ReturnsAsync(expectedReturn);

            // Act
            var result = await _orchestrator.DeleteTrackFromPlaylistAsync(playlistId, trackId);

            // Assert
            result.Should().BeSameAs(expectedReturn);
            _playlistTrackRepoMock.Verify(x => x.DeleteTrackFromPlaylistAsync(It.Is<PlaylistTrackDto>(dto => 
                dto.PlaylistId == playlistId && dto.TrackId == trackId)), Times.Once);
        }

        [Fact]
        public async Task UpdateTracksFromPlaylistAsync_SyncsMissingAndRemovedTracks()
        {
            // Arrange
            var playlistId = "PL_123";
            
            var youTubeIds = new List<string> { "T1", "T2" };
            var dbIds = new List<string> { "T2", "T3" };

            _youTubeRepoMock.Setup(x => x.GetTrackIdsFromPlaylistAsync(playlistId)).ReturnsAsync(youTubeIds);
            _playlistTrackRepoMock.Setup(x => x.GetTrackIdsByPlaylistAsync(playlistId)).ReturnsAsync(dbIds);

            var missingYtTracks = new List<TrackDto> { new TrackDto { TrackId = "T1" } };
            _youTubeRepoMock.Setup(x => x.GetTracksAsync(It.Is<List<string>>(l => l.Contains("T1"))))
                .ReturnsAsync(missingYtTracks);

            _trackRepoMock.Setup(x => x.GetByIdTrackAsync("T1")).ReturnsAsync((TrackDto?)null);

            // Act
            await _orchestrator.UpdateTracksFromPlaylistAsync(playlistId);

            // Assert
            _trackRepoMock.Verify(x => x.PostTrackAsync(It.Is<TrackDto>(t => t.TrackId == "T1")), Times.Once);
            
            _playlistTrackRepoMock.Verify(x => x.PostTrackToPlaylistAsync(It.Is<PlaylistTrackDto>(dto => 
                dto.PlaylistId == playlistId && dto.TrackId == "T1")), Times.Once);
            
            _playlistTrackRepoMock.Verify(x => x.DeleteTrackFromPlaylistAsync(It.Is<PlaylistTrackDto>(dto => 
                dto.PlaylistId == playlistId && dto.TrackId == "T3")), Times.Once);
        }

        [Fact]
        public async Task UpdateTracksDataFromPlaylist_WhenNoTracksInDb_ThrowsKeyNotFoundException()
        {
            // Arrange
            var playlistId = "PL_123";
            _playlistTrackRepoMock.Setup(x => x.GetTrackIdsByPlaylistAsync(playlistId))
                .ReturnsAsync(new List<string>());

            // Act
            Func<Task> act = async () => await _orchestrator.UpdateTracksDataFromPlaylist(playlistId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Playlist not found in the database.");
                
            _youTubeRepoMock.Verify(x => x.GetTracksAsync(It.IsAny<List<string>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTracksDataFromPlaylist_WhenTracksExist_UpdatesDbAndReturnsUpdatedTracks()
        {
            // Arrange
            var playlistId = "PL_123";
            var dbIds = new List<string> { "T1", "T2" };
            
            var updatedTracksFromYt = new List<TrackDto>
            {
                new() { TrackId = "T1", ViewCount = 5000 },
                new() { TrackId = "T2", ViewCount = 8000 }
            };

            _playlistTrackRepoMock.Setup(x => x.GetTrackIdsByPlaylistAsync(playlistId)).ReturnsAsync(dbIds);
            _youTubeRepoMock.Setup(x => x.GetTracksAsync(dbIds)).ReturnsAsync(updatedTracksFromYt);

            // Act
            var result = await _orchestrator.UpdateTracksDataFromPlaylist(playlistId);

            // Assert
            result.Should().BeSameAs(updatedTracksFromYt);
            
            _trackRepoMock.Verify(x => x.UpdateTrackAsync(It.Is<TrackDto>(t => t.TrackId == "T1")), Times.Once);
            _trackRepoMock.Verify(x => x.UpdateTrackAsync(It.Is<TrackDto>(t => t.TrackId == "T2")), Times.Once);
        }

        [Fact]
        public async Task PostOptimizedTracksAsync_WithValidTracks_SetsCorrectOrderIndex()
        {
            // Arrange
            var playlistId = "OP_123";
            var trackIds = new List<string> { "T_Second", "T_First", "T_Third" };

            // Act
            await _orchestrator.PostOptimizedTracksAsync(playlistId, trackIds);

            // Assert
            _playlistTrackRepoMock.Verify(x => x.PostTrackToPlaylistAsync(It.Is<PlaylistTrackDto>(dto => 
                dto.TrackId == "T_Second" && dto.OrderIndex == 0)), Times.Once);
                
            _playlistTrackRepoMock.Verify(x => x.PostTrackToPlaylistAsync(It.Is<PlaylistTrackDto>(dto => 
                dto.TrackId == "T_First" && dto.OrderIndex == 1)), Times.Once);
                
            _playlistTrackRepoMock.Verify(x => x.PostTrackToPlaylistAsync(It.Is<PlaylistTrackDto>(dto => 
                dto.TrackId == "T_Third" && dto.OrderIndex == 2)), Times.Once);
        }
    }
}