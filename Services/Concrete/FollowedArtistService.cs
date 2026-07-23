using MusicProject.DTOs;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class FollowedArtistService : IFollowedArtistService
    {
        private readonly IFollowedArtistRepository _followedArtistRepository;

        public FollowedArtistService(IFollowedArtistRepository followedArtistRepository)
        {
            _followedArtistRepository = followedArtistRepository;
        }

        public bool ToggleFollow(int userId, int artistId)
        {
            var existingFollow = _followedArtistRepository
                .GetByUserAndArtist(userId, artistId);

            if (existingFollow == null)
            {
                var followedArtist = new FollowedArtist
                {
                    UserId = userId,
                    ArtistId = artistId,
                    FollowedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _followedArtistRepository.Create(followedArtist);

                return true;
            }

            existingFollow.IsActive = !existingFollow.IsActive;

            if (existingFollow.IsActive)
            {
                existingFollow.FollowedAt = DateTime.UtcNow;
            }

            _followedArtistRepository.Update(existingFollow);

            return existingFollow.IsActive;
        }

        public IEnumerable<int> GetActiveFollowedArtistIds(int userId)
        {
            return _followedArtistRepository
                .GetActiveFollowedArtistIdsByUser(userId);
        }

        public IEnumerable<FollowedArtistDto> GetFollowedArtistsByUser(int userId)
        {
            var followedArtists = _followedArtistRepository
                .GetActiveFollowedArtistsByUser(userId);

            return followedArtists.Select(followedArtist => new FollowedArtistDto
            {
                ArtistId = followedArtist.ArtistId,
                Name = followedArtist.Artist.Name,
                Country = followedArtist.Artist.Country ?? "Ülke bilgisi yok",
                DebutYear = followedArtist.Artist.DebutYear,
                FollowedAt = followedArtist.FollowedAt,

                TotalFollowers = _followedArtistRepository
                    .GetActiveFollowerCountByArtist(followedArtist.ArtistId)
            }).ToList();
        }
    }
}