using MusicProject.Contracts.Responses.AdminDashboard;
using MusicProject.Models.Enums;

namespace MusicProject.ViewModels.AdminDashboard
{
    public class AdminAlbumsViewModel : AdminLayoutViewModel
    {
        public string? SearchTerm { get; set; }

        public PublicationStatus? StatusFilter { get; set; }

        public int DisplayedAlbums { get; set; }

        public int PublishedAlbums { get; set; }

        public int ScheduledAlbums { get; set; }

        public int DraftAlbums { get; set; }

        public int ArchivedAlbums { get; set; }

        public List<AdminAlbumListItemDto> Albums { get; set; } = [];
    }
}
