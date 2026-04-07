using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using YTMusicApi.Model.Track;

namespace YTMusicApi.IntegrationTest.PlaylistTrack;

public class GetTracksForPlaylistAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/playlists";

    public GetTracksForPlaylistAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetTracksForPlaylistAsync_IfTracksExist_ReturnsOkAndTrackDtos()
    {
        // Arrange
        await SeedUserAsync();
        var playlist = await SeedPlaylistAsync();
        
        var track1 = await SeedTrackAsync(t => t.Title = "First Track");
        var track2 = await SeedTrackAsync(t => t.Title = "Second Track");
        
        await SeedPlaylistTrackAsync(playlist.PlaylistId, track1.TrackId);
        await SeedPlaylistTrackAsync(playlist.PlaylistId, track2.TrackId);

        // Act
        var response = await HttpClient.GetAsync($"{BaseRoute}/{playlist.PlaylistId}/tracks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var resultDtos = JsonConvert.DeserializeObject<List<TrackDto>>(responseContent);

        resultDtos.Should().NotBeNull();
        resultDtos.Should().HaveCount(2);
        
        resultDtos.Should().ContainSingle(t => t.TrackId == track1.TrackId && t.Title == "First Track");
        resultDtos.Should().ContainSingle(t => t.TrackId == track2.TrackId && t.Title == "Second Track");
    }

    [Fact]
    public async Task GetTracksForPlaylistAsync_IfPlaylistHasNoTracksOrDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await SeedUserAsync();
        var emptyPlaylistId = "PL" + Guid.NewGuid().ToString("N");

        // Act
        var response = await HttpClient.GetAsync($"{BaseRoute}/{emptyPlaylistId}/tracks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}