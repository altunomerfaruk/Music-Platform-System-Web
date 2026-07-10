using Microsoft.EntityFrameworkCore;
using MusicProject.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicProject.Models.Concrete
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    public class User : BaseEntities
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Password { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        public bool IsAdmin { get; set; } = false;

        public bool? IsPremium { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<LikedSong> LikedSong { get; set; } = new List<LikedSong>();

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}