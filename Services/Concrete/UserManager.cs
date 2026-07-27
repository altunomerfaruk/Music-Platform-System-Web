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

            // DEĞİŞİKLİK:
            // Pasif kullanıcıların sisteme giriş yapması engellendi.
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
            // DEĞİŞİKLİK:
            // Kullanıcı adı ve e-postadaki gereksiz boşluklar temizleniyor.
            user.Username = user.Username.Trim();
            user.Email = user.Email.Trim();

            var existingEmail = _userRepository.GetByEmail(user.Email);

            if (existingEmail != null)
            {
                return false;
            }

            // DEĞİŞİKLİK:
            // Önceden yalnızca e-posta kontrol ediliyordu.
            // Artık aynı kullanıcı adıyla ikinci hesap da oluşturulamaz.
            var existingUsername = _userRepository.GetByUsername(user.Username);

            if (existingUsername != null)
            {
                return false;
            }

            _userRepository.Create(user);

            return true;
        }

        // DEĞİŞİKLİK:
        // Entity doğrudan View'a gönderilmez.
        // Yalnızca ayarlar ekranında kullanılacak alanlar ViewModel'e aktarılır.
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

            // DEĞİŞİKLİK:
            // Kullanıcı kendi mevcut kullanıcı adını kullanabilir.
            // Yalnızca başka bir kullanıcıda varsa hata döner.
            var usernameExists = _userRepository.UsernameExists(
                normalizedUsername,
                userId
            );

            if (usernameExists)
            {
                return UserSettingsResult.UsernameAlreadyExists;
            }

            // DEĞİŞİKLİK:
            // Kullanıcı kendi mevcut e-postasını kullanabilir.
            // Yalnızca başka bir kullanıcıda varsa hata döner.
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
                // DEĞİŞİKLİK:
                // Şifre alanlarından herhangi biri doldurulduysa
                // mevcut şifrenin de girilmesi zorunludur.
                if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                    user.Password != model.CurrentPassword)
                {
                    return UserSettingsResult.CurrentPasswordIncorrect;
                }

                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    return UserSettingsResult.NewPasswordRequired;
                }

                // ConfirmNewPassword eşleşme kontrolü ViewModel üzerindeki
                // Compare doğrulaması tarafından yapılır.
                user.Password = model.NewPassword;
            }

            user.Username = normalizedUsername;
            user.Email = normalizedEmail;

            _userRepository.Update(user);

            return UserSettingsResult.Success;
        }
    }
}