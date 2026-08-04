using MusicProject.Contracts.Responses;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class ListeningHistoryManager : IListeningHistoryService
    {
        private readonly IListeningHistoryRepository _listeningHistoryRepository;
        private readonly ISongRepository _songRepository;

        public ListeningHistoryManager(IListeningHistoryRepository listeningHistoryRepository, ISongRepository songRepository)
        {
            _listeningHistoryRepository = listeningHistoryRepository;
            _songRepository = songRepository;
        }

        public bool AddListening(int userId, int songId)
        {
            var song = _songRepository.GetSongForListening(songId);

            if (song == null)
            {
                return false;
            }

            var listeningHistory = new ListeningHistory
            {
                UserId = userId,
                SongId = songId,
                ListenedAt = DateTime.UtcNow
            };

            _listeningHistoryRepository.AddListeningHistory(listeningHistory);

            if (song.SongStat == null)
            {
                song.SongStat = new SongStat
                {
                    SongId = song.Id,
                    TotalStreams = 1,
                    TotalLikes = 0,
                    PopularityScore = 1
                };
            }
            else
            {
                song.SongStat.TotalStreams++;

                song.SongStat.PopularityScore =
                    song.SongStat.TotalStreams +
                    (song.SongStat.TotalLikes * 3);
            }

            _listeningHistoryRepository.SaveChanges();

            return true;
        }

        public IEnumerable<ListeningHistoryDto> GetRecentListeningHistory(int userId, int count)
        {
            return _listeningHistoryRepository
                .GetRecentListeningHistoryByUser(userId, count)
                .Select(history =>
                {
                    var artist = history.Song.Album?.Artist ??
                                 history.Song.SongArtists
                                     .Select(songArtist => songArtist.Artist)
                                     .FirstOrDefault();

                    var genreName = history.Song.SongGenres
                        .Select(songGenre => songGenre.Genre.Name)
                        .FirstOrDefault() ?? "Tür bilgisi yok";

                    return new ListeningHistoryDto
                    {
                        ListeningHistoryId = history.Id,
                        SongId = history.SongId,
                        SongTitle = history.Song.Title,
                        AlbumId = history.Song.AlbumId,
                        AlbumName = history.Song.Album?.Name ?? "Single",
                        ArtistId = artist?.Id,
                        ArtistName = artist?.Name ?? "Sanatçı bilgisi yok",
                        GenreName = genreName,
                        ListenedAt = history.ListenedAt
                    };
                })
                .ToList();
        }

        public int GetTotalListeningCount(int userId)
        {
            return _listeningHistoryRepository
                .GetTotalListeningCountByUser(userId);
        }
    }
}