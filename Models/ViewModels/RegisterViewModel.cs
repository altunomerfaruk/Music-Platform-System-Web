using System.ComponentModel.DataAnnotations;

namespace MusicProject.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;
        // DEĞİŞİKLİK: null uyarısı oluşmaması için = string.Empty eklendi.

        [Required(ErrorMessage = "E-Posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi girin.")]
        [Display(Name = "E-Posta")]
        public string Email { get; set; } = string.Empty;
        // DEĞİŞİKLİK: null uyarısı oluşmaması için = string.Empty eklendi.

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifreniz en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;
        // DEĞİŞİKLİK: null uyarısı oluşmaması için = string.Empty eklendi.

        [Required(ErrorMessage = "Şifre tekrar alanı zorunludur.")]
        [Compare("Password", ErrorMessage = "Şifreler birbiriyle uyuşmuyor!")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        public string ConfirmPassword { get; set; } = string.Empty;
        // DEĞİŞİKLİK: null uyarısı oluşmaması için = string.Empty eklendi.
    }
}