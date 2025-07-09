using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Model.Playlist;
using YTMusicApi.Model.PlaylistTrack;

namespace YTMusicApi.Data.PlaylistTrack
{
    public class PlaylistTrackRepository : IPlaylistTrackRepository
    {
        private readonly SqlDbContext _context;
        private readonly IMapper _mapper;

        public PlaylistTrackRepository(
            SqlDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PlaylistTrackDto> PostTrackToPlaylistAsync(PlaylistTrackDto playlistTrackDto)
        {
            var playlistTrackDao = _mapper.Map<PlaylistTrackDao>(playlistTrackDto);
            var existingPlaylistTrack = await _context.PlaylistTracks.FindAsync(playlistTrackDao.PlaylistId, playlistTrackDao.TrackId);
            if (existingPlaylistTrack == null)
            {
                var createdPlaylistTrack = _context.PlaylistTracks.Add(playlistTrackDao);
                await _context.SaveChangesAsync();
                return _mapper.Map<PlaylistTrackDto>(createdPlaylistTrack.Entity);

            }
            return _mapper.Map<PlaylistTrackDto>(existingPlaylistTrack);
        }

        public async Task<PlaylistTrackDto> DeleteTrackFromPlaylistAsync(PlaylistTrackDto playlistTrackDto)
        {
            var playlistTrackDao = _mapper.Map<PlaylistTrackDao>(playlistTrackDto);
            var existingPlaylistTrack = await _context.PlaylistTracks.FindAsync(playlistTrackDao.PlaylistId, playlistTrackDao.TrackId);

            var deletedPlaylistTrack = _context.PlaylistTracks.Remove(existingPlaylistTrack);
            await _context.SaveChangesAsync();

            return _mapper.Map<PlaylistTrackDto>(deletedPlaylistTrack.Entity);
        }

        public async Task<List<string>> GetTrackIdsByPlaylistAsync(string playlistId)
        { 
            return await _context.PlaylistTracks
                .Where(pt => pt.PlaylistId == playlistId)
                .Select(pt => pt.TrackId)
                .ToListAsync();
        } 
    }
}
