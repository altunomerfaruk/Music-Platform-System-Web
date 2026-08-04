using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.ViewModels.UserDashboard;

namespace MusicProject.Services.Interface
{
    public interface IUserService
    {
        User? Authenticate(string email, string password);

        bool Register(User user);

        UserSettingsViewModel? GetUserSettings(int userId);

        UserSettingsResult UpdateUserSettings(int userId, UserSettingsViewModel model);
    }
}