using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class ArtistLayoutViewModel
    {
        public Artist Artist { get; set; } = null!;

        public int TotalAlbums { get; set; }

        public int TotalSongs { get; set; }

        public string ArtistInitial { get; set; } = "?";
    }
}