namespace MusicProject.Contracts.Responses.UserDashboard
{
    public class SongDetailsDto
    {
        public int SongId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int? AlbumId { get; set; }

        public string AlbumName { get; set; } = string.Empty;

        public int TotalStreams { get; set; }

        public int TotalLikes { get; set; }

        public int PopularityScore { get; set; }

        public IEnumerable<SongArtistDto> Artists { get; set; } = new List<SongArtistDto>();

        public IEnumerable<SongGenreDto> Genres { get; set; } = new List<SongGenreDto>();
    }
}