using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using YTMusicApi.Data;
using YTMusicApi.User.Contracts;

namespace YTMusicApi.IntegrationTest.User;

public class RegisterAsyncTests : BaseTest
{
    private const string BaseRoute = "/api/v1/user";

    public RegisterAsyncTests(YTMusicApiApplicationFactory factory) : base(factory)
    {
        Factory.EmailSenderMock.Invocations.Clear();
        Factory.EmailSenderMock.Reset();
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_SavesUserSendsEmailAndReturnsOk()
    {
        // Arrange
        var request = new RegisterUser
        {
            UserName = "NewTestUser",
            Email = "newuser@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = Factory.Services.CreateScope();
        var newDbContext = scope.ServiceProvider.GetRequiredService<SqlDbContext>();

        var savedUser = await newDbContext.Users.SingleOrDefaultAsync(u => u.Email == request.Email);

        savedUser.Should().NotBeNull();
        savedUser!.UserName.Should().Be(request.UserName);
        savedUser.PasswordHash.Should().NotBe(request.Password);
        savedUser.IsEmailVerified.Should().BeFalse();
        savedUser.EmailVerificationToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_IfEmailAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var existingEmail = "existing@example.com";
        await SeedUserAsync(u => u.Email = existingEmail);

        var request = new RegisterUser
        {
            UserName = "AnotherUser",
            Email = existingEmail,
            Password = "SomePassword123!"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseRoute}/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}