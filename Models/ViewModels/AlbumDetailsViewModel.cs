using MusicProject.DTOs;

namespace MusicProject.Models.ViewModels
{
    public class AlbumDetailsViewModel : UserLayoutViewModel
    {
        public AlbumDetailsDto Album { get; set; } = new();

        public HashSet<int> LikedSongIds { get; set; } = new();
    }
}