using MusicProject.data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class SongRepository : GenericRepository<Song>, ISongRepository
    {
        public SongRepository(AppDbContext context) : base(context) { }

        public IEnumerable<Song> GetSongsSortedByAlphabet()
        {
            return _dbSet.OrderBy(x => x.AlbumId).ThenBy(x => x.Title).ToList();
        }

        // Dönüş tipi, metot ismi ve Include mantığı düzeltildi
        public List<Song> GetSongsByAlbum(int albumId)
        {
            return _dbSet
                // .Include(x => x.Album) // Eğer Album tablosundan verileri de getirmek istiyorsan navigation property'yi dahil etmelisin. AlbumId (int) Include edilmez.
                .Where(x => x.AlbumId == albumId)
                .ToList();
        }
    }
}