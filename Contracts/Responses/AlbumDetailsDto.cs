namespace MusicProject.Contracts.Responses
{
    public class AlbumDetailsDto
    {
        public int AlbumId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? CoverImageUrl { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int ArtistId { get; set; }

        public string ArtistName { get; set; } = string.Empty;

        public IEnumerable<AlbumSongDto> Songs { get; set; } = new List<AlbumSongDto>();
    }
}