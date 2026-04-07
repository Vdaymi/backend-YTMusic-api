using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using YTMusicApi.Model.Track;

namespace YTMusicApi.IntegrationTest.Track;

public class GetByIdAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/Tracks"; 

    public GetByIdAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetByIdAsync_IfTrackExists_ReturnsOkAndTrackDto()
    {
        // Arrange
        var expectedTrack = await SeedTrackAsync();

        // Act
        var response = await HttpClient.GetAsync($"{BaseRoute}/{expectedTrack.TrackId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var resultDto = JsonConvert.DeserializeObject<TrackDto>(responseContent);
        
        resultDto.Should().NotBeNull();
        resultDto!.TrackId.Should().Be(expectedTrack.TrackId);
        resultDto.Title.Should().Be(expectedTrack.Title);
        resultDto.ChannelTitle.Should().Be(expectedTrack.ChannelTitle);
        resultDto.ViewCount.Should().Be(expectedTrack.ViewCount);
    }

    [Fact]
    public async Task GetByIdAsync_IfTrackDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentTrackId = Guid.NewGuid().ToString("N").Substring(0, 11);
        
        // Act
        var response = await HttpClient.GetAsync($"{BaseRoute}/{nonExistentTrackId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}