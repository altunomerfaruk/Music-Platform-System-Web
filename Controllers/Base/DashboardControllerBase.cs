using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MusicProject.Controllers.Base
{
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
