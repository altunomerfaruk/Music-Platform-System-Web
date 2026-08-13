using MusicProject.Contracts.Responses.AdminDashboard;

namespace MusicProject.ViewModels.AdminDashboard
{
    public class AdminUsersViewModel : AdminLayoutViewModel
    {
        public string? SearchTerm { get; set; }

        public int DisplayedUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int InactiveUsers { get; set; }

        public List<AdminUserListItemDto> Users { get; set; } = [];
    }
}
