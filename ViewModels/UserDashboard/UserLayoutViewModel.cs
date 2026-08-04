using System.ComponentModel.DataAnnotations;

namespace MusicProject.ViewModels.UserDashboard
{
    public class UserLayoutViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [MaxLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Kullanıcı adı")]
        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public int LikedSongCount { get; set; }

        public int FollowedArtistCount { get; set; }
    }
}