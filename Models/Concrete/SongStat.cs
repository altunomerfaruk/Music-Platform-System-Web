using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    public class SongStat
    {
        [Key]
        public int SongId { get; set; }

        [ForeignKey(nameof(SongId))]
        public virtual Song Song { get; set; } = null!;

        public int TotalStreams { get; set; } = 0;

        public int TotalLikes { get; set; } = 0;

        public int PopularityScore { get; set; } = 0;
    }
}