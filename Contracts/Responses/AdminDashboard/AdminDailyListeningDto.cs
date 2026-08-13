namespace MusicProject.Contracts.Responses.AdminDashboard
{
    public class AdminDailyListeningDto
    {
        public DateTime Date { get; set; }

        public int ListeningCount { get; set; }

        public string DayLabel { get; set; } = string.Empty;

        public int BarHeightPercent { get; set; }
    }
}
