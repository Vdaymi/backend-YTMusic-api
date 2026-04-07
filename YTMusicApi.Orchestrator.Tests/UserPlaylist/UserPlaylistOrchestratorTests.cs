using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.UserPlaylist;
using YTMusicApi.Model.YouTube;
using YTMusicApi.Orchestrator.UserPlaylist;

namespace YTMusicApi.Orchestrator.Tests.UserPlaylist
{
    public class UserPlaylistOrchestratorTests
    {
        private readonly Mock<IUserPlaylistRepository> _userPlaylistRepoMock;
        private readonly Mock<IPlaylistRepository> _playlistRepoMock;
        private readonly Mock<IYouTubeRepository> _youTubeRepoMock;
        private readonly UserPlaylistOrchestrator _orchestrator;

        public UserPlaylistOrchestratorTests()
        {
            _userPlaylistRepoMock = new Mock<IUserPlaylistRepository>();
            _playlistRepoMock = new Mock<IPlaylistRepository>();
            _youTubeRepoMock = new Mock<IYouTubeRepository>();

            _orchestrator = new UserPlaylistOrchestrator(
                _userPlaylistRepoMock.Object,
                _playlistRepoMock.Object,
                _youTubeRepoMock.Object);
        }

        [Fact]
        public async Task GetPlaylistsByUserAsync_ReturnsListOfPlaylists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var playlistIds = new List<string> { "PL_1", "PL_2" };
            var expectedPlaylists = new List<PlaylistDto>
            {
                new() { PlaylistId = "PL_1", Title = "Playlist 1" },
                new() { PlaylistId = "PL_2", Title = "Playlist 2" }
            };

            _userPlaylistRepoMock.Setup(x => x.GetPlaylistIdsByUserAsync(userId))
                .ReturnsAsync(playlistIds);
            
            _playlistRepoMock.Setup(x => x.GetPlaylistsByIdsAsync(playlistIds))
                .ReturnsAsync(expectedPlaylists);

            // Act
            var result = await _orchestrator.GetPlaylistsByUserAsync(userId);

            // Assert
            result.Should().BeSameAs(expectedPlaylists);
            
            _userPlaylistRepoMock.Verify(x => x.GetPlaylistIdsByUserAsync(userId), Times.Once);
            _playlistRepoMock.Verify(x => x.GetPlaylistsByIdsAsync(playlistIds), Times.Once);
        }

        [Fact]
        public async Task PostPlaylistToUserAsync_CallsRepositoryWithCorrectDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var playlistId = "PL_123";

            // Act
            await _orchestrator.PostPlaylistToUserAsync(userId, playlistId);

            // Assert
            _userPlaylistRepoMock.Verify(x => x.PostPlaylistToUserAsync(It.Is<UserPlaylistDto>(dto => 
                dto.UserId == userId && dto.PlaylistId == playlistId)), Times.Once);
        }

        [Fact]
        public async Task DeletePlaylistFromUserAsync_WhenPlaylistDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var playlistId = "PL_123";

            _playlistRepoMock.Setup(x => x.GetByIdPlaylistAsync(playlistId))
                .ReturnsAsync((PlaylistDto?)null);

            // Act
            Func<Task> act = async () => await _orchestrator.DeletePlaylistFromUserAsync(userId, playlistId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Playlist not found in Database");
                
            _userPlaylistRepoMock.Verify(x => x.DeletePlaylistFromUserAsync(It.IsAny<UserPlaylistDto>()), Times.Never);
        }

        [Fact]
        public async Task DeletePlaylistFromUserAsync_IfSourceIsYouTube_DeletesLinkOnly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var playlistId = "PL_123";
            var playlist = new PlaylistDto { PlaylistId = playlistId, Source = PlaylistSource.YouTube };
            var deletedLink = new UserPlaylistDto { UserId = userId, PlaylistId = playlistId };

            _playlistRepoMock.Setup(x => x.GetByIdPlaylistAsync(playlistId))
                .ReturnsAsync(playlist);
                
            _userPlaylistRepoMock.Setup(x => x.DeletePlaylistFromUserAsync(It.IsAny<UserPlaylistDto>()))
                .ReturnsAsync(deletedLink);

            // Act
            var result = await _orchestrator.DeletePlaylistFromUserAsync(userId, playlistId);

            // Assert
            result.Should().BeSameAs(deletedLink);
            
            _userPlaylistRepoMock.Verify(x => x.DeletePlaylistFromUserAsync(It.Is<UserPlaylistDto>(dto => 
                dto.UserId == userId && dto.PlaylistId == playlistId)), Times.Once);
                
            // Переконуємося, що сам плейлист НЕ був видалений з БД (оскільки він з YouTube)
            _playlistRepoMock.Verify(x => x.DeletePlaylistAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeletePlaylistFromUserAsync_IfSourceIsOptimized_DeletesLinkAndPlaylist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var playlistId = "OP_123";
            var playlist = new PlaylistDto { PlaylistId = playlistId, Source = PlaylistSource.Optimized };
            var deletedLink = new UserPlaylistDto { UserId = userId, PlaylistId = playlistId };

            _playlistRepoMock.Setup(x => x.GetByIdPlaylistAsync(playlistId))
                .ReturnsAsync(playlist);
                
            _userPlaylistRepoMock.Setup(x => x.DeletePlaylistFromUserAsync(It.IsAny<UserPlaylistDto>()))
                .ReturnsAsync(deletedLink);

            // Act
            var result = await _orchestrator.DeletePlaylistFromUserAsync(userId, playlistId);

            // Assert
            result.Should().BeSameAs(deletedLink);
            
            _userPlaylistRepoMock.Verify(x => x.DeletePlaylistFromUserAsync(It.Is<UserPlaylistDto>(dto => 
                dto.UserId == userId && dto.PlaylistId == playlistId)), Times.Once);
                
            // Переконуємося, що сам плейлист БУВ видалений з БД (оскільки він згенерований нами)
            _playlistRepoMock.Verify(x => x.DeletePlaylistAsync(playlistId), Times.Once);
        }
    }
}