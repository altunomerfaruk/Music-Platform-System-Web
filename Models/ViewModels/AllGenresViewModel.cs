using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class AllGenresViewModel : UserLayoutViewModel
    {
        public IEnumerable<Genre> Genres { get; set; } = new List<Genre>();
    }
}