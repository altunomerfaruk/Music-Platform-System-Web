using Microsoft.EntityFrameworkCore;
using MusicProject.Contracts.Requests;
using MusicProject.Data;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class SongRepository : GenericRepository<Song>, ISongRepository
    {
        public SongRepository(AppDbContext context)
            : base(context)
        {
        }

        public Song? GetByID(int id)
        {
            return _dbSet.Find(id);
        }

        public IEnumerable<Song> GetSongsSortedByAlphabet()
        {
            return _dbSet
                .AsNoTracking()
                .Where(song =>
                    !song.IsAdminHidden &&
                    song.PublicationStatus == PublicationStatus.Published &&
                    (song.AlbumId == null ||
                     (song.Album!.PublicationStatus == PublicationStatus.Published &&
                      !song.Album.IsAdminHidden)))
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)
                .Include(song => song.SongStat)
                .OrderBy(song => song.Title)
                .ThenByDescending(song => song.AlbumId)
                .ToList();
        }

        public IEnumerable<Song> SearchSongs(SongSearchRequest request)
        {
            var query = VisibleSongs();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                query = query.Where(song =>
                    song.Title.Contains(search) ||
                    (song.Album != null && song.Album.Name.Contains(search)) ||
                    (song.Album != null && song.Album.Artist.Name.Contains(search)) ||
                    song.SongArtists.Any(songArtist =>
                        songArtist.Artist.Name.Contains(search)));
            }

            if (request.ArtistId.HasValue)
            {
                query = query.Where(song =>
                    (song.Album != null && song.Album.ArtistId == request.ArtistId.Value) ||
                    song.SongArtists.Any(songArtist =>
                        songArtist.ArtistId == request.ArtistId.Value));
            }

            if (request.AlbumId.HasValue)
            {
                query = query.Where(song => song.AlbumId == request.AlbumId.Value);
            }

            if (request.GenreId.HasValue)
            {
                query = query.Where(song =>
                    song.SongGenres.Any(songGenre =>
                        songGenre.GenreId == request.GenreId.Value));
            }

            if (request.LikedOnly)
            {
                query = query.Where(song =>
                    _context.LikedSongs.Any(likedSong =>
                        likedSong.SongId == song.Id &&
                        likedSong.UserId == request.UserId &&
                        likedSong.IsActive));
            }

            query = request.Sort switch
            {
                "name-desc" => query
                    .OrderByDescending(song => song.Title),

                "streams-desc" => query
                    .OrderByDescending(song => song.SongStat!.TotalStreams)
                    .ThenBy(song => song.Title),

                "streams-asc" => query
                    .OrderBy(song => song.SongStat!.TotalStreams)
                    .ThenBy(song => song.Title),

                "newest" => query
                    .OrderByDescending(song => song.CreatedAt)
                    .ThenBy(song => song.Title),

                "oldest" => query
                    .OrderBy(song => song.CreatedAt)
                    .ThenBy(song => song.Title),

                _ => query.OrderBy(song => song.Title)
            };

            return query
                .AsSplitQuery()
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)
                .Include(song => song.SongStat)
                .ToList();
        }

        public int GetVisibleSongCount()
        {
            return VisibleSongs().Count();
        }

        public IEnumerable<Song> SearchVisibleSongsByText(string query, int? maxResults)
        {
            var search = query.Trim();

            var songs = VisibleSongs()
                .Where(song =>
                    song.Title.Contains(search) ||
                    (song.Album != null && song.Album.Name.Contains(search)))
                .OrderBy(song => song.Title)
                .ThenByDescending(song => song.AlbumId);

            var limited = maxResults.HasValue
                ? songs.Take(maxResults.Value)
                : songs;

            return limited
                .AsSplitQuery()
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .Include(song => song.SongStat)
                .ToList();
        }

        private IQueryable<Song> VisibleSongs()
        {
            return _dbSet
                .AsNoTracking()
                .Where(song =>
                    !song.IsAdminHidden &&
                    song.PublicationStatus == PublicationStatus.Published &&
                    (song.AlbumId == null ||
                     (song.Album!.PublicationStatus == PublicationStatus.Published &&
                      !song.Album.IsAdminHidden)));
        }

        public IEnumerable<Song> GetSongsByArtistId(int artistId)
        {
            return _context.Songs
                .AsNoTracking()
                .AsSplitQuery()
                .Where(song => song.SongArtists.Any(songArtist => songArtist.ArtistId == artistId))
                .Include(song => song.Album)
                .Include(song => song.SongStat)
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .OrderByDescending(song => song.CreatedAt)
                .ThenBy(song => song.Title)
                .ToList();
        }
        public Song? GetArtistSongForEdit(int songId, int artistId)
        {
            return _context.Songs
                .Include(song => song.Album)
                .Include(song => song.SongArtists)
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)
                .FirstOrDefault(song =>
                    song.Id == songId &&
                    song.SongArtists.Any(songArtist => songArtist.ArtistId == artistId));
        }

        public bool ExistsByTitleAndArtist(string title, int artistId, int excludedSongId)
        {
            var normalizedTitle = title.Trim();

            return _context.Songs
                .AsNoTracking()
                .Any(song =>
                    song.Id != excludedSongId &&
                    song.Title == normalizedTitle &&
                    song.SongArtists.Any(songArtist => songArtist.ArtistId == artistId));
        }

        public void UpdateSongWithRelations(Song song, IEnumerable<int> genreIds)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var oldSongGenres = _context.SongGenres
                    .Where(songGenre => songGenre.SongId == song.Id)
                    .ToList();

                _context.SongGenres.RemoveRange(oldSongGenres);

                var distinctGenreIds = genreIds
                    .Where(genreId => genreId > 0)
                    .Distinct()
                    .ToList();

                foreach (var genreId in distinctGenreIds)
                {
                    var songGenre = new SongGenre
                    {
                        SongId = song.Id,
                        GenreId = genreId
                    };

                    _context.SongGenres.Add(songGenre);
                }

                _context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Song> GetSongsByAlbum(int albumId)
        {
            return _dbSet
                .AsNoTracking()
                .Where(song =>
                    song.AlbumId == albumId &&
                    !song.IsAdminHidden &&
                    song.PublicationStatus == PublicationStatus.Published &&
                    song.Album!.PublicationStatus == PublicationStatus.Published &&
                    !song.Album.IsAdminHidden)
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)
                .Include(song => song.SongStat)
                .OrderBy(song => song.Title)
                .ToList();
        }

        public List<Song> GetPopularSongs()
        {
            return _context.Songs
                .AsNoTracking()
                .Where(song =>
                    !song.IsAdminHidden &&
                    song.PublicationStatus == PublicationStatus.Published &&
                    (song.AlbumId == null ||
                     (song.Album!.PublicationStatus == PublicationStatus.Published &&
                      !song.Album.IsAdminHidden)))
                .Include(song => song.SongStat)
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .OrderByDescending(song => song.SongStat != null ? song.SongStat.PopularityScore : 0)
                .ThenBy(song => song.Title)
                .Take(5)
                .ToList();
        }

        public Song? GetSongDetailsById(int songId)
        {
            return _context.Songs
                .AsNoTracking()
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)
                .Include(song => song.SongStat)
                .FirstOrDefault(song =>
                    song.Id == songId &&
                    !song.IsAdminHidden &&
                    song.PublicationStatus == PublicationStatus.Published &&
                    (song.AlbumId == null ||
                     (song.Album!.PublicationStatus == PublicationStatus.Published &&
                      !song.Album.IsAdminHidden)));
        }

        public Song? GetSongForListening(int songId)
        {
            return _context.Songs
                .Include(song => song.SongStat)
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)
                .FirstOrDefault(song =>
                    song.Id == songId &&
                    !song.IsAdminHidden &&
                    song.PublicationStatus == PublicationStatus.Published &&
                    (song.AlbumId == null ||
                     (song.Album!.PublicationStatus == PublicationStatus.Published &&
                      !song.Album.IsAdminHidden)));
        }

        public bool ExistsByTitleAndArtist(string title, int artistId)
        {
            var normalizedTitle = title.Trim();

            return _context.Songs
                .AsNoTracking()
                .Any(song =>
                    song.Title == normalizedTitle &&
                    song.SongArtists.Any(songArtist => songArtist.ArtistId == artistId));
        }

        public void CreateSongWithRelations(Song song, int artistId, IEnumerable<int> genreIds)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.Songs.Add(song);
                _context.SaveChanges();

                var songArtist = new SongArtist
                {
                    SongId = song.Id,
                    ArtistId = artistId
                };

                _context.SongArtists.Add(songArtist);

                var distinctGenreIds = genreIds
                    .Where(genreId => genreId > 0)
                    .Distinct()
                    .ToList();

                foreach (var genreId in distinctGenreIds)
                {
                    var songGenre = new SongGenre
                    {
                        SongId = song.Id,
                        GenreId = genreId
                    };

                    _context.SongGenres.Add(songGenre);
                }

                var songStat = new SongStat
                {
                    SongId = song.Id,
                    TotalStreams = 0,
                    TotalLikes = 0,
                    PopularityScore = 0
                };

                _context.SongStats.Add(songStat);

                _context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void SoftDeleteSong(Song song)
        {
            song.IsDeleted = true;

            _context.Songs.Update(song);
            _context.SaveChanges();
        }

        public Song? GetSongById(int songId)
        {
            return _context.Songs
                .FirstOrDefault(song => song.Id == songId);
        }

        public bool UpdatePublication(Song song)
        {
            var existingSong = _context.Songs
                .FirstOrDefault(existingSong => existingSong.Id == song.Id);

            if (existingSong == null)
            {
                return false;
            }

            existingSong.PublicationStatus = song.PublicationStatus;
            existingSong.ScheduledPublishAtUtc = song.ScheduledPublishAtUtc;
            existingSong.PublishedAtUtc = song.PublishedAtUtc;
            existingSong.PublicationJobId = song.PublicationJobId;

            _context.SaveChanges();

            return true;
        }
    }
}
