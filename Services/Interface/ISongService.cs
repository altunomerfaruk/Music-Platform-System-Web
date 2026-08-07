using MusicProject.Contracts.Responses;
using MusicProject.Models.Concrete;

namespace MusicProject.Services.Interface
{
    public interface ISongService
    {
        IEnumerable<Song> GetAllSongs();

        Song? GetSongById(int id);

        void AddSong(Song song);

        void AddSongWithRelations(Song song, int artistId, IEnumerable<int> genreIds);

        void UpdateSong(Song song);

        void DeleteSong(int id);

        List<Song> GetPopularSongs();

        List<Song> GetSongsByAlbum(int albumId);

        IEnumerable<Song> GetSongsSortedByAlphabet();

        SongDetailsDto? GetSongDetails(int songId);

        IEnumerable<Song> GetSongsByArtistId(int artistId);

        Song? GetArtistSongForEdit(int songId, int artistId);

        void UpdateArtistSong(Song song, int artistId, IEnumerable<int> genreIds);

        void DeleteArtistSong(int songId, int artistId);

        bool UpdatePublication(Song song);
    }
}