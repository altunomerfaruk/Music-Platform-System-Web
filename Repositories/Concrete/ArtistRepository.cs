using Microsoft.EntityFrameworkCore;
using MusicProject.Contracts.Requests;
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

        public IEnumerable<Artist> SearchArtists(ArtistSearchRequest request)
        {
            var query = _context.Artists
                .AsNoTracking()
                .Include(artist => artist.CountryEntity)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                query = query.Where(artist => artist.Name.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(request.Country))
            {
                var country = request.Country.Trim();

                query = query.Where(artist =>
                    artist.CountryEntity != null &&
                    artist.CountryEntity.Name == country);
            }

            if (request.FollowedOnly)
            {
                query = query.Where(artist =>
                    _context.FollowedArtists.Any(followedArtist =>
                        followedArtist.ArtistId == artist.Id &&
                        followedArtist.UserId == request.UserId &&
                        followedArtist.IsActive));
            }

            query = request.Sort switch
            {
                "name-desc" => query.OrderByDescending(artist => artist.Name),

                "year-newest" => query
                    .OrderByDescending(artist => artist.DebutYear.HasValue)
                    .ThenByDescending(artist => artist.DebutYear)
                    .ThenBy(artist => artist.Name),

                "year-oldest" => query
                    .OrderByDescending(artist => artist.DebutYear.HasValue)
                    .ThenBy(artist => artist.DebutYear)
                    .ThenBy(artist => artist.Name),

                _ => query.OrderBy(artist => artist.Name)
            };

            return query.ToList();
        }

        public int GetArtistCount()
        {
            return _context.Artists.Count();
        }

        public IEnumerable<Artist> SearchArtistsByText(string query, int? maxResults)
        {
            var search = query.Trim();

            var artists = _context.Artists
                .AsNoTracking()
                .Include(artist => artist.CountryEntity)
                .Where(artist =>
                    artist.Name.Contains(search) ||
                    (artist.CountryEntity != null &&
                     artist.CountryEntity.Name.Contains(search)))
                .OrderBy(artist => artist.Name);

            return maxResults.HasValue
                ? artists.Take(maxResults.Value).ToList()
                : artists.ToList();
        }

        public IEnumerable<Artist> GetFeaturedArtists(int count)
        {
            return _context.Artists
                .AsNoTracking()
                .Include(artist => artist.CountryEntity)
                .OrderBy(artist => artist.Id)
                .Take(count)
                .ToList();
        }

        public IEnumerable<string> GetUsedCountryNames()
        {
            return _context.Artists
                .AsNoTracking()
                .Where(artist => artist.CountryEntity != null)
                .Select(artist => artist.CountryEntity!.Name)
                .Distinct()
                .OrderBy(countryName => countryName)
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
