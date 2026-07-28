using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class GenreDetailsViewModel : UserLayoutViewModel
    {
        public Genre Genre { get; set; } = null!;

        public HashSet<int> LikedSongIds { get; set; } = new();
    }
}