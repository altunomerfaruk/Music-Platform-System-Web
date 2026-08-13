using Microsoft.AspNetCore.Identity;
using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.UserDashboard;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class UserManager : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserManager(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public User? Authenticate(string email, string password)
        {
            var normalizedEmail = email.Trim();

            var user = _userRepository.GetByEmail(normalizedEmail);

            if (user == null || !user.IsActive)
            {
                return null;
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Password = _passwordHasher.HashPassword(user, password);
                _userRepository.Update(user);
            }

            return user;
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

            user.Password = _passwordHasher.HashPassword(user, user.Password);

            _userRepository.Create(user);

            return true;
        }

        public UserSettingsDto? GetUserSettings(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                return null;
            }

            return new UserSettingsDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsPremium = user.IsPremium ?? false,
                Role = user.Role
            };
        }

        public UserSettingsResult UpdateUserSettings(int userId, UpdateUserSettingsRequest request)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
            {
                return UserSettingsResult.UserNotFound;
            }

            var normalizedUsername = request.Username.Trim();
            var normalizedEmail = request.Email.Trim();

            var usernameExists = _userRepository.UsernameExists(normalizedUsername, userId);

            if (usernameExists)
            {
                return UserSettingsResult.UsernameAlreadyExists;
            }

            var emailExists = _userRepository.EmailExists(normalizedEmail, userId);

            if (emailExists)
            {
                return UserSettingsResult.EmailAlreadyExists;
            }

            var wantsToChangePassword =
                !string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                !string.IsNullOrWhiteSpace(request.NewPassword) ||
                !string.IsNullOrWhiteSpace(request.ConfirmNewPassword);

            if (wantsToChangePassword)
            {
                if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                {
                    return UserSettingsResult.CurrentPasswordIncorrect;
                }

                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, request.CurrentPassword);

                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    return UserSettingsResult.CurrentPasswordIncorrect;
                }

                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return UserSettingsResult.NewPasswordRequired;
                }

                user.Password = _passwordHasher.HashPassword(user, request.NewPassword);
            }

            user.Username = normalizedUsername;
            user.Email = normalizedEmail;

            _userRepository.Update(user);

            return UserSettingsResult.Success;
        }
    }
}