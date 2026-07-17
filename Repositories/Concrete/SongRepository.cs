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
                .OrderBy(song => song.AlbumId)
                .ThenBy(song => song.Title)
                .ToList();
        }

        public List<Song> GetSongsByAlbum(int albumId)
        {
            return _dbSet
                .Where(song => song.AlbumId == albumId)
                .ToList();
        }
    }
}