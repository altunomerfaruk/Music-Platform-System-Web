namespace MusicProject.Contracts.Requests
{
    public class ArtistSearchRequest
    {
        public string? Search { get; set; }

        public string? Country { get; set; }

        public string? Sort { get; set; }

        public bool FollowedOnly { get; set; }

        public int UserId { get; set; }
    }
}
