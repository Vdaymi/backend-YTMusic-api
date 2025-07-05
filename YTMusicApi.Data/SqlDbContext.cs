using Microsoft.EntityFrameworkCore;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Data.Track;

namespace YTMusicApi.Data
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {
        } 
        public virtual DbSet<TrackDao> Tracks { get; set; }
        public virtual DbSet<PlaylistDao> Playlists { get; set; }
    }
}
