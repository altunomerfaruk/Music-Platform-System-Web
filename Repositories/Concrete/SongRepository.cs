using Microsoft.EntityFrameworkCore;
using MusicProject.Data;
using MusicProject.Models.Concrete;
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

        public IEnumerable<Song> GetSongsByArtistId(int artistId)
        {
            return _context.Songs
                .AsNoTracking()
                .AsSplitQuery()
                .Where(song =>
                    song.SongArtists.Any(songArtist =>
                        songArtist.ArtistId == artistId))
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
                .Include(song => song.SongArtists)
                .Include(song => song.SongGenres)
                    .ThenInclude(songGenre => songGenre.Genre)
                .FirstOrDefault(song =>
                    song.Id == songId &&
                    song.SongArtists.Any(songArtist =>
                        songArtist.ArtistId == artistId));
        }

        public bool ExistsByTitleAndArtist(string title, int artistId, int excludedSongId)
        {
            var normalizedTitle = title.Trim();
            return _context.Songs
                .AsNoTracking()
                .Any(song =>
                    song.Id != excludedSongId &&
                    song.Title == normalizedTitle &&
                    song.SongArtists.Any(songArtist =>
                        songArtist.ArtistId == artistId));
        }


        public void UpdateSongWithRelations(Song song, IEnumerable<int> genreIds)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var oldSongGenres = _context.SongGenres
                    .Where(sg => sg.SongId == song.Id)
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
                .Where(song => song.AlbumId == albumId)
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
                .Include(song => song.SongStat)
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .OrderByDescending(song =>
                    song.SongStat != null
                        ? song.SongStat.PopularityScore
                        : 0)
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
                .FirstOrDefault(song => song.Id == songId);
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
                .FirstOrDefault(song => song.Id == songId);
        }

        public bool ExistsByTitleAndArtist(string title, int artistId)
        {
            var normalizedTitle = title.Trim();

            return _context.Songs
                .AsNoTracking()
                .Any(song =>
                    song.Title == normalizedTitle &&
                    song.SongArtists.Any(songArtist =>
                        songArtist.ArtistId == artistId));
        }

        public void CreateSongWithRelations(Song song, int artistId, IEnumerable<int> genreIds)
        {
            using var transaction =
                _context.Database.BeginTransaction();

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
    }
}