using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using YTMusicApi.Data;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Data.UserPlaylist;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Playlist.Contracts;

namespace YTMusicApi.IntegrationTest.Playlist;

public class PostPlaylistAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/Playlists";

    public PostPlaylistAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
        Factory.YouTubeRepositoryMock.Invocations.Clear();
        Factory.YouTubeRepositoryMock.Reset();
    }

    [Fact]
    public async Task PostPlaylistAsync_IfPlaylistAlreadyInDb_LinksToUserAndReturnsDto()
    {
        // Arrange
        var user = await SeedUserAsync();
        var existingPlaylist = await SeedPlaylistAsync();
        var request = new PlaylistIdRequest { PlaylistId = existingPlaylist.PlaylistId };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultDto = JsonConvert.DeserializeObject<PlaylistDto>(await response.Content.ReadAsStringAsync());
        resultDto.Should().NotBeNull();
        resultDto!.PlaylistId.Should().Be(existingPlaylist.PlaylistId);

        var userPlaylistLink = await FindAsyncInNewContext<SqlDbContext, UserPlaylistDao>(user.Id, existingPlaylist.PlaylistId);
        userPlaylistLink.Should().NotBeNull();
    }

    [Fact]
    public async Task PostPlaylistAsync_IfPlaylistNotInDbButOnYouTube_SavesToDbLinksAndReturnsDto()
    {
        // Arrange
        var user = await SeedUserAsync();
        var newPlaylistId = "PL" + Guid.NewGuid().ToString("N");
        var request = new PlaylistIdRequest { PlaylistId = newPlaylistId };

        var ytPlaylistDto = new PlaylistDto 
        { 
            PlaylistId = newPlaylistId, 
            Title = "YT Title", 
            ChannelTitle = "YT Channel",
            ItemCount = 5
        };
        
        MockYouTubePlaylist(newPlaylistId, ytPlaylistDto);
        MockYouTubePlaylistTracks(newPlaylistId, new List<string>());

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultDto = JsonConvert.DeserializeObject<PlaylistDto>(await response.Content.ReadAsStringAsync());
        resultDto.Should().NotBeNull();
        resultDto!.PlaylistId.Should().Be(newPlaylistId);
        resultDto.Title.Should().Be(ytPlaylistDto.Title);

        var savedPlaylist = await FindAsyncInNewContext<SqlDbContext, PlaylistDao>(newPlaylistId);
        savedPlaylist.Should().NotBeNull();
        savedPlaylist!.Title.Should().Be(ytPlaylistDto.Title);

        var userPlaylistLink = await FindAsyncInNewContext<SqlDbContext, UserPlaylistDao>(user.Id, newPlaylistId);
        userPlaylistLink.Should().NotBeNull();
    }

    [Fact]
    public async Task PostPlaylistAsync_IfPlaylistNotFoundOnYouTube_ReturnsNotFound()
    {
        // Arrange
        await SeedUserAsync();
        var request = new PlaylistIdRequest { PlaylistId = "PL" + Guid.NewGuid().ToString("N") };

        MockYouTubePlaylist(request.PlaylistId, null);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}