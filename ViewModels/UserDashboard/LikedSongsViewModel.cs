using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.ViewModels.UserDashboard
{
    public class LikedSongsViewModel : UserLayoutViewModel
    {
        public IEnumerable<LikedSongDto> Songs { get; set; } = new List<LikedSongDto>();
    }
}
