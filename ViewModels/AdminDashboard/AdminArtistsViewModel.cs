using MusicProject.Contracts.Responses.AdminDashboard;

namespace MusicProject.ViewModels.AdminDashboard
{
    public class AdminArtistsViewModel : AdminLayoutViewModel
    {
        public string? SearchTerm { get; set; }

        public int DisplayedArtists { get; set; }

        public int LinkedAccounts { get; set; }

        public int UnlinkedAccounts { get; set; }

        public List<AdminArtistListItemDto> Artists { get; set; } = [];
    }
}
