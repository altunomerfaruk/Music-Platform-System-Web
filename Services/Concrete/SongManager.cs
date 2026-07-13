using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;


namespace MusicProject.Services.Concrete
{
    public class SongManager : ISongService
    {
        // Repository'yi readonly olarak enjekte ediyoruz
        private readonly ISongRepository _songRepository;

        public SongManager(ISongRepository songRepository)
        {
            _songRepository = songRepository;
        }

        public IEnumerable<Song> GetAllSongs()
        {
            return _songRepository.GetAll();
        }

        public Song GetSongById(int id)
        {
            return _songRepository.GetByID(id);
        }

        public void AddSong(Song song)
        {

            bool isSongExists = _songRepository.GetAll()
                .Any(x => x.Title.Equals(song.Title, StringComparison.OrdinalIgnoreCase));

            if (isSongExists)
            {

                throw new InvalidOperationException($"'{song.Title}' adında bir şarkı zaten sistemde kayıtlı. Lütfen farklı bir isim giriniz.");
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
    }
}