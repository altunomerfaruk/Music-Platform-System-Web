using MusicProject.Models.Core;
using MusicProject.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    public class Song : BaseEntities
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public int? AlbumId { get; set; }

        public int? LabelId { get; set; }

        public PublicationStatus PublicationStatus { get; set; } = PublicationStatus.Draft;

        [MaxLength(100)]
        public string? PublicationJobId { get; set; }

        public DateTime? ScheduledPublishAtUtc { get; set; }

        public DateTime? PublishedAtUtc { get; set; }

        [ForeignKey(nameof(AlbumId))]
        public virtual Album? Album { get; set; }

        [ForeignKey(nameof(LabelId))]
        public virtual RecordLabel? Label { get; set; }

        public virtual SongStat? SongStat { get; set; }

        public virtual ICollection<SongArtist> SongArtists { get; set; }
            = new List<SongArtist>();

        public virtual ICollection<SongGenre> SongGenres { get; set; }
            = new List<SongGenre>();

        public virtual ICollection<LikedSong> LikedSongs { get; set; }
            = new List<LikedSong>();

        public virtual ICollection<ListeningHistory> ListeningHistories { get; set; }
            = new List<ListeningHistory>();
    }
}