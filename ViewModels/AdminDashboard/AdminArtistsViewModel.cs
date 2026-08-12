namespace MusicProject.ViewModels.AdminDashboard
{
    public class AdminArtistsViewModel : AdminLayoutViewModel
    {
        public string? SearchTerm { get; set; }

        public int DisplayedArtists { get; set; }

        public int LinkedAccounts { get; set; }

        public int UnlinkedAccounts { get; set; }

        public List<AdminArtistListItemViewModel> Artists { get; set; } = [];
    }

    public class AdminArtistListItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Initial { get; set; } = string.Empty;

        public string CountryName { get; set; } = string.Empty;

        public int? DebutYear { get; set; }

        public bool HasLinkedUser { get; set; }

        public string LinkedUsername { get; set; } = string.Empty;

        public string LinkedEmail { get; set; } = string.Empty;

        public bool IsLinkedUserActive { get; set; }

        public int AlbumCount { get; set; }

        public int SongCount { get; set; }

        public int FollowerCount { get; set; }
    }
}