using MusicProject.Models.Enums;

namespace MusicProject.Services.Interface
{
    public interface IAdminContentModerationService
    {
        AdminContentVisibilityUpdateResult SetSongAdminHiddenStatus(int songId, bool isHidden, string? reason);

        AdminContentVisibilityUpdateResult SetAlbumAdminHiddenStatus(int albumId, bool isHidden, string? reason);
    }
}