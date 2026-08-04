using MusicProject.Contracts.Responses;

namespace MusicProject.Services.Interface
{
    public interface IFollowedArtistService
    {
        bool ToggleFollow(int userId, int artistId);

        IEnumerable<int> GetActiveFollowedArtistIds(int userId);

        IEnumerable<FollowedArtistDto> GetFollowedArtistsByUser(int userId);
    }
}