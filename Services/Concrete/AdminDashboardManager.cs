using MusicProject.Contracts.Responses.AdminDashboard;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class AdminDashboardManager : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _adminDashboardRepository;

        public AdminDashboardManager(IAdminDashboardRepository adminDashboardRepository)
        {
            _adminDashboardRepository = adminDashboardRepository;
        }

        public AdminLayoutTotalsDto GetLayoutTotals()
        {
            return new AdminLayoutTotalsDto
            {
                TotalUsers = _adminDashboardRepository.GetTotalUserCount(),
                TotalArtists = _adminDashboardRepository.GetTotalArtistCount(),
                TotalAlbums = _adminDashboardRepository.GetTotalAlbumCount(),
                TotalSongs = _adminDashboardRepository.GetTotalSongCount()
            };
        }

        public AdminDashboardDto GetDashboard()
        {
            var recentUsers = _adminDashboardRepository
                .GetRecentUsers(5)
                .Select(user => new AdminRecentUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive
                })
                .ToList();

            var topSongs = _adminDashboardRepository
                .GetTopSongsByStreams(5)
                .Select(song => new AdminTopSongDto
                {
                    Id = song.Id,
                    Title = song.Title,
                    ArtistName = GetPrimaryArtistName(song),
                    TotalStreams = song.SongStat?.TotalStreams ?? 0
                })
                .ToList();

            return new AdminDashboardDto
            {
                TotalListenings = _adminDashboardRepository.GetTotalListeningCount(),
                RecentUsers = recentUsers,
                TopSongs = topSongs,
                WeeklyListenings = GetWeeklyListenings()
            };
        }

        public IReadOnlyList<AdminUserListItemDto> GetUsers(string? search)
        {
            return _adminDashboardRepository
                .GetUsers(search)
                .Select(user => new AdminUserListItemDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    IsPremium = user.IsPremium ?? false,
                    CreatedAt = user.CreatedAt
                })
                .ToList();
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

        public IReadOnlyList<AdminArtistListItemDto> GetArtists(string? search)
        {
            return _adminDashboardRepository
                .GetArtists(search)
                .Select(artist => new AdminArtistListItemDto
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    CountryName = artist.CountryEntity?.Name ?? string.Empty,
                    DebutYear = artist.DebutYear,
                    HasLinkedUser = artist.User != null,
                    LinkedUsername = artist.User?.Username ?? string.Empty,
                    LinkedEmail = artist.User?.Email ?? string.Empty,
                    IsLinkedUserActive = artist.User?.IsActive ?? false,
                    AlbumCount = artist.Albums.Count,
                    SongCount = artist.SongArtists.Count,
                    FollowerCount = artist.Followers.Count
                })
                .ToList();
        }

        public IReadOnlyList<AdminAlbumListItemDto> GetAlbums(string? search, PublicationStatus? status)
        {
            return _adminDashboardRepository
                .GetAlbums(search, status)
                .Select(album => new AdminAlbumListItemDto
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
                    SongCount = album.Songs.Count,

                    IsAdminHidden = album.IsAdminHidden,
                    AdminHiddenReason = album.AdminHiddenReason,
                    AdminHiddenAtUtc = album.AdminHiddenAtUtc
                })
                .ToList();
        }

        public IReadOnlyList<AdminSongListItemDto> GetSongs(string? search, PublicationStatus? status)
        {
            return _adminDashboardRepository
                .GetSongs(search, status)
                .Select(song => new AdminSongListItemDto
                {
                    Id = song.Id,
                    Title = song.Title,
                    ArtistName = GetPrimaryArtistName(song),
                    AlbumName = song.Album?.Name ?? string.Empty,
                    LabelName = song.Label?.Name ?? string.Empty,
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
        }

        private static string GetPrimaryArtistName(Song song)
        {
            return song.Album?.Artist?.Name ??
                   song.SongArtists
                       .Select(songArtist => songArtist.Artist.Name)
                       .FirstOrDefault() ??
                   string.Empty;
        }

        private List<AdminDailyListeningDto> GetWeeklyListenings()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var startUtc = todayUtc.AddDays(-6);

            var histories = _adminDashboardRepository
                .GetListeningHistorySince(startUtc)
                .ToList();

            var countsByDate = histories
                .GroupBy(history => history.ListenedAt.Date)
                .ToDictionary(group => group.Key, group => group.Count());

            return Enumerable
                .Range(0, 7)
                .Select(dayOffset =>
                {
                    var date = startUtc.AddDays(dayOffset);

                    return new AdminDailyListeningDto
                    {
                        Date = date,
                        ListeningCount = countsByDate.GetValueOrDefault(date)
                    };
                })
                .ToList();
        }
    }
}
