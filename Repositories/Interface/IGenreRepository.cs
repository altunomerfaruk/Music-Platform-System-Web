using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IGenreRepository
    {
        IEnumerable<Genre> GetAllGenres();

        Genre? GetGenreDetailsById(int genreId);
    }
}