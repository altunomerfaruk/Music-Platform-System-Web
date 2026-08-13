namespace MusicProject.Contracts.Responses.AdminDashboard
{
    public class AdminDashboardDto
    {
        public int TotalListenings { get; set; }

        public List<AdminRecentUserDto> RecentUsers { get; set; } = [];

        public List<AdminTopSongDto> TopSongs { get; set; } = [];

        public List<AdminDailyListeningDto> WeeklyListenings { get; set; } = [];
    }
}
