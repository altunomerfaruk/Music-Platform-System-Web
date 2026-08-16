namespace MusicProject.Contracts.Responses.UserDashboard
{
    public class FilterOptionDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int? ArtistId { get; set; }
    }
}