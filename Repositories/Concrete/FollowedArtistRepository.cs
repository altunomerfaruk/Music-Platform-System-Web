using Microsoft.EntityFrameworkCore;
using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class FollowedArtistRepository : IFollowedArtistRepository
    {
        private readonly AppDbContext _context;

        public FollowedArtistRepository(AppDbContext context)
        {
            _context = context;
        }

        public FollowedArtist? GetByUserAndArtist(int userId, int artistId)
        {
            return _context.FollowedArtists.FirstOrDefault(followedArtist =>
                followedArtist.UserId == userId &&
                followedArtist.ArtistId == artistId
            );
        }

        public IEnumerable<int> GetActiveFollowedArtistIdsByUser(int userId)
        {
            return _context.FollowedArtists
                .Where(followedArtist =>
                    followedArtist.UserId == userId &&
                    followedArtist.IsActive
                )
                .Select(followedArtist => followedArtist.ArtistId)
                .ToList();
        }

        // DEĞİŞİKLİK:
        // Kullanıcının aktif takip ettiği sanatçı kayıtlarını
        // Artist bilgisiyle birlikte getirir.
        public IEnumerable<FollowedArtist> GetActiveFollowedArtistsByUser(int userId)
        {
            return _context.FollowedArtists
                .Where(followedArtist =>
                    followedArtist.UserId == userId &&
                    followedArtist.IsActive
                )
                .Include(followedArtist => followedArtist.Artist)
                .OrderByDescending(followedArtist => followedArtist.FollowedAt)
                .ToList();
        }

        // DEĞİŞİKLİK:
        // Sanatçının aktif takipçi sayısını getirir.
        public int GetActiveFollowerCountByArtist(int artistId)
        {
            return _context.FollowedArtists.Count(followedArtist =>
                followedArtist.ArtistId == artistId &&
                followedArtist.IsActive
            );
        }

        public void Create(FollowedArtist followedArtist)
        {
            _context.FollowedArtists.Add(followedArtist);
            _context.SaveChanges();
        }

        public void Update(FollowedArtist followedArtist)
        {
            _context.FollowedArtists.Update(followedArtist);
            _context.SaveChanges();
        }
    }
}