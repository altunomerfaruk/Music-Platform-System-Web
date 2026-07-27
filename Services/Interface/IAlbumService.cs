using MusicProject.DTOs;
using MusicProject.Models.Concrete;
namespace MusicProject.Services.Interface
{
    public interface IAlbumService
    {
        AlbumDetailsDto? GetAlbumDetails(int albumId);

        IEnumerable<Album> GetAllAlbums();
    }
}