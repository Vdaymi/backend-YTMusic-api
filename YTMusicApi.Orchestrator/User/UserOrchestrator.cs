using System.Security.Authentication;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using YTMusicApi.Model.Auth;
using YTMusicApi.Model.User;
using YTMusicApi.Platform.Client;

namespace YTMusicApi.Orchestrator.User
{
    public class UserOrchestrator : IUserOrchestrator
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IEmailSender _emailSender;
        private readonly ClientSettings _clientSettings;

        public UserOrchestrator(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider,
            IEmailSender emailSender,
            IOptions<ClientSettings> clientSettings)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _emailSender = emailSender;
            _clientSettings = clientSettings.Value;
        }

        public async Task RegisterAsync(string username, string email, string password)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var hashedPassword = _passwordHasher.Generate(password);
            var verificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            var tokenExpiration = DateTime.UtcNow.AddHours(24);

            var newUser = UserDto.Create(username, hashedPassword, email, verificationToken, tokenExpiration);

            await _userRepository.AddUserAsync(newUser);

            var verificationLink = $"{_clientSettings.ClientBaseUrl}/verify-email?token={verificationToken}";

            await _emailSender.SendVerificationEmailAsync(email, verificationLink);
        }

        public async Task VerifyEmailAsync(string token)
        {
            var user = await _userRepository.GetByVerificationTokenAsync(token);

            if (user == null)
            {
                throw new InvalidOperationException("Invalid verification token.");
            }

            if (user.EmailVerificationTokenExpires < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Verification token has expired.");
            }

            var verifiedUser = new UserDto(user.Id, user.UserName, user.PasswordHash, user.Email, true, null, null);
            await _userRepository.UpdateUserAsync(verifiedUser);
        }

        public async Task ResendVerificationEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || user.IsEmailVerified)
            {
                return;
            }

            var newVerificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            var newTokenExpiration = DateTime.UtcNow.AddHours(24);

            var updatedUser = new UserDto(
                user.Id,
                user.UserName,
                user.PasswordHash,
                user.Email,
                false,
                newVerificationToken,
                newTokenExpiration);

            await _userRepository.UpdateUserAsync(updatedUser);

            var verificationLink = $"{_clientSettings.ClientBaseUrl}/verify-email?token={newVerificationToken}";
            await _emailSender.SendVerificationEmailAsync(user.Email, verificationLink);
        }

        public async Task<LoginResultDto> LoginAsync(string email, string password)
        {
            var userDto = await _userRepository.GetByEmailAsync(email);

            if (userDto != null && !userDto.IsEmailVerified)
            {
                throw new AuthenticationException("Please verify your email before logging in.");
            }
            
            if (userDto == null || !_passwordHasher.Verify(password, userDto.PasswordHash))
            {
                throw new AuthenticationException("Invalid email or password.");
            }

            var token = _jwtProvider.GenerateToken(userDto);

            return new LoginResultDto 
            {
                Token = token,
                UserName = userDto.UserName
            };
        }
    }
}
