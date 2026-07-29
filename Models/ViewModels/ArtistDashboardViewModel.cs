using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class ArtistDashboardViewModel : ArtistLayoutViewModel
    {
        public IEnumerable<Song> PopularSongs { get; set; } = new List<Song>();

        public IEnumerable<Album> RecentAlbums { get; set; } = new List<Album>();

        public int TotalStreams { get; set; }

        public int TotalLikes { get; set; }

        public int MonthlyListeners { get; set; }

        public int TotalFollowers { get; set; }
    }
}