using MusicProject.Models.Enums;

namespace MusicProject.Contracts.Requests
{
    public class UpdateAlbumRequest
    {
        public int AlbumId { get; set; }

        public int ArtistId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? CoverImageUrl { get; set; }

        public DateTime ReleaseDate { get; set; }

        public PublicationStatus PublicationStatus { get; set; }

        public DateTime? ScheduledPublishAtUtc { get; set; }

        public DateTime? PublishedAtUtc { get; set; }
    }
}