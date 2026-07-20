using MusicProject.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models.Concrete
{
    public class Song : BaseEntities
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        // DEĞİŞİKLİK: Title alanının null kalmaması için string.Empty eklendi.

        public int? AlbumId { get; set; }

        public int? LabelId { get; set; }

        [ForeignKey("AlbumId")]
        public virtual Album? Album { get; set; }

        [ForeignKey("LabelId")]
        public virtual RecordLabel? Label { get; set; }

        public virtual SongStat? SongStat { get; set; }

        public virtual ICollection<SongArtist> SongArtists { get; set; }
            = new List<SongArtist>();

        public virtual ICollection<SongGenre> SongGenres { get; set; }
            = new List<SongGenre>();

        public virtual ICollection<LikedSong> LikedSongs { get; set; }
            = new List<LikedSong>();
    }
}