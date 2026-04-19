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
    public async Task OptimizePlaylistAsync_WithValidData_ReturnsAcceptedAndTaskId()
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

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/{existingPlaylist.PlaylistId}/optimize", requestSettings);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeAnonymousType(content, new { TaskId = Guid.Empty, Message = "" });
        
        result.Should().NotBeNull();
        result!.TaskId.Should().NotBeEmpty();
        result.Message.Should().Be("Optimization task has been successfully queued.");
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