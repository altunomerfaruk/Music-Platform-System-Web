using MusicProject.DTOs;

namespace MusicProject.Models.ViewModels
{
    public class ListeningHistoryViewModel : UserLayoutViewModel
    {
        public IEnumerable<ListeningHistoryDto> ListeningHistory { get; set; }
            = new List<ListeningHistoryDto>();

        public int TotalListeningCount { get; set; }
    }
}