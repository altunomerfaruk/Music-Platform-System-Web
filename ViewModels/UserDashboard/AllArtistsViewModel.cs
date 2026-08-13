using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.ViewModels.UserDashboard
{
    public class AllArtistsViewModel : UserLayoutViewModel
    {
        public IEnumerable<ArtistListItemDto> Artists { get; set; }
            = new List<ArtistListItemDto>();

        public HashSet<int> FollowedArtistIds { get; set; } = new();

        public string Search { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string Sort { get; set; } = "name-asc";

        public bool FollowedOnly { get; set; }

        public IEnumerable<string> Countries { get; set; } = new List<string>();

        public int TotalArtistCount { get; set; }
    }
}
