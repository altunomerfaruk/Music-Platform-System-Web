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

        IEnumerable<Song> GetSongsByArtistId(int artistId);

        List<Song> GetSongsByAlbum(int albumId);

        List<Song> GetPopularSongs();

        Song? GetSongDetailsById(int songId);

        Song? GetSongForListening(int songId);

        bool ExistsByTitleAndArtist(string title, int artistId);

        void CreateSongWithRelations(
            Song song,
            int artistId,
            IEnumerable<int> genreIds);
        Song? GetArtistSongForEdit(int songId, int artistId);

        bool ExistsByTitleAndArtist(string title, int artistId, int excludedSongId);

        void UpdateSongWithRelations(Song song, IEnumerable<int> genreIds);
    }
}