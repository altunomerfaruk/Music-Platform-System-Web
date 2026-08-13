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

        public List<AdminAlbumListItemViewModel> Albums { get; set; } = [];
    }

    public class AdminAlbumListItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;

        public string? CoverImageUrl { get; set; }

        public DateTime ReleaseDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public PublicationStatus PublicationStatus { get; set; }

        public DateTime? ScheduledPublishAtUtc { get; set; }

        public DateTime? PublishedAtUtc { get; set; }

        public int SongCount { get; set; }

        public bool IsAdminHidden { get; set; }

        public string? AdminHiddenReason { get; set; }

        public DateTime? AdminHiddenAtUtc { get; set; }
    }
}