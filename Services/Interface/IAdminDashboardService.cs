using MusicProject.Contracts.Responses.AdminDashboard;
using MusicProject.Models.Enums;

namespace MusicProject.Services.Interface
{
    public interface IAdminDashboardService
    {
        AdminLayoutTotalsDto GetLayoutTotals();

        AdminDashboardDto GetDashboard();

        IReadOnlyList<AdminUserListItemDto> GetUsers(string? search);

        AdminUserStatusUpdateResult SetUserActiveStatus(int userId, int currentAdminUserId, bool isActive);

        IReadOnlyList<AdminArtistListItemDto> GetArtists(string? search);

        IReadOnlyList<AdminAlbumListItemDto> GetAlbums(string? search, PublicationStatus? status);

        IReadOnlyList<AdminSongListItemDto> GetSongs(string? search, PublicationStatus? status);
    }
}
