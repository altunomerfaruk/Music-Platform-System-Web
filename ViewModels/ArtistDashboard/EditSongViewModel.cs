using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MusicProject.ViewModels.ArtistDashboard
{
    public class EditSongViewModel : ArtistLayoutViewModel
    {
        public int SongId { get; set; }

        [Required(ErrorMessage = "Şarkı adı zorunludur.")]
        [MaxLength(100, ErrorMessage = "Şarkı adı en fazla 100 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        public int? AlbumId { get; set; }

        public int? LabelId { get; set; }

        public List<int> SelectedGenreIds { get; set; } = new();

        public IEnumerable<SelectListItem> AlbumOptions { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> GenreOptions { get; set; }
            = new List<SelectListItem>();
    }
}