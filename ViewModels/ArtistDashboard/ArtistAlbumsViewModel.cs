using MusicProject.Models.Concrete;

namespace MusicProject.ViewModels.ArtistDashboard
{
    public class ArtistAlbumsViewModel : ArtistLayoutViewModel
    {
        public IEnumerable<Album> Albums { get; set; }
            = new List<Album>();

        public int TotalAlbumStreams { get; set; }

        public int TotalAlbumSongs { get; set; }
    }
}