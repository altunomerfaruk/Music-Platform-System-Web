using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class FollowedArtistService
        : IFollowedArtistService
    {
        private readonly IFollowedArtistRepository
            _followedArtistRepository;

        public FollowedArtistService(
            IFollowedArtistRepository
                followedArtistRepository)
        {
            _followedArtistRepository =
                followedArtistRepository;
        }
        public bool ToggleFollow(
            int userId,
            int artistId)
        {
            var existingFollow =
                _followedArtistRepository
                    .GetByUserAndArtist(
                        userId,
                        artistId
                    );

            if (existingFollow == null)
            {
                var newFollow =
                    new FollowedArtist
                    {
                        UserId = userId,
                        ArtistId = artistId,
                        FollowedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                _followedArtistRepository.Create(
                    newFollow
                );

                return true;
            }

            existingFollow.IsActive =
                !existingFollow.IsActive;

            // DEĞİŞİKLİK:
            // Kullanıcı yeniden takip ettiyse
            // takip tarihini güncelliyoruz.
            if (existingFollow.IsActive)
            {
                existingFollow.FollowedAt =
                    DateTime.UtcNow;
            }

            _followedArtistRepository.Update(
                existingFollow
            );

            return existingFollow.IsActive;
        }

        public IEnumerable<int>
            GetActiveFollowedArtistIds(
                int userId)
        {
            return _followedArtistRepository
                .GetActiveFollowedArtistIdsByUser(
                    userId
                );
        }
    }
}