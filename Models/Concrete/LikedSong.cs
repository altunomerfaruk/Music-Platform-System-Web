using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    [PrimaryKey(nameof(UserId), nameof(SongId))]
    public class LikedSong
    {
        public int UserId { get; set; }
        public int SongId { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!; 

        [ForeignKey("SongId")]
        public virtual Song Song { get; set; } = null!;
    }
}
