using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class CountryManager : ICountryService
    {
        private readonly ICountryRepository _countryRepository;

        public CountryManager(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        public IEnumerable<Country> GetAllCountries()
        {
            return _countryRepository.GetAll();
        }

        public Country? GetCountryById(int id)
        {
            if (id <= 0)
            {
                return null;
            }
            return _countryRepository.GetById(id);
        }

        public Country? GetCountryByIsoCode(string isoCode)
        {
            if (string.IsNullOrWhiteSpace(isoCode))
            {
                return null;
            }
            return _countryRepository.GetByIsoCode(isoCode);
        }

        public bool CountryExists(int id)
        {
            return _countryRepository.Exists(id);
        }
    }
}
