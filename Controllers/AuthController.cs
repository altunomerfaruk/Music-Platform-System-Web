using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.Concrete;
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
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(
                    ClaimTypes.Role,
                    user.IsAdmin ? "Admin" : "User"
                )
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

            return RedirectToAction("Index", "Artist");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // DEĞİŞİKLİK: Register butonuna basılınca çalışacak POST metodu eklendi.
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                Username = model.FullName,
                // DEĞİŞİKLİK: FullName bilgisi User modelindeki Username alanına aktarılıyor.

                Email = model.Email,
                Password = model.Password,
                IsAdmin = false
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