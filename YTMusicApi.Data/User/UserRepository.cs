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
    }
}
