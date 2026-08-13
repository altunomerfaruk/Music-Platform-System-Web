using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.ViewModels.UserDashboard
{
    public class SongDetailsViewModel : UserLayoutViewModel
    {
        public SongDetailsDto Song { get; set; } = new();

        public bool IsLiked { get; set; }
    }
}