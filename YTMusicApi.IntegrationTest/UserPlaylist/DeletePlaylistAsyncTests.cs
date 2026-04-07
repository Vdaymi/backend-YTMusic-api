using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using YTMusicApi.Data;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Data.UserPlaylist;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.UserPlaylist;

namespace YTMusicApi.IntegrationTest.UserPlaylist;

public class DeletePlaylistAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/playlists";

    public DeletePlaylistAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeletePlaylistAsync_IfSourceIsYouTube_DeletesLinkButKeepsPlaylistInDb()
    {
        // Arrange
        var user = await SeedUserAsync();
        
        var playlist = await SeedPlaylistAsync(p => p.Source = PlaylistSource.YouTube);
        await SeedUserPlaylistAsync(user.Id, playlist.PlaylistId);

        // Act
        var response = await HttpClient.DeleteAsync($"{BaseRoute}/{playlist.PlaylistId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var resultDto = JsonConvert.DeserializeObject<UserPlaylistDto>(responseContent);
        
        resultDto.Should().NotBeNull();
        resultDto!.PlaylistId.Should().Be(playlist.PlaylistId);
        resultDto.UserId.Should().Be(user.Id);
        
        var deletedLink = await FindAsyncInNewContext<SqlDbContext, UserPlaylistDao>(user.Id, playlist.PlaylistId);
        deletedLink.Should().BeNull("");

        var dbPlaylist = await FindAsyncInNewContext<SqlDbContext, PlaylistDao>(playlist.PlaylistId);
        dbPlaylist.Should().NotBeNull("");
    }

    [Fact]
    public async Task DeletePlaylistAsync_IfSourceIsOptimized_DeletesBothLinkAndPlaylistFromDb()
    {
        // Arrange
        var user = await SeedUserAsync();
        
        var optimizedPlaylist = await SeedPlaylistAsync(p => p.Source = PlaylistSource.Optimized);
        await SeedUserPlaylistAsync(user.Id, optimizedPlaylist.PlaylistId);

        // Act
        var response = await HttpClient.DeleteAsync($"{BaseRoute}/{optimizedPlaylist.PlaylistId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var resultDto = JsonConvert.DeserializeObject<UserPlaylistDto>(responseContent);
        resultDto.Should().NotBeNull();
        
        var deletedLink = await FindAsyncInNewContext<SqlDbContext, UserPlaylistDao>(user.Id, optimizedPlaylist.PlaylistId);
        deletedLink.Should().BeNull();

        var dbPlaylist = await FindAsyncInNewContext<SqlDbContext, PlaylistDao>(optimizedPlaylist.PlaylistId);
        dbPlaylist.Should().BeNull();
    }

    [Fact]
    public async Task DeletePlaylistAsync_IfLinkDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await SeedUserAsync();
        var nonExistentPlaylistId = "PL" + Guid.NewGuid().ToString("N");

        // Act
        var response = await HttpClient.DeleteAsync($"{BaseRoute}/{nonExistentPlaylistId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}