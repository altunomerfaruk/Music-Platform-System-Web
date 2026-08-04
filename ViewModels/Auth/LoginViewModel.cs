using System.ComponentModel.DataAnnotations;

namespace MusicProject.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        public string Password { get; set; } = string.Empty;
    }
}