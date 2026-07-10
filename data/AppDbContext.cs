using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Concrete;


namespace MusicProject.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        //public DbSet<Album> Albums { get; set; }

        //public DbSet<Artist> Artists { get; set; }
        

        public DbSet<User> users { get; set; }

        //public DbSet<ArtistStat> artistStats { get; set; }

    }
}