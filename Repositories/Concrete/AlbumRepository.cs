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
        public IEnumerable<Album> GetAlbumsByArtistId(int artistId)
        {
            return _context.Albums
                .AsNoTracking()
                .Include(album => album.Songs)
                    .ThenInclude(song => song.SongStat)
                .Where(album => album.ArtistId == artistId)
                .OrderByDescending(album => album.ReleaseDate)
                .ThenBy(album => album.Name)
                .ToList();
        }

        public Album? GetArtistAlbumDetails(int albumId, int artistId)
        {
            return _context.Albums
                .AsNoTracking()
                .Include(album => album.Artist)
                .Include(album => album.Songs)
                    .ThenInclude(song => song.SongStat)
                .FirstOrDefault(album =>
                    album.Id == albumId &&
                    album.ArtistId == artistId);
        }

        public void Create(Album album)
        {
            _context.Albums.Add(album);
            _context.SaveChanges();
        }

        public void Update(Album album)
        {
            _context.Albums.Update(album);
            _context.SaveChanges();
        }

        public void Delete(int albumId)
        {
            var album = _context.Albums.Find(albumId);

            if (album == null)
            {
                return;
            }

            _context.Albums.Remove(album);
            _context.SaveChanges();
        }
    }
}