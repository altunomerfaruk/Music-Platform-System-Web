using MusicProject.Models.Concrete;

namespace MusicProject.Models.ViewModels
{
    public class UserDashboardViewModel
    {
        public string Username { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string Role { get; set; }
            = string.Empty;

        public IEnumerable<Song> PopularSongs { get; set; }
            = new List<Song>();

        public IEnumerable<Artist> Artists { get; set; }
            = new List<Artist>();

        public HashSet<int> LikedSongIds { get; set; }
            = new HashSet<int>();

        public HashSet<int> FollowedArtistIds { get; set; }
            = new HashSet<int>();

    }
}