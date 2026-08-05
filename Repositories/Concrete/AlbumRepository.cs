using Microsoft.EntityFrameworkCore;
using MusicProject.Data;
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


        public bool UpdateArtistAlbum(int albumId,int artistId,string name,string? description,string? coverImageUrl,DateTime releaseDate)
        { 
            var album = _context.Albums.FirstOrDefault(a => a.Id == albumId && a.ArtistId == artistId);
            if (album == null)
            {
                return false;
            }

            album.Name = name;
            album.Description = description;
            album.CoverImageUrl = coverImageUrl;
            album.ReleaseDate = releaseDate;

            _context.SaveChanges();
            return true;
        }

        public void Create(Album album)
        {
            _context.Albums.Add(album);
            _context.SaveChanges();
        }

        public bool DeleteArtistAlbum(int albumId, int artistId)
        {
            var album = _context.Albums
                .Include(album => album.Songs)
                .FirstOrDefault(album => album.Id == albumId && album.ArtistId == artistId);

            if (album == null)
            {
                return false;
            }

            if (album.Songs.Count > 0)
            {
                throw new InvalidOperationException(
                    "Bu albümde şarkılar bulunduğu için albüm silinemez. Önce albümdeki şarkıları başka bir albüme taşımalı veya albüm bağlantılarını kaldırmalısınız.");
            }

            _context.Albums.Remove(album);
            _context.SaveChanges();

            return true;
        }
    }
}