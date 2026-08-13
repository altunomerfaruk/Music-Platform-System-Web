using MusicProject.Contracts.Responses.AdminDashboard;

namespace MusicProject.ViewModels.AdminDashboard
{
    public class AdminDashboardViewModel : AdminLayoutViewModel
    {
        public int TotalListenings { get; set; }

        public List<AdminRecentUserDto> RecentUsers { get; set; } = [];

        public List<AdminTopSongDto> TopSongs { get; set; } = [];

        public List<AdminDailyListeningDto> WeeklyListenings { get; set; } = [];
    }
}
