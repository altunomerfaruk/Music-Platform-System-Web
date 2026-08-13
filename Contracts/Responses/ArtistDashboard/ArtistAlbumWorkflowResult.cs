using MusicProject.Models.Enums;

namespace MusicProject.Contracts.Responses.ArtistDashboard
{
    public sealed class ArtistAlbumWorkflowResult
    {
        public bool Succeeded { get; private init; }

        public ArtistAlbumWorkflowField ErrorField { get; private init; }

        public string? ErrorMessage { get; private init; }

        public string? SuccessMessage { get; private init; }

        public static ArtistAlbumWorkflowResult Success(string message)
        {
            return new ArtistAlbumWorkflowResult
            {
                Succeeded = true,
                SuccessMessage = message
            };
        }

        public static ArtistAlbumWorkflowResult Failure(
            ArtistAlbumWorkflowField field,
            string message)
        {
            return new ArtistAlbumWorkflowResult
            {
                Succeeded = false,
                ErrorField = field,
                ErrorMessage = message
            };
        }
    }
}
