using Microsoft.EntityFrameworkCore;
using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly AppDbContext _context;

        public AlbumRepository(AppDbContext context)
        {
            _context = context;
        }

        public Album? GetAlbumDetailsById(int albumId)
        {
            return _context.Albums
                .AsNoTracking()
                .Include(album => album.Artist)
                .Include(album => album.Songs)
                    .ThenInclude(song => song.SongStat)
                .FirstOrDefault(album => album.Id == albumId);
        }
        public IEnumerable<Album> GetAllAlbums()
        {
            return _context.Albums
                .AsNoTracking()
                .Include(album => album.Artist)
                .OrderBy(album => album.Name)
                .ToList();
        }
    }
    
}