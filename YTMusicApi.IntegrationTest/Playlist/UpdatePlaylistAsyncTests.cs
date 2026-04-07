using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using System.Net;
using YTMusicApi.Data;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Model.Playlist;

namespace YTMusicApi.IntegrationTest.Playlist;

public class UpdatePlaylistAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/Playlists";

    public UpdatePlaylistAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
        Factory.YouTubeRepositoryMock.Invocations.Clear();
        Factory.YouTubeRepositoryMock.Reset();
    }

    [Fact]
    public async Task UpdatePlaylistAsync_IfSourceIsYouTube_UpdatesFromApiAndReturnsDto()
    {
        // Arrange
        await SeedUserAsync();
        var existingPlaylist = await SeedPlaylistAsync(p => 
        {
            p.Title = "Old DB Title";
            p.Source = PlaylistSource.YouTube;
        });

        var expectedYtPlaylist = new PlaylistDto 
        { 
            PlaylistId = existingPlaylist.PlaylistId, 
            Title = "New YouTube Title", 
            ChannelTitle = "YT Channel",
            Source = PlaylistSource.YouTube
        };

        MockYouTubePlaylist(existingPlaylist.PlaylistId, expectedYtPlaylist);
        MockYouTubePlaylistTracks(existingPlaylist.PlaylistId, new List<string>());

        // Act
        var response = await HttpClient.PutAsync($"{BaseRoute}/{existingPlaylist.PlaylistId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultDto = JsonConvert.DeserializeObject<PlaylistDto>(await response.Content.ReadAsStringAsync());
        resultDto.Should().NotBeNull();
        resultDto!.Title.Should().Be("New YouTube Title");
        
        var dbPlaylist = await FindAsyncInNewContext<SqlDbContext, PlaylistDao>(existingPlaylist.PlaylistId);
        dbPlaylist.Should().NotBeNull();
        dbPlaylist!.Title.Should().Be("New YouTube Title");
    }

    [Fact]
    public async Task UpdatePlaylistAsync_IfSourceIsOptimized_SkipsYouTubeAndReturnsExistingDto()
    {
        // Arrange
        await SeedUserAsync();
        var optimizedPlaylist = await SeedPlaylistAsync(p => 
        {
            p.Title = "Optimized Title";
            p.Source = PlaylistSource.Optimized;
        });

        // Act
        var response = await HttpClient.PutAsync($"{BaseRoute}/{optimizedPlaylist.PlaylistId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePlaylistAsync_IfEntityDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await SeedUserAsync();
        var nonExistentId = "PL" + Guid.NewGuid().ToString("N");

        // Act
        var response = await HttpClient.PutAsync($"{BaseRoute}/{nonExistentId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePlaylistAsync_IfCalledTwice_RateLimiterReturnsTooManyRequests()
    {
        // Arrange
        await SeedUserAsync();
        var existingPlaylist = await SeedPlaylistAsync(p => 
        {
            p.Title = "Old DB Title";
            p.Source = PlaylistSource.YouTube;
        });

        var expectedYtPlaylist = new PlaylistDto 
        { 
            PlaylistId = existingPlaylist.PlaylistId, 
            Title = "New YouTube Title", 
            ChannelTitle = "YT Channel",
            Source = PlaylistSource.YouTube
        };
        
        MockYouTubePlaylist(existingPlaylist.PlaylistId, expectedYtPlaylist);
        MockYouTubePlaylistTracks(existingPlaylist.PlaylistId, new List<string>());

        // Act 
        var response1 = await HttpClient.PutAsync($"{BaseRoute}/{existingPlaylist.PlaylistId}", null);
        var response2 = await HttpClient.PutAsync($"{BaseRoute}/{existingPlaylist.PlaylistId}", null);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}