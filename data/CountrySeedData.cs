using MusicProject.Models.Concrete;
using System.Globalization;

namespace MusicProject.Data
{
    public static class CountrySeedData
    {
        public static void AddCountries(AppDbContext context)
        {
            var existingIsoCodes = context.Countries
                .Select(country => country.IsoCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var regions = CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(culture =>
                {
                    try
                    {
                        return new RegionInfo(culture.Name);
                    }
                    catch (ArgumentException)
                    {
                        return null;
                    }
                })
                .Where(region => region != null)
                .Select(region => region!)
                .Where(region =>
                    region.TwoLetterISORegionName.Length == 2 &&
                    !existingIsoCodes.Contains(region.TwoLetterISORegionName))
                .GroupBy(
                    region => region.TwoLetterISORegionName,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            var countries = regions
                .Select(region => new Country
                {
                    Name = region.DisplayName.Trim(),
                    IsoCode = region.TwoLetterISORegionName
                        .Trim()
                        .ToUpperInvariant()
                })
                .GroupBy(
                    country => country.IsoCode,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(country => country.Name)
                .ToList();

            if (countries.Count == 0)
            {
                return;
            }

            context.Countries.AddRange(countries);
            context.SaveChanges();
        }
    }
}