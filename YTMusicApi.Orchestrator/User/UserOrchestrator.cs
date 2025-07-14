using System.Security.Authentication;
using YTMusicApi.Model.Auth;
using YTMusicApi.Model.User;

namespace YTMusicApi.Orchestrator.User
{
    public class UserOrchestrator : IUserOrchestrator
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;

        public UserOrchestrator(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
        }

        public async Task RegisterAsync(string username, string email, string password)
        {
            var hashedPassword = _passwordHasher.Generate(password); 
            
            var userDto = UserDto.Create(username, hashedPassword, email);

            await _userRepository.AddUserAsync(userDto);
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var userDto = await _userRepository.GetByEmailAsync(email);
            if (userDto == null)
            {
                throw new AuthenticationException("Invalid login");
            }
            
            var result = _passwordHasher.Verify(password, userDto.PasswordHash);
            if (result == false)
            {
                throw new AuthenticationException("Invalid password");
            }

            var token = _jwtProvider.GenerateToken(userDto);

            return token;
        }
    }
}
