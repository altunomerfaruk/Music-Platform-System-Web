using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
// DEĞİŞİKLİK: UserRole enum'unu kullanmak için eklendi.

namespace MusicProject.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Genre> Genres { get; set; }

        public DbSet<LikedSong> LikedSongs { get; set; }

        public DbSet<RecordLabel> RecordLabels { get; set; }

        public DbSet<Song> Songs { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<SongGenre> SongGenres { get; set; }

        public DbSet<Album> Albums { get; set; }

        public DbSet<Artist> Artists { get; set; }

        public DbSet<SongArtist> SongArtists { get; set; }

        public DbSet<User> users { get; set; }

        public DbSet<ArtistStat> artistStats { get; set; }

        public DbSet<SongStat> SongStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(user => user.Role)
                .HasDefaultValue(UserRole.User);

            modelBuilder.Entity<Song>()
                .HasQueryFilter(song => song.IsDeleted == false);

            modelBuilder.Entity<User>()
                .HasOne(user => user.ArtistProfile)
                .WithOne(artist => artist.User)
                .HasForeignKey<Artist>(artist => artist.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Song>()
                .HasOne(song => song.SongStat)
                .WithOne(songStat => songStat.Song)
                .HasForeignKey<SongStat>(songStat => songStat.SongId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}