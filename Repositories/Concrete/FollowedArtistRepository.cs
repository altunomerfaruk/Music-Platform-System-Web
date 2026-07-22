using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class FollowedArtistRepository
        : IFollowedArtistRepository
    {
        private readonly AppDbContext _context;

        public FollowedArtistRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public FollowedArtist? GetByUserAndArtist(
            int userId,
            int artistId)
        {
            return _context.FollowedArtists
                .FirstOrDefault(followedArtist =>
                    followedArtist.UserId == userId &&
                    followedArtist.ArtistId == artistId
                );
        }

        public IEnumerable<int>
            GetActiveFollowedArtistIdsByUser(
                int userId)
        {
            return _context.FollowedArtists
                .Where(followedArtist =>
                    followedArtist.UserId == userId &&
                    followedArtist.IsActive
                )
                .Select(followedArtist =>
                    followedArtist.ArtistId
                )
                .ToList();
        }

        public void Create(
            FollowedArtist followedArtist)
        {
            _context.FollowedArtists.Add(
                followedArtist
            );

            _context.SaveChanges();
        }

        public void Update(
            FollowedArtist followedArtist)
        {
            _context.FollowedArtists.Update(
                followedArtist
            );

            _context.SaveChanges();
        }
    }
}