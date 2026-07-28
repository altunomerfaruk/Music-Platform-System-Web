using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    [PrimaryKey(nameof(SongId), nameof(GenreId))]
    public class SongGenre
    {
        public int SongId { get; set; }

        public int GenreId { get; set; }

        [ForeignKey(nameof(SongId))]
        public virtual Song Song { get; set; } = null!;

        [ForeignKey(nameof(GenreId))]
        public virtual Genre Genre { get; set; } = null!;
    }
}