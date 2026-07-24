using MusicProject.DTOs;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;
namespace MusicProject.Services.Concrete
{
    public class SongManager : ISongService
    {
        private readonly ISongRepository _songRepository;

        public SongManager(ISongRepository songRepository)
        {
            _songRepository = songRepository;
        }

        public SongDetailsDto? GetSongDetails(int songId)
        {
            var song = _songRepository.GetSongDetailsById(songId);

            if (song == null)
            {
                return null;
            }

            var artists = song.SongArtists
                .Select(songArtist => new SongArtistDto
                {
                    ArtistId = songArtist.ArtistId,
                    Name = songArtist.Artist.Name
                })
                .ToList();

            var genres = song.SongGenres
                .Select(songGenre => new SongGenreDto
                {
                    GenreId = songGenre.GenreId,
                    Name = songGenre.Genre.Name
                })
                .ToList();

            return new SongDetailsDto
            {
                SongId = song.Id,
                Title = song.Title,
                AlbumId = song.AlbumId,
                AlbumName = song.Album?.Name ?? "Single",
                TotalStreams = song.SongStat?.TotalStreams ?? 0,
                TotalLikes = song.SongStat?.TotalLikes ?? 0,
                PopularityScore = song.SongStat?.PopularityScore ?? 0,
                Artists = artists,
                Genres = genres
            };
        }
        public IEnumerable<Song> GetAllSongs()
        {
            return _songRepository.GetAll();
        }

        public Song? GetSongById(int id)
        {
            return _songRepository.GetByID(id);

        }

        public void AddSong(Song song)
        {
            bool isSongExists = _songRepository
                .GetAll()
                .Any(existingSong =>
                    existingSong.Title.Equals(
                        song.Title,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if (isSongExists)
            {
                throw new InvalidOperationException(
                    $"'{song.Title}' adında bir şarkı zaten sistemde kayıtlı. " +
                    "Lütfen farklı bir isim giriniz."
                );
            }

            _songRepository.Create(song);
        }

        public void UpdateSong(Song song)
        {
            _songRepository.Update(song);
        }

        public void DeleteSong(int id)
        {
            _songRepository.Delete(id);
        }

        public List<Song> GetSongsByAlbum(int albumId)
        {
            return _songRepository.GetSongsByAlbum(albumId);
        }

        public IEnumerable<Song> GetSongsSortedByAlphabet()
        {
            return _songRepository.GetSongsSortedByAlphabet();
        }
        public List<Song> GetPopularSongs()
        {
            return _songRepository.GetPopularSongs();
        }
    }
}