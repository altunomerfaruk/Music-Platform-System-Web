using MusicProject.Contracts.Responses.ArtistDashboard;

namespace MusicProject.ViewModels.ArtistDashboard
{
    public class ArtistDashboardViewModel : ArtistLayoutViewModel
    {
        public IEnumerable<ArtistSongListItemDto> PopularSongs { get; set; }
            = new List<ArtistSongListItemDto>();

        public IEnumerable<ArtistAlbumListItemDto> RecentAlbums { get; set; }
            = new List<ArtistAlbumListItemDto>();

        public int TotalStreams { get; set; }

        public int TotalLikes { get; set; }

        public int MonthlyListeners { get; set; }

        public int TotalFollowers { get; set; }
    }
}
