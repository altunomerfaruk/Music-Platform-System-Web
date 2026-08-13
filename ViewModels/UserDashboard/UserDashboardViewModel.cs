using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.ViewModels.UserDashboard
{
    public class UserDashboardViewModel : UserLayoutViewModel
    {
        public IEnumerable<SongListItemDto> PopularSongs { get; set; }
            = new List<SongListItemDto>();

        public IEnumerable<ArtistListItemDto> Artists { get; set; }
            = new List<ArtistListItemDto>();

        public HashSet<int> LikedSongIds { get; set; } = new();

        public HashSet<int> FollowedArtistIds { get; set; } = new();
        public int TotalListeningCount { get; set; }
        public IEnumerable<ListeningHistoryDto> RecentListeningHistory { get; set; }
            = new List<ListeningHistoryDto>();
    }
}
