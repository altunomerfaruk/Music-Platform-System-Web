using MusicProject.Contracts.Requests;
using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IArtistRepository
    {
        IEnumerable<Artist> GetAll();

        IEnumerable<Artist> SearchArtists(ArtistSearchRequest request);

        int GetArtistCount();

        IEnumerable<Artist> SearchArtistsByText(string query, int? maxResults);

        IEnumerable<Artist> GetFeaturedArtists(int count);

        IEnumerable<string> GetUsedCountryNames();

        Artist? GetByID(int id);

        Artist? GetArtistDetailsById(int artistId);

        Artist? GetArtistDashboardByUserId(int userId);

        void Create(Artist entity);

        void Update(Artist entity);

        void Delete(int id);

        bool UpdateProfileByUserId(int userId,string name,int? countryId,int? debutYear);
    }
}
