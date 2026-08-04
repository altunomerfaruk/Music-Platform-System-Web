namespace MusicProject.ViewModels.ArtistDashboard
{
    public class ArtistStatsViewModel
    {
        public int ArtistId { get; set; } 

        public string FullName { get; set; } = string.Empty;

        public int TotalSongCount { get; set; } 
    }
}