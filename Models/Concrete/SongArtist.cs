using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    [PrimaryKey(nameof(SongId), nameof(ArtistId))]
    public class SongArtist
    {
        public int SongId { get; set; }
        public int ArtistId { get; set; }

        [ForeignKey("SongId")]
        public virtual Song Song { get; set; } = null!;

        [ForeignKey("ArtistId")]
        public virtual Artist Artist { get; set; } = null!;
    }
}
