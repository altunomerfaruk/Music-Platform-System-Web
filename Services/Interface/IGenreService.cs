using MusicProject.Models.Concrete;

namespace MusicProject.Services.Interface
{
    public interface IGenreService
    {
        IEnumerable<Genre> GetAllGenres();

        Genre? GetGenreDetails(int genreId);
    }
}