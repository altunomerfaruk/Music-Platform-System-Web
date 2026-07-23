using Microsoft.EntityFrameworkCore;
using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class LikedSongRepository : ILikedSongRepository
    {
        private readonly AppDbContext _context;

        public LikedSongRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public LikedSong? GetByUserAndSong(
            int userId,
            int songId)
        {
            return _context.LikedSongs
                .FirstOrDefault(likedSong =>
                    likedSong.UserId == userId &&
                    likedSong.SongId == songId
                );
        }

        public IEnumerable<int> GetActiveLikedSongIdsByUser(
            int userId)
        {
            return _context.LikedSongs
                .Where(likedSong =>
                    likedSong.UserId == userId &&
                    likedSong.IsActive
                )
                .Select(likedSong =>
                    likedSong.SongId
                )
                .ToList();
        }

        public int GetActiveLikeCountBySong(
            int songId)
        {
            return _context.LikedSongs
                .Count(likedSong =>
                    likedSong.SongId == songId &&
                    likedSong.IsActive
                );
        }

        public IEnumerable<LikedSong> GetActiveLikedSongsByUser(
            int userId)
        {
            return _context.LikedSongs
                .Where(likedSong =>
                    likedSong.UserId == userId &&
                    likedSong.IsActive
                )

                .Include(likedSong =>
                    likedSong.Song
                )
                .ThenInclude(song =>
                    song.Album
                )


                .Include(likedSong =>
                    likedSong.Song
                )
                .ThenInclude(song =>
                    song.SongStat
                )

                .Include(likedSong =>
                    likedSong.Song
                )
                .ThenInclude(song =>
                    song.SongArtists
                )
                .ThenInclude(songArtist =>
                    songArtist.Artist
                )

                .OrderByDescending(likedSong =>
                    likedSong.LikedAt
                )
                .ToList();
        }

        public void Create(
            LikedSong likedSong)
        {
            _context.LikedSongs.Add(
                likedSong
            );

            _context.SaveChanges();
        }

        public void Update(
            LikedSong likedSong)
        {
            _context.LikedSongs.Update(
                likedSong
            );

            _context.SaveChanges();
        }
    }
}