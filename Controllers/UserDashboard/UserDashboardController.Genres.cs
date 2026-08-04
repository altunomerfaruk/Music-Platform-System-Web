using Microsoft.AspNetCore.Mvc;
using MusicProject.ViewModels.UserDashboard;

namespace MusicProject.Controllers
{
    // Muzik turu listeleme / detay aksiyonlari.
    public partial class UserDashboardController
    {
        [HttpGet]
        public IActionResult AllGenres()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var model = new AllGenresViewModel
            {
                Genres = _genreService
                    .GetAllGenres()
                    .OrderBy(genre => genre.Name)
                    .ToList()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        /// <summary>
        /// Secilen muzik turunu ve o ture bagli sarkilari gosterir.
        /// </summary>
        [HttpGet]
        public IActionResult GenreDetails(int genreId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            if (genreId <= 0)
            {
                return BadRequest("Geçersiz tür bilgisi.");
            }

            var genre = _genreService.GetGenreDetails(genreId);

            if (genre == null)
            {
                return NotFound("Tür bulunamadı.");
            }

            var model = new GenreDetailsViewModel
            {
                Genre = genre,

                LikedSongIds = _likedSongService
                    .GetActiveLikedSongIds(userId)
                    .ToHashSet()
            };

            FillLayoutData(model, userId);

            return View(model);
        }
    }
}
