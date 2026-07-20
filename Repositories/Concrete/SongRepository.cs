using Microsoft.EntityFrameworkCore;
using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class SongRepository : GenericRepository<Song>, ISongRepository
    {
        public SongRepository(AppDbContext context)
            : base(context)
        {
        }

        public IEnumerable<Song> GetSongsSortedByAlphabet()
        {
            return _dbSet
                .OrderBy(song => song.Title)
                .ThenByDescending(song => song.AlbumId)
                .ToList();
        }

        public List<Song> GetSongsByAlbum(int albumId)
        {
            return _dbSet
                .Where(song => song.AlbumId == albumId)
                .ToList();
        }
        public List<Song> GetPopularSongs()
        {
            return _context.Songs
                .Include(song => song.SongStat)
                .Include(song => song.Album)
                .OrderByDescending(song =>
                    song.SongStat != null
                        ? song.SongStat.PopularityScore
                        : 0
                )

                .Take(5)
                .ToList();
        }
    }
}