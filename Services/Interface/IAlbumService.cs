using MusicProject.DTOs;
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

        void UpdateAlbum(Album album);

        void DeleteAlbum(int albumId);
    }
}