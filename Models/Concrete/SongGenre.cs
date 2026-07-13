using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    [PrimaryKey(nameof(SongId), nameof(GenreId))]
    public class SongGenre
    {
        public int SongId { get; set; }
        public int GenreId { get; set; }

        [ForeignKey("SongId")]
        public virtual Song Song { get; set; } = null!;

        [ForeignKey("GenreId")]
        public virtual Genre Genre { get; set; } = null!;
    }
}
