using MusicProject.Contracts.Responses;

namespace MusicProject.ViewModels.UserDashboard
{
    public class ArtistDetailsViewModel : UserLayoutViewModel
    {
        public ArtistDetailsDto Artist { get; set; } = new();

        public bool IsFollowed { get; set; }

        public HashSet<int> LikedSongIds { get; set; } = new();
    }
}