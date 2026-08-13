namespace MusicProject.Contracts.Responses.UserDashboard
{
    public class AlbumSongDto
    {
        public int SongId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int TotalStreams { get; set; }

        public int TotalLikes { get; set; }

        public int PopularityScore { get; set; }
    }
}