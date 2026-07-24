using MusicProject.DTOs;

namespace MusicProject.Models.ViewModels
{
    public class SongDetailsViewModel : UserLayoutViewModel
    {
        public SongDetailsDto Song { get; set; } = new();

        public bool IsLiked { get; set; }
    }
}