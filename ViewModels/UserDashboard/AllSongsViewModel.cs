using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.ViewModels.UserDashboard
{
    public class AllSongsViewModel : UserLayoutViewModel
    {
        public IEnumerable<SongListItemDto> Songs { get; set; }
            = new List<SongListItemDto>();

        public HashSet<int> LikedSongIds { get; set; } = new();

        public string Search { get; set; } = string.Empty;

        public int? ArtistId { get; set; }

        public int? AlbumId { get; set; }

        public int? GenreId { get; set; }

        public string Sort { get; set; } = "name-asc";

        public bool LikedOnly { get; set; }

        public IEnumerable<FilterOptionDto> Artists { get; set; }
            = new List<FilterOptionDto>();

        public IEnumerable<FilterOptionDto> Albums { get; set; }
            = new List<FilterOptionDto>();

        public IEnumerable<FilterOptionDto> Genres { get; set; }
            = new List<FilterOptionDto>();

        public int TotalSongCount { get; set; }
    }
}
