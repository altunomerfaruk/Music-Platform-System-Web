using Microsoft.EntityFrameworkCore;
using MusicProject.Data;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class ListeningHistoryRepository : IListeningHistoryRepository
    {
        private readonly AppDbContext _context;

        public ListeningHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public void AddListeningHistory(ListeningHistory listeningHistory)
        {
            _context.ListeningHistories.Add(listeningHistory);
        }

        public IEnumerable<ListeningHistory> GetRecentListeningHistoryByUser(int userId, int count)
        {
            return _context.ListeningHistories
                .AsNoTracking()
                .Where(history =>
                    history.UserId == userId &&
                    history.Song.PublicationStatus == PublicationStatus.Published &&
                    (history.Song.AlbumId == null ||
                     history.Song.Album!.PublicationStatus == PublicationStatus.Published))
                .Include(history => history.Song)
                    .ThenInclude(song => song.Album)
                        .ThenInclude(album => album!.Artist)
                .Include(history => history.Song)
                    .ThenInclude(song => song.SongArtists)
                        .ThenInclude(songArtist => songArtist.Artist)
                .Include(history => history.Song)
                    .ThenInclude(song => song.SongGenres)
                        .ThenInclude(songGenre => songGenre.Genre)
                .OrderByDescending(history => history.ListenedAt)
                .Take(count)
                .ToList();
        }

        public int GetTotalListeningCountByUser(int userId)
        {
            return _context.ListeningHistories
                .Count(history => history.UserId == userId);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}