using Microsoft.EntityFrameworkCore;
using YTMusicApi.Data.Playlist;
using YTMusicApi.Data.PlaylistTrack;
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

        public virtual DbSet<PlaylistTrackDao> PlaylistTracks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PlaylistTrackDao>()
                .HasKey(pt => new { pt.PlaylistId, pt.TrackId });

            modelBuilder.Entity<PlaylistTrackDao>()
                .HasOne(pt => pt.Playlist)
                .WithMany(p => p.PlaylistTracks)
                .HasForeignKey(pt => pt.PlaylistId);

            modelBuilder.Entity<PlaylistTrackDao>()
                .HasOne(pt => pt.Track)
                .WithMany(t => t.PlaylistTracks)
                .HasForeignKey(pt => pt.TrackId);
        }
    }
}
