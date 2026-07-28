using System.ComponentModel.DataAnnotations;

namespace MusicProject.Models.ViewModels
{
    public class UserSettingsViewModel : UserLayoutViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "E-posta adresi en fazla 100 karakter olabilir.")]
        [Display(Name = "E-posta adresi")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Mevcut şifre")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
        [MaxLength(50, ErrorMessage = "Yeni şifre en fazla 50 karakter olabilir.")]
        [Display(Name = "Yeni şifre")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Yeni şifreler eşleşmiyor.")]
        [Display(Name = "Yeni şifre tekrar")]
        public string? ConfirmNewPassword { get; set; }

        public bool IsPremium { get; set; }

        public string RoleName { get; set; } = string.Empty;
    }
}