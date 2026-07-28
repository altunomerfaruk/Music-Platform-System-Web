using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Models.ViewModels;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class UserManager : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserManager(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? Authenticate(string email, string password)
        {
            var normalizedEmail = email.Trim();

            var user = _userRepository.GetByEmail(normalizedEmail);

            if (user != null &&
                user.IsActive &&
                user.Password == password)
            {
                return user;
            }

            return null;
        }

        public bool Register(User user)
        {
            user.Username = user.Username.Trim();
            user.Email = user.Email.Trim();

            var existingEmail = _userRepository.GetByEmail(user.Email);

            if (existingEmail != null)
            {
                return false;
            }

            var existingUsername = _userRepository.GetByUsername(user.Username);

            if (existingUsername != null)
            {
                return false;
            }

            _userRepository.Create(user);

            return true;
        }

        public UserSettingsViewModel? GetUserSettings(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                return null;
            }

            return new UserSettingsViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsPremium = user.IsPremium ?? false,
                RoleName = user.Role.ToString()
            };
        }

        public UserSettingsResult UpdateUserSettings(int userId, UserSettingsViewModel model)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                return UserSettingsResult.UserNotFound;
            }

            var normalizedUsername = model.Username.Trim();
            var normalizedEmail = model.Email.Trim();

            var usernameExists = _userRepository.UsernameExists(
                normalizedUsername,
                userId
            );

            if (usernameExists)
            {
                return UserSettingsResult.UsernameAlreadyExists;
            }

            var emailExists = _userRepository.EmailExists(
                normalizedEmail,
                userId
            );

            if (emailExists)
            {
                return UserSettingsResult.EmailAlreadyExists;
            }

            var wantsToChangePassword =
                !string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                !string.IsNullOrWhiteSpace(model.NewPassword) ||
                !string.IsNullOrWhiteSpace(model.ConfirmNewPassword);

            if (wantsToChangePassword)
            {

                if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                    user.Password != model.CurrentPassword)
                {
                    return UserSettingsResult.CurrentPasswordIncorrect;
                }

                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    return UserSettingsResult.NewPasswordRequired;
                }

                user.Password = model.NewPassword;
            }

            user.Username = normalizedUsername;
            user.Email = normalizedEmail;

            _userRepository.Update(user);

            return UserSettingsResult.Success;
        }
    }
}