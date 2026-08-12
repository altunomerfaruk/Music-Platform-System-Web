using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.Enums;
using MusicProject.Services.Interface;
using System.Security.Claims;

namespace MusicProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = _adminDashboardService.GetDashboard();

            return View(model);
        }

        [HttpGet]
        public IActionResult Users(string? search)
        {
            if (!TryGetCurrentUserId(out var currentAdminUserId))
            {
                return Forbid();
            }

            var model = _adminDashboardService.GetUsers(search, currentAdminUserId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetUserActiveStatus(int userId, bool isActive, string? search)
        {
            if (!TryGetCurrentUserId(out var currentAdminUserId))
            {
                return Forbid();
            }

            var result = _adminDashboardService.SetUserActiveStatus(userId, currentAdminUserId, isActive);

            if (result == AdminUserStatusUpdateResult.UserNotFound)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
            }
            else if (result == AdminUserStatusUpdateResult.CannotChangeOwnStatus)
            {
                TempData["ErrorMessage"] = "Kendi admin hesabının aktiflik durumunu değiştiremezsin.";
            }
            else
            {
                TempData["SuccessMessage"] = isActive
                    ? "Kullanıcı hesabı aktif hale getirildi."
                    : "Kullanıcı hesabı pasif hale getirildi.";
            }

            return RedirectToAction(nameof(Users), new { search });
        }
        [HttpGet]
        public IActionResult Songs(string? search, PublicationStatus? status)
        {
            var model = _adminDashboardService.GetSongs(search, status);

            return View(model);
        }

        [HttpGet]
        public IActionResult Artists(string? search)
        {
            var model = _adminDashboardService.GetArtists(search);

            return View(model);
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out userId);
        }
    }
}