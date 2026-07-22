using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface ILikedSongRepository
    {
        LikedSong? GetByUserAndSong(
            int userId,
            int songId
        );

        IEnumerable<int> GetActiveLikedSongIdsByUser(
            int userId
        );

        int GetActiveLikeCountBySong(
            int songId
        );

        void Create(LikedSong likedSong);

        void Update(LikedSong likedSong);
    }
}