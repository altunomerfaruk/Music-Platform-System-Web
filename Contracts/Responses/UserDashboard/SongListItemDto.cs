namespace MusicProject.Contracts.Responses.UserDashboard
{
    public class SongListItemDto
    {
        public int SongId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int? AlbumId { get; set; }

        public string AlbumName { get; set; } = string.Empty;

        public int? ArtistId { get; set; }

        public string ArtistName { get; set; } = string.Empty;

        public int TotalStreams { get; set; }
    }
}
