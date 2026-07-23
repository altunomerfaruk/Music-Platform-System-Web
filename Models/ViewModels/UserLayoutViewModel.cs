namespace MusicProject.Models.ViewModels
{
    public class UserLayoutViewModel
    {
        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public int LikedSongCount { get; set; }

        public int FollowedArtistCount { get; set; }
    }
}
