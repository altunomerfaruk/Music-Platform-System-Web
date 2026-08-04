using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface ICountryRepository
    {
        IEnumerable<Country> GetAll();

        Country? GetById(int id);

        Country? GetByIsoCode(string isoCode);

        bool Exists(int id);
    }
}