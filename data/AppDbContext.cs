using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Concrete;


namespace MusicProject.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<LikedSong> LikedSongs { get; set; }
        public DbSet<RecordLabel> RecordLabels { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<SongGenre> SongGenres { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<SongArtist> SongArtists { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<ArtistStat> artistStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Song>().HasQueryFilter(x => x.IsDeleted == false);
        }
    }
}