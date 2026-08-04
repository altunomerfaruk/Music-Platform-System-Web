namespace MusicProject.Contracts.Responses
{
    public class ArtistDetailsDto
    {
        public int ArtistId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public int? DebutYear { get; set; }

        public int TotalFollowers { get; set; }

        public IEnumerable<ArtistAlbumDto> Albums { get; set; }
            = new List<ArtistAlbumDto>();

        public IEnumerable<ArtistSongDto> Songs { get; set; }
            = new List<ArtistSongDto>();
    }
}