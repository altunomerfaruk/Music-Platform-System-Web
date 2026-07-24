using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class AllSongsViewModel : UserLayoutViewModel
    {
        public IEnumerable<Song> Songs { get; set; } = new List<Song>();

        public HashSet<int> LikedSongIds { get; set; } = new();
    }
}