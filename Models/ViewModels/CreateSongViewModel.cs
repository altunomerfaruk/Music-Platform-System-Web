using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MusicProject.Models.ViewModels
{
    public class CreateSongViewModel : ArtistLayoutViewModel
    {
        [Required(ErrorMessage = "Şarkı adı zorunludur.")]
        [MaxLength(100, ErrorMessage = "Şarkı adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Şarkı adı")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Albüm")]
        public int? AlbumId { get; set; }

        [Display(Name = "Plak şirketi")]
        public int? LabelId { get; set; }

        [Display(Name = "Müzik türleri")]
        public List<int> SelectedGenreIds { get; set; } = new();

        public IEnumerable<SelectListItem> AlbumOptions { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> GenreOptions { get; set; }
            = new List<SelectListItem>();
    }
}