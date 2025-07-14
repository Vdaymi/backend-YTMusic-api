using AutoMapper;
using Microsoft.EntityFrameworkCore;
using YTMusicApi.Model.UserPlaylist;

namespace YTMusicApi.Data.UserPlaylist
{
    public class UserPlaylistRepository : IUserPlaylistRepository
    {
        private readonly SqlDbContext _context;
        private readonly IMapper _mapper;

        public UserPlaylistRepository(
            SqlDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task PostPlaylistToUserAsync(UserPlaylistDto userPlaylistDto)
        {
           var userPlaylistDao = _mapper.Map<UserPlaylistDao>(userPlaylistDto);
            
           var existingUserPlaylist = await _context.UserPlaylists.FindAsync(userPlaylistDao.UserId , userPlaylistDao.PlaylistId);
           if (existingUserPlaylist == null)
           {
               _context.UserPlaylists.Add(userPlaylistDao);
               await _context.SaveChangesAsync();
           }
        }

        public async Task<UserPlaylistDto> DeletePlaylistFromUserAsync(UserPlaylistDto userPlaylistDto)
        {
            var userPlaylistDao = _mapper.Map<UserPlaylistDao>(userPlaylistDto);

            var existingUserPlaylist = await _context.UserPlaylists.FindAsync(userPlaylistDao.UserId, userPlaylistDao.PlaylistId);

            _context.UserPlaylists.Remove(existingUserPlaylist);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserPlaylistDto>(existingUserPlaylist);
        }

        public async Task<List<string>> GetPlaylistIdsByUserAsync(Guid userId)
        {
            return await _context.UserPlaylists
                .Where(up => up.UserId == userId)
                .Select(up => up.PlaylistId)
                .ToListAsync();
        }
    }
}
