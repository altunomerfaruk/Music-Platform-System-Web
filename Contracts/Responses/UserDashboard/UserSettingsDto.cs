using MusicProject.Models.Enums;

namespace MusicProject.Contracts.Responses.UserDashboard
{
    public class UserSettingsDto
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsPremium { get; set; }

        public UserRole Role { get; set; }
    }
}
