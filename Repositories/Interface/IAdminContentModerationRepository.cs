using MusicProject.Models.Concrete;

namespace MusicProject.Repositories.Interface
{
    public interface IAdminContentModerationRepository
    {
        Song? GetSongById(int songId);

        Album? GetAlbumById(int albumId);

        void SaveChanges();
    }
}