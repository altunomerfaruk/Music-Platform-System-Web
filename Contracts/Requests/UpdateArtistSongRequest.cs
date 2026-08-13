using Microsoft.AspNetCore.Http;
using MusicProject.Models.Enums;

namespace MusicProject.Contracts.Requests
{
    public class UpdateArtistSongRequest
    {
        public int SongId { get; set; }

        public int ArtistId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int? AlbumId { get; set; }

        public int? LabelId { get; set; }

        public IReadOnlyList<int> GenreIds { get; set; } = [];

        public PublicationStatus RequestedStatus { get; set; }

        public DateTime? ScheduledPublishAtLocal { get; set; }

        public IFormFile? AudioFile { get; set; }
    }
}
