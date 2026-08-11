using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IAdminDashboardRepository
    {
        int GetTotalUserCount();

        int GetTotalArtistCount();

        int GetTotalSongCount();

        int GetTotalListeningCount();

        IEnumerable<User> GetRecentUsers(int count);

        IEnumerable<Song> GetTopSongsByStreams(int count);

        IEnumerable<ListeningHistory> GetListeningHistorySince(DateTime startUtc);

        IEnumerable<User> GetUsers(string? search);

        User? GetUserById(int userId);

        void UpdateUser(User user);
    }
}