using FluentAssertions;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using YTMusicApi.User.Contracts;

namespace YTMusicApi.IntegrationTest.User;

public class LoginAsyncTests : BaseTest, IClassFixture<YTMusicApiApplicationFactory>
{
    private const string BaseRoute = "/api/v1/user";

    public LoginAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentialsAndVerifiedEmail_ReturnsOkUserNameAndSetCookie()
    {
        // Arrange
        var user = await SeedUserAsync();
        
        var loginRequest = new LoginUser
        {
            Email = user.Email,
            Password = "TestPassword123!"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseUserName = await response.Content.ReadAsStringAsync();
        responseUserName.Should().Be(user.UserName);

        // Assert
        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders).Should().BeTrue();
        var cookieString = setCookieHeaders!.FirstOrDefault();
        
        cookieString.Should().NotBeNullOrEmpty();
        cookieString.Should().Contain("cookies-play=");
        
        var lowerCookie = cookieString!.ToLowerInvariant();
        lowerCookie.Should().Contain("httponly");
        lowerCookie.Should().Contain("secure");
        lowerCookie.Should().Contain("samesite=none");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var user = await SeedUserAsync();
        
        var loginRequest = new LoginUser
        {
            Email = user.Email,
            Password = "WrongPassword123!"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/login", loginRequest);

        // Assert (Middleware перехопить AuthenticationException)
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginAsync_IfEmailIsNotVerified_ReturnsUnauthorized()
    {
        // Arrange
        var user = await SeedUserAsync(u => u.IsEmailVerified = false);
        
        var loginRequest = new LoginUser
        {
            Email = user.Email,
            Password = "TestPassword123!"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}