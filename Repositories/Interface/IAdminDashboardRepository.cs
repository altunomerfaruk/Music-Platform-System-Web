using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;

namespace MusicProject.Repositories.Interface
{
    public interface IAdminDashboardRepository
    {
        int GetTotalUserCount();

        int GetTotalArtistCount();

        int GetTotalAlbumCount();

        int GetTotalSongCount();

        int GetTotalListeningCount();

        IEnumerable<User> GetRecentUsers(int count);

        IEnumerable<Song> GetTopSongsByStreams(int count);

        IEnumerable<ListeningHistory> GetListeningHistorySince(DateTime startUtc);

        IEnumerable<User> GetUsers(string? search);

        User? GetUserById(int userId);

        void UpdateUser(User user);

        IEnumerable<Artist> GetArtists(string? search);

        IEnumerable<Album> GetAlbums(string? search, PublicationStatus? status);

        IEnumerable<Song> GetSongs(string? search, PublicationStatus? status);
    }
}