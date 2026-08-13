namespace MusicProject.Contracts.Responses.ArtistDashboard
{
    public class ArtistAlbumListItemDto
    {
        public int AlbumId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? CoverImageUrl { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int SongCount { get; set; }

        public int TotalStreams { get; set; }

        public bool IsAdminHidden { get; set; }

        public string? AdminHiddenReason { get; set; }

        public DateTime? AdminHiddenAtUtc { get; set; }
    }
}
