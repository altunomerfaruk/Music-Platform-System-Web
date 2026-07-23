namespace MusicProject.DTOs
{
    public class LikedSongDto
    {
        public int SongId { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string AlbumName { get; set; }
            = string.Empty;

        public string ArtistName { get; set; }
            = string.Empty;

        public int TotalStreams { get; set; }

        public DateTime LikedAt { get; set; }
    }
}