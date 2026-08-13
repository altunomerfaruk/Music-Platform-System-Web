namespace MusicProject.Contracts.Requests
{
    public class SongSearchRequest
    {
        public string? Search { get; set; }

        public int? ArtistId { get; set; }

        public int? AlbumId { get; set; }

        public int? GenreId { get; set; }

        public string? Sort { get; set; }

        public bool LikedOnly { get; set; }

        public int UserId { get; set; }
    }
}
