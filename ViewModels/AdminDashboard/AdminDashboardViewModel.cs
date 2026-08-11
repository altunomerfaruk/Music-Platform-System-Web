namespace MusicProject.ViewModels.AdminDashboard
{
    public class AdminDashboardViewModel : AdminLayoutViewModel
    {
        public int TotalListenings { get; set; }

        public List<AdminRecentUserViewModel> RecentUsers { get; set; } = [];

        public List<AdminTopSongViewModel> TopSongs { get; set; } = [];

        public List<AdminDailyListeningViewModel> WeeklyListenings { get; set; } = [];
    }

    public class AdminRecentUserViewModel
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string Initial { get; set; } = string.Empty;
    }

    public class AdminTopSongViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;

        public int TotalStreams { get; set; }
    }

    public class AdminDailyListeningViewModel
    {
        public string DayLabel { get; set; } = string.Empty;

        public int ListeningCount { get; set; }

        public int BarHeightPercent { get; set; }
    }
}