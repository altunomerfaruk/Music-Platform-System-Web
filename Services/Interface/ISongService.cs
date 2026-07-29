using MusicProject.DTOs;
using MusicProject.Models.Concrete;

namespace MusicProject.Services.Interface
{
    public interface ISongService
    {
        IEnumerable<Song> GetAllSongs();

        Song? GetSongById(int id);

        void AddSong(Song song);

        void AddSongWithRelations(
            Song song,
            int artistId,
            IEnumerable<int> genreIds);

        void UpdateSong(Song song);

        void DeleteSong(int id);

        List<Song> GetPopularSongs();

        List<Song> GetSongsByAlbum(int albumId);

        IEnumerable<Song> GetSongsSortedByAlphabet();

        SongDetailsDto? GetSongDetails(int songId);
    }
}