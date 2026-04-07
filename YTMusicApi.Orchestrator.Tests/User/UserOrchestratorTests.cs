using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using Xunit;
using YTMusicApi.Model.Auth;
using YTMusicApi.Model.User;
using YTMusicApi.Orchestrator.User;
using YTMusicApi.Platform.Client;

namespace YTMusicApi.Orchestrator.Tests.User
{
    public class UserOrchestratorTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtProvider> _jwtProviderMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<IOptions<ClientSettings>> _clientSettingsOptionsMock;
        private readonly ClientSettings _clientSettings;
        private readonly UserOrchestrator _orchestrator;

        public UserOrchestratorTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _jwtProviderMock = new Mock<IJwtProvider>();
            _emailSenderMock = new Mock<IEmailSender>();
            
            _clientSettings = new ClientSettings { ClientBaseUrl = "https://test-client.com" };
            _clientSettingsOptionsMock = new Mock<IOptions<ClientSettings>>();
            _clientSettingsOptionsMock.Setup(x => x.Value).Returns(_clientSettings);

            _orchestrator = new UserOrchestrator(
                _userRepoMock.Object,
                _passwordHasherMock.Object,
                _jwtProviderMock.Object,
                _emailSenderMock.Object,
                _clientSettingsOptionsMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_WhenUserExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var email = "test@test.com";
            _userRepoMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(new UserDto(Guid.NewGuid(), "User", "hash", email, true, null, null));

            // Act
            Func<Task> act = async () => await _orchestrator.RegisterAsync("User", email, "password");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A user with this email already exists.");
                
            _userRepoMock.Verify(x => x.AddUserAsync(It.IsAny<UserDto>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenValid_HashesPasswordSavesUserAndSendsEmail()
        {
            // Arrange
            var username = "NewUser";
            var email = "new@test.com";
            var password = "PlainPassword123";
            var hashedPassword = "HashedPassword123";

            _userRepoMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync((UserDto?)null);
            _passwordHasherMock.Setup(x => x.Generate(password)).Returns(hashedPassword);

            // Act
            await _orchestrator.RegisterAsync(username, email, password);

            // Assert
            _userRepoMock.Verify(x => x.AddUserAsync(It.Is<UserDto>(u => 
                u.UserName == username && 
                u.Email == email && 
                u.PasswordHash == hashedPassword && 
                u.IsEmailVerified == false && 
                u.EmailVerificationToken != null &&
                u.EmailVerificationTokenExpires > DateTime.UtcNow)), Times.Once);

            _emailSenderMock.Verify(x => x.SendVerificationEmailAsync(email, It.Is<string>(link => 
                link.StartsWith(_clientSettings.ClientBaseUrl) && link.Contains("token="))), Times.Once);
        }

        [Fact]
        public async Task VerifyEmailAsync_WhenTokenIsInvalid_ThrowsInvalidOperationException()
        {
            // Arrange
            var token = "invalid_token";
            _userRepoMock.Setup(x => x.GetByVerificationTokenAsync(token)).ReturnsAsync((UserDto?)null);

            // Act
            Func<Task> act = async () => await _orchestrator.VerifyEmailAsync(token);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid verification token.");
        }

        [Fact]
        public async Task VerifyEmailAsync_WhenTokenIsExpired_ThrowsInvalidOperationException()
        {
            // Arrange
            var token = "expired_token";
            var expiredUser = new UserDto(Guid.NewGuid(), "User", "hash", "email@test.com", false, token, DateTime.UtcNow.AddHours(-1));
            
            _userRepoMock.Setup(x => x.GetByVerificationTokenAsync(token)).ReturnsAsync(expiredUser);

            // Act
            Func<Task> act = async () => await _orchestrator.VerifyEmailAsync(token);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Verification token has expired.");
        }

        [Fact]
        public async Task VerifyEmailAsync_WhenValid_UpdatesUserToVerifiedAndClearsTokens()
        {
            // Arrange
            var token = "valid_token";
            var validUser = new UserDto(Guid.NewGuid(), "User", "hash", "email@test.com", false, token, DateTime.UtcNow.AddHours(1));
            
            _userRepoMock.Setup(x => x.GetByVerificationTokenAsync(token)).ReturnsAsync(validUser);

            // Act
            await _orchestrator.VerifyEmailAsync(token);

            // Assert
            _userRepoMock.Verify(x => x.UpdateUserAsync(It.Is<UserDto>(u => 
                u.Id == validUser.Id &&
                u.IsEmailVerified == true &&
                u.EmailVerificationToken == null &&
                u.EmailVerificationTokenExpires == null)), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ResendVerificationEmailAsync_WhenUserNotFoundOrAlreadyVerified_DoesNothing(bool isVerified)
        {
            // Arrange
            var email = "test@test.com";
            var user = isVerified ? new UserDto(Guid.NewGuid(), "User", "hash", email, true, null, null) : null;
            
            _userRepoMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);

            // Act
            await _orchestrator.ResendVerificationEmailAsync(email);

            // Assert
            _userRepoMock.Verify(x => x.UpdateUserAsync(It.IsAny<UserDto>()), Times.Never);
            _emailSenderMock.Verify(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_WhenValid_UpdatesTokenAndSendsEmail()
        {
            // Arrange
            var email = "test@test.com";
            var unverifiedUser = new UserDto(Guid.NewGuid(), "User", "hash", email, false, "old_token", DateTime.UtcNow.AddHours(-1));
            
            _userRepoMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(unverifiedUser);

            // Act
            await _orchestrator.ResendVerificationEmailAsync(email);

            // Assert
            _userRepoMock.Verify(x => x.UpdateUserAsync(It.Is<UserDto>(u => 
                u.Id == unverifiedUser.Id &&
                u.IsEmailVerified == false &&
                u.EmailVerificationToken != "old_token" &&
                u.EmailVerificationToken != null)), Times.Once);

            _emailSenderMock.Verify(x => x.SendVerificationEmailAsync(email, It.Is<string>(link => 
                link.StartsWith(_clientSettings.ClientBaseUrl))), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenUserEmailIsNotVerified_ThrowsAuthenticationException()
        {
            // Arrange
            var email = "test@test.com";
            var unverifiedUser = new UserDto(Guid.NewGuid(), "User", "hash", email, false, null, null);
            
            _userRepoMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(unverifiedUser);

            // Act
            Func<Task> act = async () => await _orchestrator.LoginAsync(email, "password");

            // Assert
            await act.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("Please verify your email before logging in.");
        }

        [Fact]
        public async Task LoginAsync_WhenUserNotFoundOrPasswordInvalid_ThrowsAuthenticationException()
        {
            // Arrange
            var email = "test@test.com";
            var password = "wrong_password";
            var verifiedUser = new UserDto(Guid.NewGuid(), "User", "hash", email, true, null, null);
            
            _userRepoMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(verifiedUser);
            _passwordHasherMock.Setup(x => x.Verify(password, verifiedUser.PasswordHash)).Returns(false);

            // Act
            Func<Task> act = async () => await _orchestrator.LoginAsync(email, password);

            // Assert
            await act.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("Invalid email or password.");
        }

        [Fact]
        public async Task LoginAsync_WhenValid_ReturnsLoginResultDtoWithToken()
        {
            // Arrange
            var email = "test@test.com";
            var password = "correct_password";
            var verifiedUser = new UserDto(Guid.NewGuid(), "ValidUser", "hash", email, true, null, null);
            var generatedToken = "jwt_token_string";
            
            _userRepoMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(verifiedUser);
            _passwordHasherMock.Setup(x => x.Verify(password, verifiedUser.PasswordHash)).Returns(true);
            _jwtProviderMock.Setup(x => x.GenerateToken(verifiedUser)).Returns(generatedToken);

            // Act
            var result = await _orchestrator.LoginAsync(email, password);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be(generatedToken);
            result.UserName.Should().Be(verifiedUser.UserName);
        }
    }
}