using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IArtistRepository
    {
        IEnumerable<Artist> GetAll();

        Artist? GetByID(int id);

        Artist? GetArtistDetailsById(int artistId);

        Artist? GetArtistDashboardByUserId(int userId);

        void Create(Artist entity);

        void Update(Artist entity);

        void Delete(int id);

        bool UpdateProfileByUserId(int userId,string name,int? countryId,int? debutYear);
    }
}