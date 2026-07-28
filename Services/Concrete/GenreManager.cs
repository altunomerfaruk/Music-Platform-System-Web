using MusicProject.Models.Concrete;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class GenreManager : IGenreService
    {
        private readonly IGenreRepository _genreRepository;

        public GenreManager(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public IEnumerable<Genre> GetAllGenres()
        {
            return _genreRepository.GetAllGenres();
        }

        public Genre? GetGenreDetails(int genreId)
        {
            if (genreId <= 0)
            {
                return null;
            }

            return _genreRepository.GetGenreDetailsById(genreId);
        }
    }
}