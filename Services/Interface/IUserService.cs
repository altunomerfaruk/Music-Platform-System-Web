using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Models.ViewModels;

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