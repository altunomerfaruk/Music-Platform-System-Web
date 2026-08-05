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

        bool UpdateArtistAlbum(
           int albumId,
           int artistId,
           string name,
           string? description,
           string? coverImageUrl,
           DateTime releaseDate);

        bool DeleteArtistAlbum(int albumId, int artistId);
    }
}