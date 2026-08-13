namespace MusicProject.Contracts.Responses.UserDashboard
{
    public class AlbumListItemDto
    {
        public int AlbumId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;
    }
}
