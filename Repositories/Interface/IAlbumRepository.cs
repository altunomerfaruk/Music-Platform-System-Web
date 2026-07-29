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

        void Update(Album album);

        void Delete(int albumId);
    }
}