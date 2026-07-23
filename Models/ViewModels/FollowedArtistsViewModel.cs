using MusicProject.DTOs;

namespace MusicProject.Models.ViewModels
{
    public class FollowedArtistsViewModel : UserLayoutViewModel
    {
        public IEnumerable<FollowedArtistDto> Artists { get; set; }
            = new List<FollowedArtistDto>();
    }
}