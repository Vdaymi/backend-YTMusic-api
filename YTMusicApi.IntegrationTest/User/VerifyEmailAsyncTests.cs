using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using YTMusicApi.Data;
using YTMusicApi.Data.User;
using YTMusicApi.User.Contracts;

namespace YTMusicApi.IntegrationTest.User;

public class VerifyEmailAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/user";

    public VerifyEmailAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
    }
    
    private class VerifyResponseDto
    {
        public string Message { get; set; } = string.Empty;
    }

    [Fact]
    public async Task VerifyEmail_WithValidAndUnexpiredToken_UpdatesDbAndReturnsOk()
    {
        // Arrange
        var validToken = "valid_secure_token_123";
        var user = await SeedUserAsync(u => 
        {
            u.IsEmailVerified = false;
            u.EmailVerificationToken = validToken;
            u.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);
        });

        var request = new VerificationRequest { Token = validToken };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/verify-email", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var resultDto = JsonConvert.DeserializeObject<VerifyResponseDto>(responseContent);
        
        resultDto.Should().NotBeNull();
        resultDto!.Message.Should().Be("Email successfully verified. You can now log in.");

        // Assert
        var updatedUserInDb = await FindAsyncInNewContext<SqlDbContext, UserDao>(user.Id);
        
        updatedUserInDb.Should().NotBeNull();
        updatedUserInDb!.IsEmailVerified.Should().BeTrue();
        updatedUserInDb.EmailVerificationToken.Should().BeNull();
        updatedUserInDb.EmailVerificationTokenExpires.Should().BeNull();
    }

    [Fact]
    public async Task VerifyEmail_WithInvalidToken_ReturnsBadRequestAndDoesNotUpdateDb()
    {
        // Arrange
        var user = await SeedUserAsync(u => 
        {
            u.IsEmailVerified = false;
            u.EmailVerificationToken = "actual_token_in_db";
            u.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);
        });

        var request = new VerificationRequest { Token = "completely_wrong_token" };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/verify-email", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var unmodifiedUser = await FindAsyncInNewContext<SqlDbContext, UserDao>(user.Id);
        unmodifiedUser!.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmail_WithExpiredToken_ReturnsBadRequestAndDoesNotUpdateDb()
    {
        // Arrange
        var expiredToken = "expired_token_123";
        var user = await SeedUserAsync(u => 
        {
            u.IsEmailVerified = false;
            u.EmailVerificationToken = expiredToken;
            u.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(-1);
        });

        var request = new VerificationRequest { Token = expiredToken };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/verify-email", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var unmodifiedUser = await FindAsyncInNewContext<SqlDbContext, UserDao>(user.Id);
        unmodifiedUser!.IsEmailVerified.Should().BeFalse();
    }
}