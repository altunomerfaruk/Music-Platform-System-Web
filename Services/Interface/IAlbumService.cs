using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.UserDashboard;
using MusicProject.Models.Concrete;

namespace MusicProject.Services.Interface
{
    public interface IAlbumService
    {
        AlbumDetailsDto? GetAlbumDetails(int albumId);

        IEnumerable<Album> GetAllAlbums();

        IEnumerable<Album> SearchAlbumsByText(string query, int? maxResults);

        Album? GetArtistAlbumDetails(int albumId, int artistId);

        IEnumerable<Album> GetAlbumsByArtistId(int artistId);

        void AddAlbum(Album album);

        bool UpdateArtistAlbum(UpdateAlbumRequest request);

        bool DeleteArtistAlbum(int albumId, int artistId);

        Album? GetAlbumById(int albumId);

        bool UpdatePublication(Album album);
    }
}
