using MusicProject.Models.Enums;

namespace MusicProject.Contracts.Requests
{
    public class CreateArtistAlbumRequest
    {
        public int ArtistId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? CoverImageUrl { get; set; }

        public DateTime ReleaseDate { get; set; }

        public PublicationStatus RequestedStatus { get; set; }

        public DateTime? ScheduledPublishAtLocal { get; set; }
    }
}
