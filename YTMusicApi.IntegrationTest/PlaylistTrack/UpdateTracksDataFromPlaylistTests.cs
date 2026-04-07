using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using System.Net;
using YTMusicApi.Data;
using YTMusicApi.Data.Track;
using YTMusicApi.Model.Track;

namespace YTMusicApi.IntegrationTest.PlaylistTrack;

public class UpdateTracksDataFromPlaylistTests : BaseTest
{
    private const string BaseRoute = "/api/v1/playlists";

    public UpdateTracksDataFromPlaylistTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
        Factory.YouTubeRepositoryMock.Invocations.Clear();
        Factory.YouTubeRepositoryMock.Reset();
    }

    [Fact]
    public async Task UpdateTracksDataFromPlaylist_IfTracksExist_UpdatesDbAndReturnsUpdatedDtos()
    {
        // Arrange
        await SeedUserAsync();
        var playlist = await SeedPlaylistAsync();
        
        var track1 = await SeedTrackAsync(t => { t.Title = "Old Title 1"; t.ViewCount = 100; });
        var track2 = await SeedTrackAsync(t => { t.Title = "Old Title 2"; t.ViewCount = 200; });
        
        await SeedPlaylistTrackAsync(playlist.PlaylistId, track1.TrackId);
        await SeedPlaylistTrackAsync(playlist.PlaylistId, track2.TrackId);

        var updatedTrackDtos = new List<TrackDto>
        {
            new TrackDto { TrackId = track1.TrackId, Title = "New Title 1", ViewCount = 5000 },
            new TrackDto { TrackId = track2.TrackId, Title = "New Title 2", ViewCount = 8000 }
        };
        
        MockYouTubeGetTracks(updatedTrackDtos);

        // Act
        var response = await HttpClient.PutAsync($"{BaseRoute}/{playlist.PlaylistId}/tracks", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var resultDtos = JsonConvert.DeserializeObject<List<TrackDto>>(responseContent);

        resultDtos.Should().NotBeNull();
        resultDtos.Should().HaveCount(2);
        resultDtos.Should().ContainSingle(t => t.TrackId == updatedTrackDtos[0].TrackId && t.ViewCount == updatedTrackDtos[0].ViewCount);
        resultDtos.Should().ContainSingle(t => t.TrackId == updatedTrackDtos[1].TrackId && t.ViewCount == updatedTrackDtos[1].ViewCount);
    }

    [Fact]
    public async Task UpdateTracksDataFromPlaylist_IfPlaylistHasNoTracksOrDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await SeedUserAsync();
        var emptyPlaylist = await SeedPlaylistAsync(); 
        
        // Act
        var response = await HttpClient.PutAsync($"{BaseRoute}/{emptyPlaylist.PlaylistId}/tracks", null);

        // Assert 
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTracksDataFromPlaylist_IfCalledTwice_RateLimiterReturnsTooManyRequests()
    {
        // Arrange
        await SeedUserAsync();
        var playlist = await SeedPlaylistAsync();

        // Act
        var response1 = await HttpClient.PutAsync($"{BaseRoute}/{playlist.PlaylistId}/tracks", null);
        var response2 = await HttpClient.PutAsync($"{BaseRoute}/{playlist.PlaylistId}/tracks", null);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}