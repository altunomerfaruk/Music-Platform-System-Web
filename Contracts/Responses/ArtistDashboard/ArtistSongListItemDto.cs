using MusicProject.Models.Enums;

namespace MusicProject.Contracts.Responses.ArtistDashboard
{
    public class ArtistSongListItemDto
    {
        public int SongId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int? AlbumId { get; set; }

        public string AlbumName { get; set; } = string.Empty;

        public int TotalStreams { get; set; }

        public int TotalLikes { get; set; }

        public int PopularityScore { get; set; }

        public PublicationStatus PublicationStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsAdminHidden { get; set; }

        public string? AdminHiddenReason { get; set; }

        public bool IsHiddenByAlbum { get; set; }

        public string? AlbumAdminHiddenReason { get; set; }

        public IEnumerable<string> GenreNames { get; set; } = new List<string>();
    }
}
