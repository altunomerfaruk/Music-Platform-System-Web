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

        public ArtistManager(IArtistRepository artistRepository, ISongRepository songRepository)
        {
            _artistRepository = artistRepository;
            _songRepository = songRepository;
        }
        // --- TEMEL CRUD İŞLEMLERİ ---

        public IEnumerable<Artist> GetAllArtists()
        {
            return _artistRepository.GetAll();
        }

        public Artist GetArtistById(int id)
        {
            return _artistRepository.GetByID(id);
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

        // --- İSTATİSTİK İÇİN ÖZEL METOT ---

        public int GetTotalSongCount(int artistId)
        {
            // Şarkıları getir ve içinde bu sanatçının (artistId) bulunduğu şarkıları say
            return _songRepository.GetAll().Count(x => x.SongArtists.Any(sa => sa.ArtistId == artistId));
        }
    }
}