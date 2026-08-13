namespace MusicProject.Contracts.Responses.AdminDashboard
{
    public class AdminTopSongDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;

        public int TotalStreams { get; set; }
    }
}
