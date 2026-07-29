using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class AllSongsViewModel : UserLayoutViewModel
    {
        public IEnumerable<Song> Songs { get; set; } = new List<Song>();

        public HashSet<int> LikedSongIds { get; set; } = new();

        public string Search { get; set; } = string.Empty;

        public int? ArtistId { get; set; }

        public int? AlbumId { get; set; }

        public int? GenreId { get; set; }

        public string Sort { get; set; } = "name-asc";

        public bool LikedOnly { get; set; }

        public IEnumerable<Artist> Artists { get; set; } = new List<Artist>();
        public IEnumerable<Album> Albums { get; set; } = new List<Album>();
        public IEnumerable<Genre> Genres { get; set; } = new List<Genre>();
        public int TotalSongCount { get; set; }
    }
}