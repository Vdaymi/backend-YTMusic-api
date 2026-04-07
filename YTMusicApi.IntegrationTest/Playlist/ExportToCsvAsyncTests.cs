using FluentAssertions;
using System.Net;
using Xunit;

namespace YTMusicApi.IntegrationTest.Playlist;

public class ExportToCsvAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/Playlists";

    public ExportToCsvAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ExportToCsvAsync_IfPlaylistWithTracksExists_ReturnsOkAndCsvFile()
    {
        // Arrange
        await SeedUserAsync();
        var playlist = await SeedPlaylistAsync();
        
        var track1 = await SeedTrackAsync(t =>
        {
            t.Title = "First Test Track";
            t.ChannelTitle = "Artist One";
        });
        var track2 = await SeedTrackAsync(t =>
        {
            t.Title = "Second Test Track";
            t.ChannelTitle = "Artist Two";
        });

        await SeedPlaylistTrackAsync(playlist.PlaylistId, track1.TrackId);
        await SeedPlaylistTrackAsync(playlist.PlaylistId, track2.TrackId);

        // Act
        var response = await HttpClient.GetAsync($"{BaseRoute}/{playlist.PlaylistId}/export/csv");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ToString().Should().Be("text/csv");
        response.Content.Headers.ContentDisposition.FileName.Should().Be($"playlist_{playlist.PlaylistId}.csv");

        var csvContent = await response.Content.ReadAsStringAsync();
        var lines = csvContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        
        lines.Should().HaveCount(3);
        lines[0].Should().Be("title,artist,album,isrc");
        lines[1].Should().Be($"\"{track1.Title}\",\"{track1.ChannelTitle}\",,");
        lines[2].Should().Be($"\"{track2.Title}\",\"{track2.ChannelTitle}\",,");
    }

    [Fact]
    public async Task ExportToCsvAsync_IfPlaylistIsEmptyOrNotFound_ReturnsNotFound()
    {
        // Arrange
        await SeedUserAsync();
        var nonExistentId = "PL" + Guid.NewGuid().ToString("N");

        // Act
        var response = await HttpClient.GetAsync($"{BaseRoute}/{nonExistentId}/export/csv");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}