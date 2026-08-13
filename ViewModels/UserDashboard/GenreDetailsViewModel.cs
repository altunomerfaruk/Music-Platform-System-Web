using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.ViewModels.UserDashboard
{
    public class GenreDetailsViewModel : UserLayoutViewModel
    {
        public GenreDetailsDto Genre { get; set; } = null!;

        public HashSet<int> LikedSongIds { get; set; } = new();
    }
}
