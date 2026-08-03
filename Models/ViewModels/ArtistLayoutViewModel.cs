using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class ArtistLayoutViewModel
    {
        [ValidateNever]
        public Artist Artist { get; set; } = null!;

        [ValidateNever]
        public int TotalAlbums { get; set; }

        [ValidateNever]
        public int TotalSongs { get; set; }

        [ValidateNever]
        public string ArtistInitial { get; set; } = "?";
    }
}