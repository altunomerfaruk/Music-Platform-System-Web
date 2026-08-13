using MusicProject.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MusicProject.ViewModels.ArtistDashboard
{
    public class EditAlbumViewModel : ArtistLayoutViewModel
    {
        public int AlbumId { get; set; }

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
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "Yayın durumu seçilmelidir.")]
        public PublicationStatus PublicationStatus { get; set; }

        [Display(Name = "Planlanan yayın zamanı")]
        public DateTime? ScheduledPublishAtLocal { get; set; }

        public bool IsAdminHidden { get; set; }

        public string? AdminHiddenReason { get; set; }

        public DateTime? AdminHiddenAtUtc { get; set; }
    }
}