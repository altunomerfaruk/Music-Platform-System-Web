using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class AllArtistsViewModel : UserLayoutViewModel
    {
        public IEnumerable<Artist> Artists { get; set; } = new List<Artist>();

        public HashSet<int> FollowedArtistIds { get; set; } = new();
    }
}