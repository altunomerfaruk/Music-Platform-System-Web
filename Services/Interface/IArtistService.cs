using MusicProject.DTOs;
using MusicProject.Models.Concrete;

namespace MusicProject.Services.Interface
{
    public interface IArtistService
    {
        IEnumerable<Artist> GetAllArtists();

        Artist? GetArtistById(int id);

        void AddArtist(Artist artist);

        void UpdateArtist(Artist artist);

        void DeleteArtist(int id);

        int GetTotalSongCount(int artistId);

        ArtistDetailsDto? GetArtistDetails(int artistId);
    }
}