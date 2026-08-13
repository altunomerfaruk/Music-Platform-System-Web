using MusicProject.Contracts.Responses.ArtistDashboard;

namespace MusicProject.ViewModels.ArtistDashboard
{
    public class ArtistAlbumsViewModel : ArtistLayoutViewModel
    {
        public IEnumerable<ArtistAlbumListItemDto> Albums { get; set; }
            = new List<ArtistAlbumListItemDto>();

        public int TotalAlbumStreams { get; set; }

        public int TotalAlbumSongs { get; set; }
    }
}
