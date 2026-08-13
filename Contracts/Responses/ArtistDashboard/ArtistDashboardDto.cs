namespace MusicProject.Contracts.Responses.ArtistDashboard
{
    public class ArtistDashboardDto
    {
        public ArtistProfileDto Artist { get; set; } = null!;

        public int TotalAlbums { get; set; }

        public int TotalSongs { get; set; }

        public IEnumerable<ArtistSongListItemDto> PopularSongs { get; set; }
            = new List<ArtistSongListItemDto>();

        public IEnumerable<ArtistAlbumListItemDto> RecentAlbums { get; set; }
            = new List<ArtistAlbumListItemDto>();

        public int TotalStreams { get; set; }

        public int TotalLikes { get; set; }

        public int MonthlyListeners { get; set; }

        public int TotalFollowers { get; set; }
    }
}
