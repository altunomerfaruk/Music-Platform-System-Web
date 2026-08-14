using Microsoft.AspNetCore.Mvc;
using MusicProject.Models.Enums;
using MusicProject.ViewModels.AdminDashboard;

namespace MusicProject.Controllers
{
    public partial class AdminDashboardController
    {
        [HttpGet]
        public IActionResult Users(string? search)
        {
            if (!TryGetCurrentUserId(out var currentAdminUserId))
            {
                return Forbid();
            }

            var users = _adminDashboardService.GetUsers(search).ToList();

            foreach (var user in users)
            {
                user.RoleName = user.Role.ToString();
                user.Initial = GetInitial(user.Username);

                user.CanChangeStatus = user.Id != currentAdminUserId;
            }

            var model = new AdminUsersViewModel
            {
                SearchTerm = NormalizeSearch(search),
                DisplayedUsers = users.Count,
                ActiveUsers = users.Count(user => user.IsActive),
                InactiveUsers = users.Count(user => !user.IsActive),
                Users = users
            };

            FillLayoutData(model);

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

            var result = _adminDashboardService.SetUserActiveStatus(
                userId,
                currentAdminUserId,
                isActive);

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
    }
}
