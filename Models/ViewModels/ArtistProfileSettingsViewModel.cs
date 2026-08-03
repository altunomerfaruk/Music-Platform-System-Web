using System.ComponentModel.DataAnnotations;

namespace MusicProject.Models.ViewModels
{
    public class ArtistProfileSettingsViewModel : ArtistLayoutViewModel
    {
        [Required(ErrorMessage = "Sanatçı adı zorunludur.")]
        [MaxLength(100, ErrorMessage = "Sanatçı adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Sanatçı Adı")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Ülke adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Ülke")]
        public string? Country { get; set; }

        [Range(1900, 2100, ErrorMessage = "Geçerli bir çıkış yılı giriniz.")]
        [Display(Name = "Çıkış Yılı")]
        public int? DebutYear { get; set; }
    }
}