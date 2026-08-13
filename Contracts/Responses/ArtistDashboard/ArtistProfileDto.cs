namespace MusicProject.Contracts.Responses.ArtistDashboard
{
    public class ArtistProfileDto
    {
        public int ArtistId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int? CountryId { get; set; }

        public string Country { get; set; } = string.Empty;

        public int? DebutYear { get; set; }
    }
}
