using MusicProject.ViewModels.AdminDashboard;

namespace MusicProject.Services.Interface
{
    public interface IAdminDashboardService
    {
        AdminDashboardViewModel GetDashboard();
    }
}