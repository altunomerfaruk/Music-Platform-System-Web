using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.UserDashboard;
using MusicProject.Models.Concrete;

namespace MusicProject.Services.Interface
{
    public interface ISongService
    {
        IEnumerable<Song> GetAllSongs();

        Song? GetSongById(int id);

        void AddSong(Song song);

        void AddSongWithRelations(Song song, int artistId, IEnumerable<int> genreIds);

        bool TitleExistsForArtist(string title, int artistId);

        void UpdateSong(Song song);

        void DeleteSong(int id);

        List<Song> GetPopularSongs();

        List<Song> GetSongsByAlbum(int albumId);

        IEnumerable<Song> GetSongsSortedByAlphabet();

        IEnumerable<Song> SearchSongs(SongSearchRequest request);

        int GetVisibleSongCount();

        IEnumerable<Song> SearchSongsByText(string query, int? maxResults);

        SongDetailsDto? GetSongDetails(int songId);

        IEnumerable<Song> GetSongsByArtistId(int artistId);

        Song? GetArtistSongForEdit(int songId, int artistId);

        void UpdateArtistSong(Song song, int artistId, IEnumerable<int> genreIds);

        void DeleteArtistSong(int songId, int artistId);

        bool UpdatePublication(Song song);

        Song? GetSongForListening(int songId);
    }
}
