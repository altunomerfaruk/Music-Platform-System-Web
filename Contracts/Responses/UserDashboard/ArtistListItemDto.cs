namespace MusicProject.Contracts.Responses.UserDashboard
{
    public class ArtistListItemDto
    {
        public int ArtistId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public int? DebutYear { get; set; }
    }
}
