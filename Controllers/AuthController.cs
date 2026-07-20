using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Models.ViewModels;
using MusicProject.Services.Interface;
using System.Security.Claims;

namespace MusicProject.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _userService.Authenticate(
                model.Email,
                model.Password
            );

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "E-posta veya şifre hatalı."
                );

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                new Claim(ClaimTypes.Name, user.Username),

                new Claim(ClaimTypes.Email, user.Email),

                new Claim(ClaimTypes.Role, user.Role.ToString())

            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );


            switch (user.Role)
            {
                case UserRole.Admin:
                    return RedirectToAction(
                        "Index",
                        "AdminDashboard"
                    );

                case UserRole.Artist:
                    return RedirectToAction(
                        "Index",
                        "ArtistDashboard"
                    );

                case UserRole.User:
                    return RedirectToAction(
                        "Index",
                        "UserDashboard"
                    );

                default:
                    await HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme
                    );

                    ModelState.AddModelError(
                        "",
                        "Kullanıcı rolü tanımlı değil."
                    );

                    return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                Username = model.FullName,
                Email = model.Email,
                Password = model.Password,

                Role = UserRole.User
                // DEĞİŞİKLİK: Yeni kayıt olan kullanıcıların rolü
                // varsayılan olarak User yapıldı.
            };

            var registrationSuccessful = _userService.Register(user);

            if (!registrationSuccessful)
            {
                ModelState.AddModelError(
                    "",
                    "Bu e-posta adresiyle daha önce kayıt olunmuş."
                );

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Kayıt başarılı. Şimdi giriş yapabilirsiniz.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login");
        }
    }
}