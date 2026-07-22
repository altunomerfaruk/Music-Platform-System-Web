using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    public class Artist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Country { get; set; }

        public int? DebutYear { get; set; }

        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        public virtual ICollection<Album> Albums { get; set; }
            = new List<Album>();

        public virtual ICollection<SongArtist> SongArtists { get; set; }
            = new List<SongArtist>();

        public virtual ArtistStat? ArtistStat { get; set; }

        public virtual ICollection<FollowedArtist> Followers
        {
            get;
            set;
        } = new List<FollowedArtist>();
    }
}