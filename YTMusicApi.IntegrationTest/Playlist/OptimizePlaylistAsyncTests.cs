using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using System.Net;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Playlist.Contracts;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.IntegrationTest.Playlist;

public class OptimizePlaylistAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/Playlists";

    public OptimizePlaylistAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
        Factory.OptimizerClientMock.Invocations.Clear();
        Factory.OptimizerClientMock.Reset();
    }

    [Fact]
    public async Task OptimizePlaylistAsync_WithValidData_ReturnsOkAndOptimizedResultDto()
    {
        // Arrange
        await SeedUserAsync();
        var existingPlaylist = await SeedPlaylistAsync();
        
        var track1 = await SeedTrackAsync(t => t.Title = "Track 1");
        var track2 = await SeedTrackAsync(t => t.Title = "Track 2");
        
        await SeedPlaylistTrackAsync(existingPlaylist.PlaylistId, track1.TrackId);
        await SeedPlaylistTrackAsync(existingPlaylist.PlaylistId, track2.TrackId);

        var requestSettings = new OptimizationRequest
        {
            TimeLimit = TimeSpan.FromMinutes(30),
            MaxTracks = 10,
            Algorithm = OptimizationAlgorithmType.Greedy,
            GenreWeight = 0.5
        };

        var expectedResponse = new OptimizationResponse
        {
            Success = true,
            OrderedTrackIds = new List<string> { track2.TrackId, track1.TrackId },
            TotalScore = 150,
            ExecutionTime = TimeSpan.FromMilliseconds(450)
        };

        MockOptimizerClient(expectedResponse);

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/{existingPlaylist.PlaylistId}/optimize", requestSettings);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultDto = JsonConvert.DeserializeObject<OptimizedPlaylistResultDto>(await response.Content.ReadAsStringAsync());
        
        resultDto.Should().NotBeNull();
        resultDto!.TotalScore.Should().Be(expectedResponse.TotalScore);
        resultDto.ExecutionTime.Should().Be(expectedResponse.ExecutionTime);
        
        resultDto.Tracks.Should().HaveCount(2);
        resultDto.Tracks[0].TrackId.Should().Be(track2.TrackId);
        resultDto.Tracks[1].TrackId.Should().Be(track1.TrackId);
    }

    [Fact]
    public async Task OptimizePlaylistAsync_IfPlaylistIsEmptyOrNotFound_ReturnsNotFound()
    {
        // Arrange
        await SeedUserAsync();
        var nonExistentPlaylistId = "PL" + Guid.NewGuid().ToString("N");
        var requestSettings = new OptimizationRequest
        {
            TimeLimit = TimeSpan.FromMinutes(30),
            MaxTracks = 10,
            Algorithm = OptimizationAlgorithmType.Greedy,
            GenreWeight = 0.5
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/{nonExistentPlaylistId}/optimize", requestSettings);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}