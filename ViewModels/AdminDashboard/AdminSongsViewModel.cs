using MusicProject.Models.Enums;

namespace MusicProject.ViewModels.AdminDashboard
{
    public class AdminSongsViewModel : AdminLayoutViewModel
    {
        public string? SearchTerm { get; set; }

        public PublicationStatus? StatusFilter { get; set; }

        public int DisplayedSongs { get; set; }

        public int PublishedSongs { get; set; }

        public int ScheduledSongs { get; set; }

        public int DraftSongs { get; set; }

        public int ArchivedSongs { get; set; }

        public List<AdminSongListItemViewModel> Songs { get; set; } = [];
    }

    public class AdminSongListItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;

        public string AlbumName { get; set; } = string.Empty;

        public string LabelName { get; set; } = string.Empty;

        public PublicationStatus PublicationStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ScheduledPublishAtUtc { get; set; }

        public DateTime? PublishedAtUtc { get; set; }

        public int TotalStreams { get; set; }

        public int TotalLikes { get; set; }

        public int PopularityScore { get; set; }
    }
}