using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.UserDashboard;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;

namespace MusicProject.Services.Interface
{
    public interface IUserService
    {
        User? Authenticate(string email, string password);

        bool Register(User user);

        UserSettingsDto? GetUserSettings(int userId);

        UserSettingsResult UpdateUserSettings(int userId, UpdateUserSettingsRequest request);
    }
}
