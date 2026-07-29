using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface ISongRepository
    {
        IEnumerable<Song> GetAll();

        Song? GetByID(int id);

        void Create(Song entity);

        void Update(Song entity);

        void Delete(int id);

        IEnumerable<Song> GetSongsSortedByAlphabet();

        List<Song> GetSongsByAlbum(int albumId);

        List<Song> GetPopularSongs();

        Song? GetSongDetailsById(int songId);

        Song? GetSongForListening(int songId);

        // YENİ:
        // Aynı sanatçıda aynı isimli şarkı bulunup bulunmadığını kontrol eder.
        bool ExistsByTitleAndArtist(string title, int artistId);

        void CreateSongWithRelations(
            Song song,
            int artistId,
            IEnumerable<int> genreIds);
    }
}