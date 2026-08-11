namespace MusicProject.ViewModels.AdminDashboard
{
    public class AdminUsersViewModel : AdminLayoutViewModel
    {
        public string? SearchTerm { get; set; }

        public int DisplayedUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int InactiveUsers { get; set; }

        public List<AdminUserListItemViewModel> Users { get; set; } = [];
    }

    public class AdminUserListItemViewModel
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsPremium { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Initial { get; set; } = string.Empty;

        public bool CanChangeStatus { get; set; }
    }
}