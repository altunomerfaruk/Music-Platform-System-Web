using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.ArtistDashboard;
using MusicProject.Contracts.Responses.UserDashboard;
using MusicProject.Models.Concrete;

namespace MusicProject.Services.Interface
{
    public interface IArtistService
    {
        IEnumerable<Artist> GetAllArtists();

        IEnumerable<Artist> SearchArtists(ArtistSearchRequest request);

        int GetArtistCount();

        IEnumerable<Artist> SearchArtistsByText(string query, int? maxResults);

        IEnumerable<Artist> GetFeaturedArtists(int count);

        IEnumerable<string> GetUsedCountryNames();

        Artist? GetArtistById(int id);

        void AddArtist(Artist artist);

        void UpdateArtist(Artist artist);

        ArtistDashboardDto? GetArtistDashboard(int userId);

        void DeleteArtist(int id);

        int GetTotalSongCount(int artistId);

        ArtistDetailsDto? GetArtistDetails(int artistId);

        bool UpdateArtistProfile(
            int userId,
            string name,
            int? countryId,
            int? debutYear);
    }
}
