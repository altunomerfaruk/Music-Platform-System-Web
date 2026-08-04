using MusicProject.Models.Concrete;

namespace MusicProject.ViewModels.UserDashboard
{
    public class AllGenresViewModel : UserLayoutViewModel
    {
        public IEnumerable<Genre> Genres { get; set; } = new List<Genre>();
    }
}