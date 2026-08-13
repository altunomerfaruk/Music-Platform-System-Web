using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicProject.ViewModels.ArtistDashboard;

namespace MusicProject.Controllers
{
    public partial class ArtistDashboardController
    {
        [HttpGet]
        public IActionResult ProfileSettings()
        {
            if (!TryGetDashboard(out var dashboard, out _, out var error))
            {
                return error!;
            }

            var model = new ArtistProfileSettingsViewModel
            {
                Name = dashboard.Artist.Name,
                CountryId = dashboard.Artist.CountryId,
                DebutYear = dashboard.Artist.DebutYear
            };

            FillArtistLayoutData(model, dashboard);
            FillCountryOptions(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProfileSettings(ArtistProfileSettingsViewModel model)
        {
            if (!TryGetDashboard(out var dashboard, out var userId, out var error))
            {
                return error!;
            }

            FillArtistLayoutData(model, dashboard);
            FillCountryOptions(model);

            if (model.CountryId.HasValue &&
                !_countryService.CountryExists(model.CountryId.Value))
            {
                ModelState.AddModelError(
                    nameof(model.CountryId),
                    "Seçilen ülke bulunamadı.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updated = _artistService.UpdateArtistProfile(
                userId,
                model.Name,
                model.CountryId,
                model.DebutYear);

            if (!updated)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Sanatçı profili güncellenemedi.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Sanatçı profiliniz başarıyla güncellendi.";

            return RedirectToAction(nameof(ProfileSettings));
        }

        private void FillCountryOptions(ArtistProfileSettingsViewModel model)
        {
            model.CountryOptions = _countryService
                .GetAllCountries()
                .Select(country => new SelectListItem
                {
                    Value = country.Id.ToString(),
                    Text = country.Name,
                    Selected = model.CountryId == country.Id
                })
                .ToList();
        }
    }
}
