using MusicProject.Models.Enums;

namespace MusicProject.Contracts.Responses.AdminDashboard
{
    public class AdminAlbumListItemDto
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
