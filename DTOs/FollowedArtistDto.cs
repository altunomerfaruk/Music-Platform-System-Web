namespace MusicProject.DTOs
{
    public class FollowedArtistDto
    {
        public int ArtistId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public int? DebutYear { get; set; }

        public int TotalFollowers { get; set; }

        public DateTime FollowedAt { get; set; }
    }
}