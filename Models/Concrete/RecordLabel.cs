using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace MusicProject.Models.Concrete
{
    public class RecordLabel : BaseEntities
    {
        [Required(ErrorMessage = "Plak şirketi adı zorunludur.")]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(100)]
        public string? Country { get; set; }

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
    }
}