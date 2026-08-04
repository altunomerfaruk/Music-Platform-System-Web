using MusicProject.Contracts.Responses;

namespace MusicProject.ViewModels.UserDashboard
{
    public class FollowedArtistsViewModel : UserLayoutViewModel
    {
        public IEnumerable<FollowedArtistDto> Artists { get; set; }
            = new List<FollowedArtistDto>();
    }
}