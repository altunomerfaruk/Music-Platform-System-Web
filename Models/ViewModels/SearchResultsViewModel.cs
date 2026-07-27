using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class SearchResultsViewModel : UserLayoutViewModel
    {
        public string Query { get; set; } = string.Empty;

        public IEnumerable<Song> Songs { get; set; } = new List<Song>();

        public IEnumerable<Artist> Artists { get; set; } = new List<Artist>();

        public IEnumerable<Album> Albums { get; set; } = new List<Album>();

        public HashSet<int> LikedSongIds { get; set; } = new();

        public HashSet<int> FollowedArtistIds { get; set; } = new();
    }
}