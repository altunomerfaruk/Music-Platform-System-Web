using MusicProject.Models.Enums;

namespace MusicProject.Contracts.Responses.AdminDashboard
{
    public class AdminRecentUserDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public string Initial { get; set; } = string.Empty;
    }
}
