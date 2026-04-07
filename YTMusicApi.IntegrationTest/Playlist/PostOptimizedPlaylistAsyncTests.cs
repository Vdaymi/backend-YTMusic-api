using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using YTMusicApi.Data;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Data.PlaylistTrack;
using YTMusicApi.Data.UserPlaylist;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Playlist.Contracts;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.IntegrationTest.Playlist;

public class PostOptimizedPlaylistAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/Playlists";

    public PostOptimizedPlaylistAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task PostOptimizedPlaylistAsync_WithValidRequest_SavesAllRelatedEntitiesAndReturnsDto()
    {
        // Arrange
        var user = await SeedUserAsync();
        
        var track1 = await SeedTrackAsync(); 
        var track2 = await SeedTrackAsync();

        var request = new SaveOptimizedPlaylistRequest
        {
            Title = "My Generated Playlist",
            ChannelTitle = "Test Channel",
            TrackIds = new List<string> { track1.TrackId, track2.TrackId },
            TargetDuration = TimeSpan.FromMinutes(45),
            Algorithm = OptimizationAlgorithmType.AntColony,
            GenreWeight = 0.8
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/optimized", request);

        // Assert 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var resultDto = JsonConvert.DeserializeObject<PlaylistDto>(responseContent);
        
        resultDto.Should().NotBeNull();
        resultDto!.Title.Should().Be(request.Title);
        resultDto.ChannelTitle.Should().Be(request.ChannelTitle);
        resultDto.Source.Should().Be(PlaylistSource.Optimized);
        resultDto.ItemCount.Should().Be(2);
        resultDto.PlaylistId.Should().StartWith("OP");

        var dbPlaylist = await FindAsyncInNewContext<SqlDbContext, PlaylistDao>(resultDto.PlaylistId);
        dbPlaylist.Should().NotBeNull(); ;
    }
}