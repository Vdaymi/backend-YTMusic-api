using FluentAssertions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace YTMusicApi.IntegrationTest.User;

public class LogoutAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/user";

    public LogoutAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Logout_Always_ReturnsOkAndExpiredCookie()
    {
        // Act
        var response = await HttpClient.PostAsync($"{BaseRoute}/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders).Should().BeTrue();
        
        var cookieString = setCookieHeaders!.FirstOrDefault();
        cookieString.Should().NotBeNullOrEmpty();
        
        var lowerCookie = cookieString!.ToLowerInvariant();

        lowerCookie.Should().Contain("cookies-play=;");
        lowerCookie.Should().Contain("expires=");
        lowerCookie.Should().Contain("httponly");
        lowerCookie.Should().Contain("secure");
        lowerCookie.Should().Contain("samesite=none");
    }
}