using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class AllArtistsViewModel : UserLayoutViewModel
    {
        public IEnumerable<Artist> Artists { get; set; } = new List<Artist>();

        public HashSet<int> FollowedArtistIds { get; set; } = new();

        public string Search { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string Sort { get; set; } = "name-asc";

        public bool FollowedOnly { get; set; }

        public IEnumerable<string> Countries { get; set; } = new List<string>();

        public int TotalArtistCount { get; set; }
    }
}