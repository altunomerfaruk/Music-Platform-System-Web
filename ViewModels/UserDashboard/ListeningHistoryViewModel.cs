using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.ViewModels.UserDashboard
{
    public class ListeningHistoryViewModel : UserLayoutViewModel
    {
        public IEnumerable<ListeningHistoryDto> ListeningHistory { get; set; }
            = new List<ListeningHistoryDto>();

        public int TotalListeningCount { get; set; }
    }
}