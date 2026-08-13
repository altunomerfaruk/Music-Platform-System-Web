using MusicProject.Models.Enums;

namespace MusicProject.Contracts.Responses.AdminDashboard
{
    public class AdminSongListItemDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;

        public string AlbumName { get; set; } = string.Empty;

        public string LabelName { get; set; } = string.Empty;

        public PublicationStatus PublicationStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ScheduledPublishAtUtc { get; set; }

        public DateTime? PublishedAtUtc { get; set; }

        public int TotalStreams { get; set; }

        public int TotalLikes { get; set; }

        public int PopularityScore { get; set; }

        public bool IsAdminHidden { get; set; }

        public string? AdminHiddenReason { get; set; }

        public DateTime? AdminHiddenAtUtc { get; set; }

        public bool IsHiddenByAlbum { get; set; }

        public string? AlbumAdminHiddenReason { get; set; }
    }
}
