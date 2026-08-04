using MusicProject.Contracts.Responses;

namespace MusicProject.ViewModels.UserDashboard
{
    public class SongDetailsViewModel : UserLayoutViewModel
    {
        public SongDetailsDto Song { get; set; } = new();

        public bool IsLiked { get; set; }
    }
}