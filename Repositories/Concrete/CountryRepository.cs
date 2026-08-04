using Microsoft.EntityFrameworkCore;
using MusicProject.Data;
using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;

namespace MusicProject.Repositories.Concrete
{
    public class CountryRepository : ICountryRepository
    {
        private readonly AppDbContext _context;

        public CountryRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Country> GetAll()
        {
            return _context.Countries
                .AsNoTracking()
                .OrderBy(country => country.Name)
                .ToList();
        }

        public Country? GetById(int id)
        {
            return _context.Countries
                .AsNoTracking()
                .FirstOrDefault(country => country.Id == id);
        }

        public Country? GetByIsoCode(string isoCode)
        {
            if (string.IsNullOrWhiteSpace(isoCode))
            {
                return null;
            }

            var normalizedIsoCode = isoCode
                .Trim()
                .ToUpperInvariant();

            return _context.Countries
                .AsNoTracking()
                .FirstOrDefault(country =>
                    country.IsoCode == normalizedIsoCode);
        }

        public bool Exists(int id)
        {
            return id > 0 &&
                   _context.Countries.Any(country => country.Id == id);
        }
    }
}