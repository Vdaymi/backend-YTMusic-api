using AutoMapper;
using YTMusicApi.Model.Playlist;

namespace YTMusicApi.Data.Playlist
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly SqlDbContext _context;
        private readonly IMapper _mapper;

        public PlaylistRepository(
            SqlDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PlaylistDto> PostPlaylistAsync(PlaylistDto playlistDto)
        {
            var playlistDao = _mapper.Map<PlaylistDao>(playlistDto);
            var createdPlaylist = await _context.Playlists.AddAsync(playlistDao);

            await _context.SaveChangesAsync();

            return _mapper.Map<PlaylistDto>(createdPlaylist.Entity);
        }

        public async Task<PlaylistDto> GetByIdPlaylistAsync(string id)
        {
            var playlistDao = await _context.Playlists.FindAsync(id);
            return _mapper.Map<PlaylistDto>(playlistDao);
        }

        public async Task<PlaylistDto> UpdatePlaylistAsync(PlaylistDto playlistDto)
        {
            var playlistDao = _mapper.Map<PlaylistDao>(playlistDto);
            _context.Playlists.Update(playlistDao);
            await _context.SaveChangesAsync();
            return _mapper.Map<PlaylistDto>(playlistDao); ;
        }
    }
}
