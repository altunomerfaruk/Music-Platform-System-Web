using MusicProject.Models.Core;
using System.ComponentModel.DataAnnotations;

namespace MusicProject.Models.Concrete
{
    public class Genre : BaseEntities
    {
        [Required(ErrorMessage = "Tür adı boş bırakılamaz.")]
        [MaxLength(25, ErrorMessage = "Tür adı en fazla 25 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<SongGenre> SongGenres { get; set; }
            = new List<SongGenre>();
    }
}