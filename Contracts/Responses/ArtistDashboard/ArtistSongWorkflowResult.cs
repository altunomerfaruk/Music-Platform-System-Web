using MusicProject.Models.Enums;

namespace MusicProject.Contracts.Responses.ArtistDashboard
{
    public sealed class ArtistSongWorkflowResult
    {
        public bool Succeeded { get; private init; }

        public ArtistSongWorkflowField ErrorField { get; private init; }

        public string? ErrorMessage { get; private init; }

        public string? SuccessMessage { get; private init; }

        public int? AlbumId { get; private init; }

        public static ArtistSongWorkflowResult Success(
            string message,
            int? albumId = null)
        {
            return new ArtistSongWorkflowResult
            {
                Succeeded = true,
                SuccessMessage = message,
                AlbumId = albumId
            };
        }

        public static ArtistSongWorkflowResult Failure(
            ArtistSongWorkflowField field,
            string message)
        {
            return new ArtistSongWorkflowResult
            {
                Succeeded = false,
                ErrorField = field,
                ErrorMessage = message
            };
        }
    }
}
