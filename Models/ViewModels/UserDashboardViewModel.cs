using MusicProject.DTOs;
using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class UserDashboardViewModel : UserLayoutViewModel
    {
        public IEnumerable<Song> PopularSongs { get; set; } = new List<Song>();

        public IEnumerable<Artist> Artists { get; set; } = new List<Artist>();

        public HashSet<int> LikedSongIds { get; set; } = new();

        public HashSet<int> FollowedArtistIds { get; set; } = new();
        public int TotalListeningCount { get; set; }
        public IEnumerable<ListeningHistoryDto> RecentListeningHistory { get; set; }
            = new List<ListeningHistoryDto>();
    }
}