using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IFollowedArtistRepository
    {
        FollowedArtist? GetByUserAndArtist(
            int userId,
            int artistId
        );

        IEnumerable<int> GetActiveFollowedArtistIdsByUser(
            int userId
        );

        void Create(FollowedArtist followedArtist);

        void Update(FollowedArtist followedArtist);
    }
}