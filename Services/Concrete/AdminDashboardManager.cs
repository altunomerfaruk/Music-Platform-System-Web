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

        public AdminArtistsViewModel GetArtists(string? search)
        {
            var artists = _adminDashboardRepository
                .GetArtists(search)
                .Select(artist => new AdminArtistListItemViewModel
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    Initial = GetInitial(artist.Name),
                    CountryName = artist.CountryEntity?.Name ?? "Belirtilmedi",
                    DebutYear = artist.DebutYear,
                    HasLinkedUser = artist.User != null,
                    LinkedUsername = artist.User?.Username ?? "Bağlı hesap yok",
                    LinkedEmail = artist.User?.Email ?? "-",
                    IsLinkedUserActive = artist.User?.IsActive ?? false,
                    AlbumCount = artist.Albums.Count,
                    SongCount = artist.SongArtists.Count,
                    FollowerCount = artist.Followers.Count
                })
                .ToList();

            var model = new AdminArtistsViewModel
            {
                SearchTerm = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                DisplayedArtists = artists.Count,
                LinkedAccounts = artists.Count(artist => artist.HasLinkedUser),
                UnlinkedAccounts = artists.Count(artist => !artist.HasLinkedUser),
                Artists = artists
            };

            FillLayoutData(model);

            return model;
        }

        public AdminAlbumsViewModel GetAlbums(string? search, PublicationStatus? status)
        {
            var albums = _adminDashboardRepository
                .GetAlbums(search, status)
                .Select(album => new AdminAlbumListItemViewModel
                {
                    Id = album.Id,
                    Name = album.Name,
                    ArtistName = album.Artist.Name,
                    CoverImageUrl = album.CoverImageUrl,
                    ReleaseDate = album.ReleaseDate,
                    CreatedAt = album.CreatedAt,
                    PublicationStatus = album.PublicationStatus,
                    ScheduledPublishAtUtc = album.ScheduledPublishAtUtc,
                    PublishedAtUtc = album.PublishedAtUtc,
                    SongCount = album.Songs.Count
                })
                .ToList();

            var model = new AdminAlbumsViewModel
            {
                SearchTerm = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                StatusFilter = status,
                DisplayedAlbums = albums.Count,
                PublishedAlbums = albums.Count(album => album.PublicationStatus == PublicationStatus.Published),
                ScheduledAlbums = albums.Count(album => album.PublicationStatus == PublicationStatus.Scheduled),
                DraftAlbums = albums.Count(album => album.PublicationStatus == PublicationStatus.Draft),
                ArchivedAlbums = albums.Count(album => album.PublicationStatus == PublicationStatus.Archived),
                Albums = albums
            };

            FillLayoutData(model);

            return model;
        }

        public AdminSongsViewModel GetSongs(string? search, PublicationStatus? status)
        {
            var songs = _adminDashboardRepository
                .GetSongs(search, status)
                .Select(song => new AdminSongListItemViewModel
                {
                    Id = song.Id,
                    Title = song.Title,
                    ArtistName = song.Album?.Artist?.Name ??
                                 song.SongArtists
                                     .Select(songArtist => songArtist.Artist.Name)
                                     .FirstOrDefault() ??
                                 "Sanatçı bilgisi yok",
                    AlbumName = song.Album?.Name ?? "Single",
                    LabelName = song.Label?.Name ?? "Bağımsız",
                    PublicationStatus = song.PublicationStatus,
                    CreatedAt = song.CreatedAt,
                    ScheduledPublishAtUtc = song.ScheduledPublishAtUtc,
                    PublishedAtUtc = song.PublishedAtUtc,
                    TotalStreams = song.SongStat?.TotalStreams ?? 0,
                    TotalLikes = song.SongStat?.TotalLikes ?? 0,
                    PopularityScore = song.SongStat?.PopularityScore ?? 0,

                    IsAdminHidden = song.IsAdminHidden,
                    AdminHiddenReason = song.AdminHiddenReason,
                    AdminHiddenAtUtc = song.AdminHiddenAtUtc,

                    IsHiddenByAlbum = song.Album?.IsAdminHidden ?? false,
                    AlbumAdminHiddenReason = song.Album?.AdminHiddenReason
                })
                .ToList();

            var model = new AdminSongsViewModel
            {
                SearchTerm = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                StatusFilter = status,
                DisplayedSongs = songs.Count,
                PublishedSongs = songs.Count(song => song.PublicationStatus == PublicationStatus.Published),
                ScheduledSongs = songs.Count(song => song.PublicationStatus == PublicationStatus.Scheduled),
                DraftSongs = songs.Count(song => song.PublicationStatus == PublicationStatus.Draft),
                ArchivedSongs = songs.Count(song => song.PublicationStatus == PublicationStatus.Archived),
                Songs = songs
            };

            FillLayoutData(model);

            return model;
        }

        private void FillLayoutData(AdminLayoutViewModel model)
        {
            model.TotalUsers = _adminDashboardRepository.GetTotalUserCount();
            model.TotalArtists = _adminDashboardRepository.GetTotalArtistCount();
            model.TotalAlbums = _adminDashboardRepository.GetTotalAlbumCount();
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

        private string GetInitial(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "?";
            }

            return char.ToUpperInvariant(value.Trim()[0]).ToString();
        }
    }
}