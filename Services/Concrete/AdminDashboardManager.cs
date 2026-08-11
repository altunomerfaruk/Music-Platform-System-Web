using System.Globalization;
using MusicProject.Models.Enums;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;
using MusicProject.ViewModels.AdminDashboard;

namespace MusicProject.Services.Concrete
{
    public class AdminDashboardManager : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _adminDashboardRepository;

        public AdminDashboardManager(IAdminDashboardRepository adminDashboardRepository)
        {
            _adminDashboardRepository = adminDashboardRepository;
        }

        public AdminDashboardViewModel GetDashboard()
        {
            var recentUsers = _adminDashboardRepository
                .GetRecentUsers(5)
                .Select(user => new AdminRecentUserViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    RoleName = user.Role.ToString(),
                    IsActive = user.IsActive,
                    Initial = GetInitial(user.Username)
                })
                .ToList();

            var topSongs = _adminDashboardRepository
                .GetTopSongsByStreams(5)
                .Select(song => new AdminTopSongViewModel
                {
                    Id = song.Id,
                    Title = song.Title,
                    ArtistName = song.Album?.Artist?.Name ??
                                 song.SongArtists
                                     .Select(songArtist => songArtist.Artist.Name)
                                     .FirstOrDefault() ??
                                 "Sanatçı bilgisi yok",
                    TotalStreams = song.SongStat?.TotalStreams ?? 0
                })
                .ToList();

            var model = new AdminDashboardViewModel
            {
                TotalListenings = _adminDashboardRepository.GetTotalListeningCount(),
                RecentUsers = recentUsers,
                TopSongs = topSongs,
                WeeklyListenings = GetWeeklyListenings()
            };

            FillLayoutData(model);

            return model;
        }

        public AdminUsersViewModel GetUsers(string? search, int currentAdminUserId)
        {
            var users = _adminDashboardRepository
                .GetUsers(search)
                .Select(user => new AdminUserListItemViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    RoleName = user.Role.ToString(),
                    IsActive = user.IsActive,
                    IsPremium = user.IsPremium ?? false,
                    CreatedAt = user.CreatedAt,
                    Initial = GetInitial(user.Username),
                    CanChangeStatus = user.Id != currentAdminUserId
                })
                .ToList();

            var model = new AdminUsersViewModel
            {
                SearchTerm = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                DisplayedUsers = users.Count,
                ActiveUsers = users.Count(user => user.IsActive),
                InactiveUsers = users.Count(user => !user.IsActive),
                Users = users
            };

            FillLayoutData(model);

            return model;
        }

        public AdminUserStatusUpdateResult SetUserActiveStatus(int userId, int currentAdminUserId, bool isActive)
        {
            if (userId == currentAdminUserId)
            {
                return AdminUserStatusUpdateResult.CannotChangeOwnStatus;
            }

            var user = _adminDashboardRepository.GetUserById(userId);

            if (user == null)
            {
                return AdminUserStatusUpdateResult.UserNotFound;
            }

            user.IsActive = isActive;

            _adminDashboardRepository.UpdateUser(user);

            return AdminUserStatusUpdateResult.Success;
        }

        private void FillLayoutData(AdminLayoutViewModel model)
        {
            model.TotalUsers = _adminDashboardRepository.GetTotalUserCount();
            model.TotalArtists = _adminDashboardRepository.GetTotalArtistCount();
            model.TotalSongs = _adminDashboardRepository.GetTotalSongCount();
        }

        private List<AdminDailyListeningViewModel> GetWeeklyListenings()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var startUtc = todayUtc.AddDays(-6);

            var histories = _adminDashboardRepository
                .GetListeningHistorySince(startUtc)
                .ToList();

            var countsByDate = histories
                .GroupBy(history => history.ListenedAt.Date)
                .ToDictionary(group => group.Key, group => group.Count());

            var dailyCounts = Enumerable
                .Range(0, 7)
                .Select(dayOffset =>
                {
                    var date = startUtc.AddDays(dayOffset);

                    return new
                    {
                        Date = date,
                        Count = countsByDate.GetValueOrDefault(date)
                    };
                })
                .ToList();

            var maximumListeningCount = dailyCounts.Max(day => day.Count);
            var turkishCulture = CultureInfo.GetCultureInfo("tr-TR");

            return dailyCounts
                .Select(day => new AdminDailyListeningViewModel
                {
                    DayLabel = day.Date.ToString("ddd", turkishCulture),
                    ListeningCount = day.Count,
                    BarHeightPercent = CalculateBarHeight(day.Count, maximumListeningCount)
                })
                .ToList();
        }

        private int CalculateBarHeight(int listeningCount, int maximumListeningCount)
        {
            if (listeningCount == 0 || maximumListeningCount == 0)
            {
                return 0;
            }

            var percentage = (int)Math.Round(listeningCount * 100d / maximumListeningCount);

            return Math.Max(8, percentage);
        }

        private string GetInitial(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return "?";
            }

            return char
                .ToUpperInvariant(username.Trim()[0])
                .ToString();
        }
    }
}