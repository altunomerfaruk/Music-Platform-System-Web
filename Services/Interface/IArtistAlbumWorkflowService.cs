using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.ArtistDashboard;

namespace MusicProject.Services.Interface
{
    public interface IArtistAlbumWorkflowService
    {
        ArtistAlbumWorkflowResult CreateAlbum(CreateArtistAlbumRequest request);

        ArtistAlbumWorkflowResult UpdateAlbum(UpdateArtistAlbumRequest request);

        ArtistAlbumWorkflowResult DeleteAlbum(int albumId, int artistId);
    }
}
