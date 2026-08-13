namespace MusicProject.Contracts.Responses.UserDashboard
{
    public class GenreListItemDto
    {
        public int GenreId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int SongCount { get; set; }
    }
}
