namespace MusicProject.Contracts.Requests
{
    public class UpdateUserSettingsRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? CurrentPassword { get; set; }

        public string? NewPassword { get; set; }

        public string? ConfirmNewPassword { get; set; }
    }
}
