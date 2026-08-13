using MusicProject.Contracts.Responses.AdminDashboard;
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

        public List<AdminSongListItemDto> Songs { get; set; } = [];
    }
}
