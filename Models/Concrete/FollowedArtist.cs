using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    [PrimaryKey(
        nameof(UserId),
        nameof(ArtistId)
    )]
    public class FollowedArtist
    {
        public int UserId { get; set; }

        public int ArtistId { get; set; }

        public DateTime FollowedAt { get; set; }
            = DateTime.UtcNow;

        public bool IsActive { get; set; }
            = true;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
            = null!;

        [ForeignKey(nameof(ArtistId))]
        public virtual Artist Artist { get; set; }
            = null!;
    }
}