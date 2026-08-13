using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.Services.Interface
{
    public interface IListeningHistoryService
    {
        bool AddListening(int userId, int songId);

        IEnumerable<ListeningHistoryDto> GetRecentListeningHistory(
            int userId,
            int count);

        int GetTotalListeningCount(int userId);
    }
}