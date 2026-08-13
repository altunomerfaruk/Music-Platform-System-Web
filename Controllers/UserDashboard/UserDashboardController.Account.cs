using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Contracts.Requests;
using MusicProject.Models.Enums;
using MusicProject.ViewModels.UserDashboard;
using System.Security.Claims;

namespace MusicProject.Controllers
{
    public partial class UserDashboardController
    {
        [HttpGet]
        public IActionResult UserSettings()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var settings = _userService.GetUserSettings(userId);

            if (settings == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var model = new UserSettingsViewModel
            {
                UserId = settings.UserId,
                Username = settings.Username,
                Email = settings.Email,
                IsPremium = settings.IsPremium,
                RoleName = settings.Role.ToString()
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserSettings(UserSettingsViewModel model)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            model.UserId = userId;

            if (!ModelState.IsValid)
            {
                FillLayoutData(model, userId);
                return View(model);
            }

            var result = _userService.UpdateUserSettings(
                userId,
                new UpdateUserSettingsRequest
                {
                    Username = model.Username,
                    Email = model.Email,
                    CurrentPassword = model.CurrentPassword,
                    NewPassword = model.NewPassword,
                    ConfirmNewPassword = model.ConfirmNewPassword
                });

            switch (result)
            {
                case UserSettingsResult.Success:
                    await RefreshUserClaimsAsync(userId, model);

                    TempData["SuccessMessage"] =
                        "Hesap bilgileriniz başarıyla güncellendi.";

                    return RedirectToAction(nameof(UserSettings));

                case UserSettingsResult.UsernameAlreadyExists:
                    ModelState.AddModelError(
                        nameof(model.Username),
                        "Bu kullanıcı adı başka bir kullanıcı tarafından kullanılıyor."
                    );
                    break;

                case UserSettingsResult.EmailAlreadyExists:
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor."
                    );
                    break;

                case UserSettingsResult.CurrentPasswordIncorrect:
                    ModelState.AddModelError(
                        nameof(model.CurrentPassword),
                        "Mevcut şifreniz yanlış."
                    );
                    break;

                case UserSettingsResult.NewPasswordRequired:
                    ModelState.AddModelError(
                        nameof(model.NewPassword),
                        "Şifre değiştirmek için yeni şifrenizi girmelisiniz."
                    );
                    break;

                case UserSettingsResult.UserNotFound:
                    return NotFound("Kullanıcı bulunamadı.");

                default:
                    ModelState.AddModelError(
                        string.Empty,
                        "Hesap bilgileri güncellenirken beklenmeyen bir hata oluştu."
                    );
                    break;
            }

            FillLayoutData(model, userId);

            return View(model);
        }

        [HttpGet]
        public IActionResult ListeningHistory()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return RedirectToLogin();
            }

            var model = new ListeningHistoryViewModel
            {
                ListeningHistory = _listeningHistoryService
                    .GetRecentListeningHistory(userId, 100),

                TotalListeningCount = _listeningHistoryService
                    .GetTotalListeningCount(userId)
            };

            FillLayoutData(model, userId);

            return View(model);
        }

        private async Task RefreshUserClaimsAsync(
            int userId,
            UserSettingsViewModel model)
        {
            var currentRole =
                User.FindFirstValue(ClaimTypes.Role) ?? "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Role, currentRole)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var claimsPrincipal =
                new ClaimsPrincipal(claimsIdentity);

            var authenticationResult =
                await HttpContext.AuthenticateAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

            var authenticationProperties =
                authenticationResult.Properties ??
                new AuthenticationProperties();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authenticationProperties
            );
        }
    }
}
