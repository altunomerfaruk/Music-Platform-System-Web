using Microsoft.EntityFrameworkCore;
using MusicProject.Contracts.Requests;
using MusicProject.Data;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
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
                .Include(album => album.Songs
                    .Where(song => song.PublicationStatus == PublicationStatus.Published))
                    .ThenInclude(song => song.SongStat)
                .FirstOrDefault(album =>
                    album.Id == albumId &&
                    album.PublicationStatus == PublicationStatus.Published);
        }

        public IEnumerable<Album> GetAllAlbums()
        {
            return _context.Albums
                .AsNoTracking()
                .Where(album => album.PublicationStatus == PublicationStatus.Published)
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

        public Album? GetAlbumById(int albumId)
        {
            return _context.Albums
                .Include(album => album.Songs)
                .FirstOrDefault(album => album.Id == albumId);
        }

        public bool UpdatePublication(Album album)
        {
            var existingAlbum = _context.Albums
                .FirstOrDefault(existingAlbum => existingAlbum.Id == album.Id);

            if (existingAlbum == null)
            {
                return false;
            }

            existingAlbum.PublicationStatus = album.PublicationStatus;
            existingAlbum.ScheduledPublishAtUtc = album.ScheduledPublishAtUtc;
            existingAlbum.PublishedAtUtc = album.PublishedAtUtc;
            existingAlbum.PublicationJobId = album.PublicationJobId;

            _context.SaveChanges();

            return true;
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

        public bool UpdateArtistAlbum(UpdateAlbumRequest request)
        {
            var album = _context.Albums.FirstOrDefault(album =>
                album.Id == request.AlbumId &&
                album.ArtistId == request.ArtistId);

            if (album == null)
            {
                return false;
            }

            album.Name = request.Name;
            album.Description = request.Description;
            album.CoverImageUrl = request.CoverImageUrl;
            album.ReleaseDate = request.ReleaseDate;
            album.PublicationStatus = request.PublicationStatus;
            album.ScheduledPublishAtUtc = request.ScheduledPublishAtUtc;
            album.PublishedAtUtc = request.PublishedAtUtc;
            album.PublicationJobId = request.PublicationJobId;

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