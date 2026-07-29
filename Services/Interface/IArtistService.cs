using MusicProject.DTOs;
using MusicProject.Models.Concrete;
using MusicProject.Models.ViewModels;

namespace MusicProject.Services.Interface
{
    public interface IArtistService
    {
        IEnumerable<Artist> GetAllArtists();

        Artist? GetArtistById(int id);

        void AddArtist(Artist artist);

        void UpdateArtist(Artist artist);

        ArtistDashboardViewModel? GetArtistDashboard(int userId);

        void DeleteArtist(int id);

        int GetTotalSongCount(int artistId);

        ArtistDetailsDto? GetArtistDetails(int artistId);
    }
}