using MusicProject.Models.Enums;
using MusicProject.ViewModels.AdminDashboard;

namespace MusicProject.Services.Interface
{
    public interface IAdminDashboardService
    {
        AdminDashboardViewModel GetDashboard();

        AdminUsersViewModel GetUsers(string? search, int currentAdminUserId);

        AdminUserStatusUpdateResult SetUserActiveStatus(int userId, int currentAdminUserId, bool isActive);

        AdminArtistsViewModel GetArtists(string? search);

        AdminSongsViewModel GetSongs(string? search, PublicationStatus? status);
    }
}