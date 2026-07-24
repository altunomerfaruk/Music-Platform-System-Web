using MusicProject.DTOs;

namespace MusicProject.Models.ViewModels
{
    public class ArtistDetailsViewModel : UserLayoutViewModel
    {
        public ArtistDetailsDto Artist { get; set; } = new();

        public bool IsFollowed { get; set; }

        public HashSet<int> LikedSongIds { get; set; } = new();
    }
}