using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IListeningHistoryRepository
    {
        void AddListeningHistory(ListeningHistory listeningHistory);

        IEnumerable<ListeningHistory> GetRecentListeningHistoryByUser(
            int userId,
            int count);

        int GetTotalListeningCountByUser(int userId);

        void SaveChanges();
    }
}