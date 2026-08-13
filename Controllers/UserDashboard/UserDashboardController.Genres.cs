using Microsoft.AspNetCore.Mvc;
using MusicProject.Contracts.Responses.UserDashboard;
using MusicProject.ViewModels.UserDashboard;

namespace MusicProject.Controllers
{
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
                    .Select(genre => new GenreListItemDto
                    {
                        GenreId = genre.Id,
                        Name = genre.Name,
                        SongCount = genre.SongGenres.Count
                    })
                    .ToList()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

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
                Genre = new GenreDetailsDto
                {
                    GenreId = genre.Id,
                    Name = genre.Name,
                    Songs = genre.SongGenres
                        .Select(songGenre => songGenre.Song)
                        .OrderBy(song => song.Title)
                        .Select(ToSongListItem)
                        .ToList()
                },

                LikedSongIds = _likedSongService
                    .GetActiveLikedSongIds(userId)
                    .ToHashSet()
            };

            FillLayoutData(model, userId);

            return View(model);
        }
    }
}
