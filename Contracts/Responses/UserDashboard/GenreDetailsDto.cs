namespace MusicProject.Contracts.Responses.UserDashboard
{
    public class GenreDetailsDto
    {
        public int GenreId { get; set; }

        public string Name { get; set; } = string.Empty;

        public IEnumerable<SongListItemDto> Songs { get; set; }
            = new List<SongListItemDto>();
    }
}
