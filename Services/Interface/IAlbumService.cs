using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses;
using MusicProject.Models.Concrete;

namespace MusicProject.Services.Interface
{
    public interface IAlbumService
    {
        AlbumDetailsDto? GetAlbumDetails(int albumId);

        IEnumerable<Album> GetAllAlbums();

        Album? GetArtistAlbumDetails(int albumId, int artistId);

        IEnumerable<Album> GetAlbumsByArtistId(int artistId);

        void AddAlbum(Album album);

        bool UpdateArtistAlbum(UpdateAlbumRequest request);

        bool DeleteArtistAlbum(int albumId, int artistId);

        Album? GetAlbumById(int albumId);

        bool UpdatePublication(Album album);

    }
}