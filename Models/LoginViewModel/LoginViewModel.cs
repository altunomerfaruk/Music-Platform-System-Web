using System.ComponentModel.DataAnnotations;

namespace MusicProject.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Lütfen e-posta adresinizi giriniz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lütfen şifrenizi giriniz.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; } // "Beni Hatırla" seçeneği için
    }
}