using System.ComponentModel.DataAnnotations;

namespace MusicProject.ViewModels.ArtistDashboard
{
    public class CreateAlbumViewModel : ArtistLayoutViewModel
    {
        [Required(ErrorMessage = "Albüm adı zorunludur.")]
        [MaxLength(150, ErrorMessage = "Albüm adı en fazla 150 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string? Description { get; set; }

        [MaxLength(500, ErrorMessage = "Kapak görseli adresi en fazla 500 karakter olabilir.")]
        [Url(ErrorMessage = "Geçerli bir görsel adresi giriniz.")]
        public string? CoverImageUrl { get; set; }

        [Required(ErrorMessage = "Yayın tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } = DateTime.Today;
    }
}