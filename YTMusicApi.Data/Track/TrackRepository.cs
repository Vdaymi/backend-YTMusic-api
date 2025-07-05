using AutoMapper;
using YTMusicApi.Model.Track;

namespace YTMusicApi.Data.Track
{
    public class TrackRepository : ITrackRepository
    {
        private readonly SqlDbContext _context;
        private readonly IMapper _mapper;

        public TrackRepository(
            SqlDbContext context,
            IMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TrackDto> PostTrackAsync(TrackDto trackDto)
        {
            var trackDao = _mapper.Map<TrackDao>(trackDto);
            var createdTrack = await _context.Tracks.AddAsync(trackDao);
            
            await _context.SaveChangesAsync();

            return _mapper.Map<TrackDto>(createdTrack.Entity);
        }

        public async Task<TrackDto> GetByIdTrackAsync(string id)
        {
            var trackDao = await _context.Tracks.FindAsync(id);
            return _mapper.Map<TrackDto>(trackDao);
        }

        public async Task<TrackDto> UpdateTrackAsync(TrackDto trackDto)
        {
            var trackDao = _mapper.Map<TrackDao>(trackDto);
            _context.Tracks.Update(trackDao);
            await _context.SaveChangesAsync();
            return _mapper.Map<TrackDto>(trackDao);
        }
    }
}
