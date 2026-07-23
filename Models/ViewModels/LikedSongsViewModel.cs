using MusicProject.DTOs;

namespace MusicProject.Models.ViewModels
{
    public class LikedSongsViewModel : UserLayoutViewModel
    {
        public IEnumerable<LikedSongDto> Songs { get; set; } = new List<LikedSongDto>();
    }
}
