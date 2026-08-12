using Microsoft.EntityFrameworkCore;
using MusicProject.Data;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly AppDbContext _context;

        public AdminDashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public int GetTotalUserCount()
        {
            return _context.users.Count();
        }

        public int GetTotalArtistCount()
        {
            return _context.Artists.Count();
        }

        public int GetTotalSongCount()
        {
            return _context.Songs.Count();
        }

        public int GetTotalListeningCount()
        {
            return _context.ListeningHistories.Count();
        }

        public IEnumerable<User> GetRecentUsers(int count)
        {
            return _context.users
                .AsNoTracking()
                .OrderByDescending(user => user.CreatedAt)
                .Take(count)
                .ToList();
        }

        public IEnumerable<Song> GetTopSongsByStreams(int count)
        {
            return _context.Songs
                .AsNoTracking()
                .Include(song => song.SongStat)
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .OrderByDescending(song => song.SongStat != null ? song.SongStat.TotalStreams : 0)
                .ThenBy(song => song.Title)
                .Take(count)
                .ToList();
        }

        public IEnumerable<ListeningHistory> GetListeningHistorySince(DateTime startUtc)
        {
            return _context.ListeningHistories
                .AsNoTracking()
                .Where(history => history.ListenedAt >= startUtc)
                .ToList();
        }

        public IEnumerable<User> GetUsers(string? search)
        {
            var query = _context.users
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();

                query = query.Where(user =>
                    user.Username.Contains(normalizedSearch) ||
                    user.Email.Contains(normalizedSearch));
            }

            return query
                .OrderByDescending(user => user.CreatedAt)
                .ThenBy(user => user.Username)
                .ToList();
        }

        public User? GetUserById(int userId)
        {
            return _context.users
                .FirstOrDefault(user => user.Id == userId);
        }

        public void UpdateUser(User user)
        {
            _context.SaveChanges();
        }

        public IEnumerable<Artist> GetArtists(string? search)
        {
            var query = _context.Artists
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();

                query = query.Where(artist =>
                    artist.Name.Contains(normalizedSearch) ||
                    (artist.User != null &&
                     (artist.User.Username.Contains(normalizedSearch) ||
                      artist.User.Email.Contains(normalizedSearch))) ||
                    (artist.CountryEntity != null &&
                     artist.CountryEntity.Name.Contains(normalizedSearch)));
            }

            return query
                .Include(artist => artist.User)
                .Include(artist => artist.CountryEntity)
                .Include(artist => artist.Albums.Where(album => !album.IsDeleted))
                .Include(artist => artist.SongArtists.Where(songArtist => !songArtist.Song.IsDeleted))
                .Include(artist => artist.Followers.Where(follower => follower.IsActive))
                .OrderBy(artist => artist.Name)
                .ToList();
        }

        public IEnumerable<Song> GetSongs(string? search, PublicationStatus? status)
        {
            var query = _context.Songs
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();

                query = query.Where(song =>
                    song.Title.Contains(normalizedSearch) ||
                    (song.Album != null &&
                     (song.Album.Name.Contains(normalizedSearch) ||
                      song.Album.Artist.Name.Contains(normalizedSearch))) ||
                    song.SongArtists.Any(songArtist =>
                        songArtist.Artist.Name.Contains(normalizedSearch)) ||
                    (song.Label != null &&
                     song.Label.Name.Contains(normalizedSearch)));
            }

            if (status.HasValue)
            {
                query = query.Where(song => song.PublicationStatus == status.Value);
            }

            return query
                .Include(song => song.Album)
                    .ThenInclude(album => album!.Artist)
                .Include(song => song.Label)
                .Include(song => song.SongArtists)
                    .ThenInclude(songArtist => songArtist.Artist)
                .Include(song => song.SongStat)
                .OrderByDescending(song => song.CreatedAt)
                .ThenBy(song => song.Title)
                .ToList();
        }
    }
}