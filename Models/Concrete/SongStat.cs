using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    public class SongStat : BaseEntities
    {
        [Key]
        [ForeignKey("Song")]
        public int SongId { get; set; }

        [Required]
        public int TotalStreams { get; set; } = 0;

        [Required]
        public int TotalLikes { get; set; } = 0;

        [Required]
        public int PopularityScore { get; set; } = 0;

        public virtual Song Song { get; set; } = null!;
    }
}
