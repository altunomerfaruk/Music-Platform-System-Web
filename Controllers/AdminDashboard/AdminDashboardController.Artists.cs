using Microsoft.AspNetCore.Mvc;
using MusicProject.ViewModels.AdminDashboard;

namespace MusicProject.Controllers
{
    public partial class AdminDashboardController
    {
        [HttpGet]
        public IActionResult Artists(string? search)
        {
            var artists = _adminDashboardService.GetArtists(search).ToList();

            foreach (var artist in artists)
            {
                artist.Initial = GetInitial(artist.Name);
                artist.CountryName = OrFallback(artist.CountryName, "Belirtilmedi");
                artist.LinkedUsername = OrFallback(artist.LinkedUsername, "Bağlı hesap yok");
                artist.LinkedEmail = OrFallback(artist.LinkedEmail, "-");
            }

            var model = new AdminArtistsViewModel
            {
                SearchTerm = NormalizeSearch(search),
                DisplayedArtists = artists.Count,
                LinkedAccounts = artists.Count(artist => artist.HasLinkedUser),
                UnlinkedAccounts = artists.Count(artist => !artist.HasLinkedUser),
                Artists = artists
            };

            FillLayoutData(model);

            return View(model);
        }
    }
}
