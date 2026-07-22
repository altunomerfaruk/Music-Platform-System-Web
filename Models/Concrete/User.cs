using MusicProject.Models.Core;
// DEĞİŞİKLİK: BaseEntities sınıfını kullanabilmek için eklendi.

using MusicProject.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MusicProject.Models.Concrete
{
    public class User : BaseEntities
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Password { get; set; } = string.Empty;


        public UserRole Role { get; set; } = UserRole.User;


        public bool IsActive { get; set; } = true;

        public bool? IsPremium { get; set; }

        public virtual Artist? ArtistProfile { get; set; }
        public virtual ICollection<FollowedArtist> FollowedArtists
        {
            get;
            set;
        } = new List<FollowedArtist>();


    }
}