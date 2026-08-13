using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.ViewModels.UserDashboard
{
    public class AlbumDetailsViewModel : UserLayoutViewModel
    {
        public AlbumDetailsDto Album { get; set; } = new();

        public HashSet<int> LikedSongIds { get; set; } = new();
    }
}