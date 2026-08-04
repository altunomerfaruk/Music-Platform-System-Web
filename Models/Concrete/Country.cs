using MusicProject.Models.Core;
using System.ComponentModel.DataAnnotations;

namespace MusicProject.Models.Concrete
{
    public class Country : BaseEntities
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        public string IsoCode { get; set; } = string.Empty;

        public virtual ICollection<Artist> Artists { get; set; }
            = new List<Artist>();
    }
}