using AutoMapper;
using Microsoft.EntityFrameworkCore;
using YTMusicApi.Model.User;

namespace YTMusicApi.Data.User
{
    public class UserRepository : IUserRepository
    {
        private readonly SqlDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(
            SqlDbContext context, 
            IMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddUserAsync(UserDto userDto)
        {
            var userDao = _mapper.Map<UserDao>(userDto);
            var createdUser = await _context.Users.AddAsync(userDao);
            await _context.SaveChangesAsync();
        }

        public async Task<UserDto> GetByEmailAsync(string email)
        {
            var userDao = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
            return _mapper.Map<UserDto>(userDao);
        }

        public async Task<UserDto> GetByVerificationTokenAsync(string token)
        {
            var userDao = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
            return _mapper.Map<UserDto>(userDao);
        }

        public async Task UpdateUserAsync(UserDto userDto)
        {
            var userDao = await _context.Users.FindAsync(userDto.Id);
            if (userDao == null)
            {
                throw new InvalidOperationException($"User with ID {userDto.Id} not found for update.");
            }

            _mapper.Map(userDto, userDao);

            await _context.SaveChangesAsync();
        }
    }
}
