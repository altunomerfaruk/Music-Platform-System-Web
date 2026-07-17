using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;
using System.Collections.Generic;
using System.Linq;

namespace MusicProject.Services.Concrete
{
    public class ArtistManager : IArtistService
    {
        private readonly IArtistRepository _artistRepository;
        private readonly ISongRepository _songRepository;

        public ArtistManager(
            IArtistRepository artistRepository,
            ISongRepository songRepository
        )
        {
            _artistRepository = artistRepository;
            _songRepository = songRepository;
        }

        // --- TEMEL CRUD İŞLEMLERİ ---

        public IEnumerable<Artist> GetAllArtists()
        {
            return _artistRepository.GetAll();
        }

        public Artist? GetArtistById(int id)
        {
            return _artistRepository.GetByID(id);

            // DEĞİŞİKLİK:
            // Artist yerine Artist? yapıldı.
            // Çünkü ID'ye ait sanatçı bulunamazsa repository null dönebilir.
        }

        public void AddArtist(Artist artist)
        {
            _artistRepository.Create(artist);
        }

        public void UpdateArtist(Artist artist)
        {
            _artistRepository.Update(artist);
        }

        public void DeleteArtist(int id)
        {
            _artistRepository.Delete(id);
        }

        public int GetTotalSongCount(int artistId)
        {
            return _songRepository
                .GetAll()
                .Count(song =>
                    song.SongArtists.Any(songArtist =>
                        songArtist.ArtistId == artistId
                    )
                );
        }
    }
}