using MusicProject.DTOs;

namespace MusicProject.Services.Interface
{
    public interface IAlbumService
    {
        AlbumDetailsDto? GetAlbumDetails(int albumId);
    }
}