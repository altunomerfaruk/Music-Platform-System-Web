namespace MusicProject.DTOs
{
    public class ArtistAlbumDto
    {
        public int AlbumId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime ReleaseDate { get; set; }

        public int SongCount { get; set; }
    }
}