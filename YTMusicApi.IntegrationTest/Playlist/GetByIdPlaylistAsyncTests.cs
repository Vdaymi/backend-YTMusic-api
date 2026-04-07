using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using Xunit;
using YTMusicApi.Model.Playlist;

namespace YTMusicApi.IntegrationTest.Playlist;

public class GetByIdPlaylistAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/Playlists"; 

    public GetByIdPlaylistAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetByIdPlaylistAsync_IfEntityExists_ReturnsEntityDto()
    {
        // Arrange
        var expectedPlaylist = await SeedPlaylistAsync();

        // Act 
        var response = await HttpClient.SendAsync(new HttpRequestMessage(
            HttpMethod.Get, $"{BaseRoute}/{expectedPlaylist.PlaylistId}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var resultPlaylistDto = JsonConvert.DeserializeObject<PlaylistDto>(responseContent);
        
        resultPlaylistDto!.PlaylistId.Should().Be(expectedPlaylist.PlaylistId);
        resultPlaylistDto.Title.Should().Be(expectedPlaylist.Title);
        resultPlaylistDto.ChannelTitle.Should().Be(expectedPlaylist.ChannelTitle);
    }

    [Fact]
    public async Task GetByIdPlaylistAsync_IfEntityDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var notExistentId = "PL" + Guid.NewGuid().ToString("N");
        
        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(
            HttpMethod.Get, $"{BaseRoute}/{notExistentId}"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}