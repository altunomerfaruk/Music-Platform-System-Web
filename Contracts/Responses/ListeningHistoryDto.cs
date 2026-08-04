namespace MusicProject.Contracts.Responses
{
    public class ListeningHistoryDto
    {
        public int ListeningHistoryId { get; set; }

        public int SongId { get; set; }

        public string SongTitle { get; set; } = string.Empty;

        public int? AlbumId { get; set; }

        public string AlbumName { get; set; } = string.Empty;

        public int? ArtistId { get; set; }

        public string ArtistName { get; set; } = string.Empty;

        public string GenreName { get; set; } = string.Empty;

        public DateTime ListenedAt { get; set; }
    }
}