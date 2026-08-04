using MusicProject.Models.Concrete;

namespace MusicProject.ViewModels.UserDashboard
{
    public class GenreDetailsViewModel : UserLayoutViewModel
    {
        public Genre Genre { get; set; } = null!;

        public HashSet<int> LikedSongIds { get; set; } = new();
    }
}