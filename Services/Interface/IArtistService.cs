using MusicProject.Models.Concrete;
using System.Collections.Generic;

namespace MusicProject.Services.Interface
{

    public interface IArtistService
    {
        // Temel CRUD işlemleri
        IEnumerable<Artist> GetAllArtists();
        Artist GetArtistById(int id);
        void AddArtist(Artist artist);
        void UpdateArtist(Artist artist);
        void DeleteArtist(int id);

        // Sanatçı İstatistikleri İçin Özel Metotlar
        int GetTotalSongCount(int artistId);
    }
}