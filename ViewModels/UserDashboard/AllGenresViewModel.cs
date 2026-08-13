using MusicProject.Contracts.Responses.UserDashboard;

namespace MusicProject.ViewModels.UserDashboard
{
    public class AllGenresViewModel : UserLayoutViewModel
    {
        public IEnumerable<GenreListItemDto> Genres { get; set; }
            = new List<GenreListItemDto>();
    }
}
