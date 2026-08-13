using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.ArtistDashboard;

namespace MusicProject.Services.Interface
{
    public interface IArtistSongWorkflowService
    {
        Task<ArtistSongWorkflowResult> CreateSongAsync(CreateArtistSongRequest request);

        Task<ArtistSongWorkflowResult> UpdateSongAsync(UpdateArtistSongRequest request);

        ArtistSongWorkflowResult DeleteSong(int songId, int artistId);
    }
}
