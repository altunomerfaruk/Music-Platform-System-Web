using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MusicProject.Controllers.Base
{
    /// <summary>
    /// Oturum acmis kullaniciyla calisan tum dashboard controller'lari icin
    /// ortak yardimcilar. (User + Artist)
    /// </summary>
    public abstract class DashboardControllerBase : Controller
    {
        protected bool TryGetCurrentUserId(out int userId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId);
        }

        protected IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Auth");
        }
    }
}
