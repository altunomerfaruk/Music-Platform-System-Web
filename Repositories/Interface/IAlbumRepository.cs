using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IAlbumRepository
    {
        Album? GetAlbumDetailsById(int albumId);

        IEnumerable<Album> GetAllAlbums();
    }
}