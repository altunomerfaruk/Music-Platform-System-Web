using MusicProject.Contracts.Responses;

namespace MusicProject.ViewModels.UserDashboard
{
    public class LikedSongsViewModel : UserLayoutViewModel
    {
        public IEnumerable<LikedSongDto> Songs { get; set; } = new List<LikedSongDto>();
    }
}
