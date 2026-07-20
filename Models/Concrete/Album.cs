using MusicProject.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    public class Album : BaseEntities
    {
        [Required(ErrorMessage = "Albüm adı zorunludur.")]
        [MaxLength(150, ErrorMessage = "Albüm adı en fazla 150 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        public int ArtistId { get; set; }
        
        [ForeignKey(nameof(ArtistId))]
        public virtual Artist Artist { get; set; } = null!;

        public virtual ICollection<Song> Songs { get; set; }
            = [];

    }
}