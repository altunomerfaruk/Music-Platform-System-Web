using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.ViewModels.UserDashboard
{
    public class SearchResultsViewModel : UserLayoutViewModel
    {
        public string Query { get; set; } = string.Empty;

        public IEnumerable<SongListItemDto> Songs { get; set; }
            = new List<SongListItemDto>();

        public IEnumerable<ArtistListItemDto> Artists { get; set; }
            = new List<ArtistListItemDto>();

        public IEnumerable<AlbumListItemDto> Albums { get; set; }
            = new List<AlbumListItemDto>();

        public HashSet<int> LikedSongIds { get; set; } = new();

        public HashSet<int> FollowedArtistIds { get; set; } = new();
    }
}
