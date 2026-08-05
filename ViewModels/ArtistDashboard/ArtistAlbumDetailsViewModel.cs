using MusicProject.Models.Concrete;

namespace MusicProject.ViewModels.ArtistDashboard
{
    public class ArtistAlbumDetailsViewModel : ArtistLayoutViewModel
    {
        public Album Album { get; set; } = null!;

        public int SongCount { get; set; }

        public long TotalStreams { get; set; }

        public int TotalLikes { get; set; }
    }
}