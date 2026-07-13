using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicProject.Models.Concrete
{
    public class Artist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        public int? DebutYear { get; set; }

        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();

        public virtual ICollection<SongArtist> SongArtists { get; set; } = new List<SongArtist>();

        public virtual ArtistStat? ArtistStat { get; set; }

    }
}