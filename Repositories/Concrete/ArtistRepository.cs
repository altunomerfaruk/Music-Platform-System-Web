using Microsoft.EntityFrameworkCore;
using MusicProject.Data;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class ArtistRepository : IArtistRepository
    {
        private readonly AppDbContext _context;

        public ArtistRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Artist> GetAll()
        {
            return _context.Artists
                .AsNoTracking()
                .Include(artist => artist.CountryEntity)
                .ToList();
        }

        public Artist? GetByID(int id)
        {
            return _context.Artists
                .AsNoTracking()
                .Include(artist => artist.CountryEntity)
                .FirstOrDefault(artist => artist.Id == id);
        }

        public Artist? GetArtistDetailsById(int artistId)
        {
            return _context.Artists
                .AsNoTracking()
                .AsSplitQuery()
                .Include(artist => artist.CountryEntity)
                .Include(artist => artist.Albums
                    .Where(album =>
                        !album.IsAdminHidden &&
                        album.PublicationStatus == PublicationStatus.Published))
                    .ThenInclude(album => album.Songs
                        .Where(song =>
                            !song.IsAdminHidden &&
                            song.PublicationStatus == PublicationStatus.Published))
                .Include(artist => artist.SongArtists
                    .Where(songArtist =>
                        !songArtist.Song.IsAdminHidden &&
                        songArtist.Song.PublicationStatus == PublicationStatus.Published &&
                        (songArtist.Song.AlbumId == null ||
                         (!songArtist.Song.Album!.IsAdminHidden &&
                          songArtist.Song.Album.PublicationStatus == PublicationStatus.Published))))
                    .ThenInclude(songArtist => songArtist.Song)
                        .ThenInclude(song => song.SongStat)
                .Include(artist => artist.SongArtists
                    .Where(songArtist =>
                        !songArtist.Song.IsAdminHidden &&
                        songArtist.Song.PublicationStatus == PublicationStatus.Published &&
                        (songArtist.Song.AlbumId == null ||
                         (!songArtist.Song.Album!.IsAdminHidden &&
                          songArtist.Song.Album.PublicationStatus == PublicationStatus.Published))))
                    .ThenInclude(songArtist => songArtist.Song)
                        .ThenInclude(song => song.Album)
                .Include(artist => artist.Followers)
                .FirstOrDefault(artist => artist.Id == artistId);
        }

        public Artist? GetArtistDashboardByUserId(int userId)
        {
            return _context.Artists
                .AsNoTracking()
                .AsSplitQuery()
                .Include(artist => artist.User)
                .Include(artist => artist.CountryEntity)
                .Include(artist => artist.ArtistStat)
                .Include(artist => artist.Followers)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Songs)
                        .ThenInclude(song => song.SongStat)
                .Include(artist => artist.SongArtists)
                    .ThenInclude(songArtist => songArtist.Song)
                        .ThenInclude(song => song.SongStat)
                .Include(artist => artist.SongArtists)
                    .ThenInclude(songArtist => songArtist.Song)
                        .ThenInclude(song => song.Album)
                .FirstOrDefault(artist => artist.UserId == userId);
        }

        public void Create(Artist entity)
        {
            _context.Artists.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Artist entity)
        {
            _context.Artists.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var artist = _context.Artists.Find(id);

            if (artist == null)
            {
                return;
            }

            _context.Artists.Remove(artist);
            _context.SaveChanges();
        }

        public bool UpdateProfileByUserId(int userId, string name, int? countryId, int? debutYear)
        {
            var artist = _context.Artists
                .FirstOrDefault(artist => artist.UserId == userId);

            if (artist == null)
            {
                return false;
            }

            artist.Name = name;
            artist.CountryId = countryId;
            artist.DebutYear = debutYear;

            _context.SaveChanges();

            return true;
        }
    }
}