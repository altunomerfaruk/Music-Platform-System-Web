using MusicProject.Contracts.Responses;

namespace MusicProject.ViewModels.UserDashboard
{
    public class AlbumDetailsViewModel : UserLayoutViewModel
    {
        public AlbumDetailsDto Album { get; set; } = new();

        public HashSet<int> LikedSongIds { get; set; } = new();
    }
}