using Microsoft.EntityFrameworkCore;
using MusicProject.Core.Abstract;
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

        public bool IsPremium { get; set; } = false; 

        public bool IsActive { get; set; } = true; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<LikedSong> LikedSongs { get; set; }

        public virtual ICollection<SubscriptionPayment>? Payments { get; set; }


        public User()
        {
            LikedSongs = new List<LikedSong>();
            Payments = new List<Payment>();
        }
    }
}