using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicProject.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MusicProject.ViewModels.ArtistDashboard
{
    public class EditSongViewModel : ArtistLayoutViewModel
    {
        public int SongId { get; set; }

        [Required(ErrorMessage = "Şarkı adı zorunludur.")]
        [MaxLength(100, ErrorMessage = "Şarkı adı en fazla 100 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        public IFormFile? AudioFile { get; set; }

        public bool HasAudioFile { get; set; }

        public int? AlbumId { get; set; }

        public int? LabelId { get; set; }

        [Required(ErrorMessage = "En az bir tür seçmelisiniz.")]
        public List<int> SelectedGenreIds { get; set; } = [];

        [Required(ErrorMessage = "Yayın durumu seçilmelidir.")]
        public PublicationStatus PublicationStatus { get; set; }

        [Display(Name = "Planlanan yayın zamanı")]
        public DateTime? ScheduledPublishAtLocal { get; set; }

        public bool IsAdminHidden { get; set; }

        public string? AdminHiddenReason { get; set; }

        public DateTime? AdminHiddenAtUtc { get; set; }

        public bool IsHiddenByAlbum { get; set; }

        public string? AlbumAdminHiddenReason { get; set; }

        public IEnumerable<SelectListItem> AlbumOptions { get; set; } = [];

        public IEnumerable<SelectListItem> GenreOptions { get; set; } = [];
    }
}