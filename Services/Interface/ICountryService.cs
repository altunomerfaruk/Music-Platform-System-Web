using MusicProject.Models.Concrete;

namespace MusicProject.Services.Interface
{
    public interface ICountryService
    {
        IEnumerable<Country> GetAllCountries();

        Country? GetCountryById(int id);

        Country? GetCountryByIsoCode(string isoCode);

        bool CountryExists(int id);
    }
}