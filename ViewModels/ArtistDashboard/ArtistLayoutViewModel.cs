using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MusicProject.Contracts.Responses.ArtistDashboard;

namespace MusicProject.ViewModels.ArtistDashboard
{
    public class ArtistLayoutViewModel
    {
        [ValidateNever]
        public ArtistProfileDto Artist { get; set; } = null!;

        [ValidateNever]
        public int TotalAlbums { get; set; }

        [ValidateNever]
        public int TotalSongs { get; set; }

        [ValidateNever]
        public string ArtistInitial { get; set; } = "?";
    }
}
