using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class ArtistAlbumsViewModel : ArtistLayoutViewModel
    {
        public IEnumerable<Album> Albums { get; set; }
            = new List<Album>();

        public int TotalAlbumStreams { get; set; }

        public int TotalAlbumSongs { get; set; }
    }
}