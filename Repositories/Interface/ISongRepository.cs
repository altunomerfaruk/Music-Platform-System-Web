using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface ISongRepository : IGenericRepository<Song>
    {
        IEnumerable<Song> GetSongsSortedByAlphabet();
        List<Song> GetSongsByAlbum(int albumId);

        List<Song> GetPopularSongs();
    }
}