using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using YTMusicApi.Model.Playlist;

namespace YTMusicApi.IntegrationTest.UserPlaylist;

public class GetMyPlaylistsAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/playlists";

    public GetMyPlaylistsAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetMyPlaylistsAsync_IfUserHasPlaylists_ReturnsOkAndPlaylistsList()
    {
        // Arrange
        var user = await SeedUserAsync();
        
        var playlist1 = await SeedPlaylistAsync(p => p.Title = "First Linked Playlist");
        var playlist2 = await SeedPlaylistAsync(p => p.Title = "Second Linked Playlist");
        
        await SeedUserPlaylistAsync(user.Id, playlist1.PlaylistId);
        await SeedUserPlaylistAsync(user.Id, playlist2.PlaylistId);

        // Act
        var response = await HttpClient.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var resultDtos = JsonConvert.DeserializeObject<List<PlaylistDto>>(responseContent);

        // Перевіряємо відповідь API
        resultDtos.Should().NotBeNull();
        resultDtos.Should().HaveCount(2);
        resultDtos.Should().ContainSingle(p => p.PlaylistId == playlist1.PlaylistId && p.Title == "First Linked Playlist");
        resultDtos.Should().ContainSingle(p => p.PlaylistId == playlist2.PlaylistId && p.Title == "Second Linked Playlist");
    }

    [Fact]
    public async Task GetMyPlaylistsAsync_IfUserHasNoPlaylists_ReturnsOkAndEmptyList()
    {
        // Arrange
        await SeedUserAsync();

        // Act
        var response = await HttpClient.GetAsync(BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var resultDtos = JsonConvert.DeserializeObject<List<PlaylistDto>>(responseContent);

        resultDtos.Should().NotBeNull();
        resultDtos.Should().BeEmpty();
    }
}