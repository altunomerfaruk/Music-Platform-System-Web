using MusicProject.DTOs;

namespace MusicProject.Services.Interface
{
    public interface ILikedSongService
    {
        bool ToggleLike(
            int userId,
            int songId
        );

        IEnumerable<int> GetActiveLikedSongIds(
            int userId
        );
        IEnumerable<LikedSongDto> GetLikedSongsByUser(
            int userId
        );
    }
}