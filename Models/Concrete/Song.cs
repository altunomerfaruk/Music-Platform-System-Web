using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    public class Song : BaseEntities
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public int? AlbumId { get; set; }


        public int? LabelId { get; set; }

        //[Required]
        //[Range(1, int.MaxValue, ErrorMessage = "Şarkı süresi 0'dan büyük olmalıdır.")]
        //public int Duration { get; set; }


        [ForeignKey("AlbumId")]
        public virtual Album? Album { get; set; }

        [ForeignKey("LabelId")]
        public virtual RecordLabel? Label { get; set; }

        public virtual SongStat? SongStat { get; set; }

        public virtual ICollection<SongArtist> SongArtists { get; set; } = new List<SongArtist>();
        public virtual ICollection<SongGenre> SongGenres { get; set; } = new List<SongGenre>();

        public virtual ICollection<LikedSong> LikedSongs { get; set; } = new List<LikedSong>();


    }
}
