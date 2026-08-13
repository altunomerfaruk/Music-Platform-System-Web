using MusicProject.Data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class AdminContentModerationRepository : IAdminContentModerationRepository
    {
        private readonly AppDbContext _context;

        public AdminContentModerationRepository(AppDbContext context)
        {
            _context = context;
        }

        public Song? GetSongById(int songId)
        {
            return _context.Songs
                .FirstOrDefault(song => song.Id == songId);
        }

        public Album? GetAlbumById(int albumId)
        {
            return _context.Albums
                .FirstOrDefault(album => album.Id == albumId && !album.IsDeleted);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}