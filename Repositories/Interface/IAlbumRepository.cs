using MusicProject.Contracts.Requests;
using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IAlbumRepository
    {
        Album? GetAlbumDetailsById(int albumId);

        IEnumerable<Album> GetAllAlbums();

        IEnumerable<Album> GetAlbumsByArtistId(int artistId);

        Album? GetArtistAlbumDetails(int albumId, int artistId);

        void Create(Album album);

        bool UpdateArtistAlbum(UpdateAlbumRequest request);

        bool DeleteArtistAlbum(int albumId, int artistId);

        Album? GetAlbumById(int albumId);

        bool UpdatePublication(Album album);
    }
}