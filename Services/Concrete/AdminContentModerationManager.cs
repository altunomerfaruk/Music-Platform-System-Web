using MusicProject.Models.Enums;
using MusicProject.Repositories.Interface;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class AdminContentModerationManager : IAdminContentModerationService
    {
        private readonly IAdminContentModerationRepository _adminContentModerationRepository;

        public AdminContentModerationManager(IAdminContentModerationRepository adminContentModerationRepository)
        {
            _adminContentModerationRepository = adminContentModerationRepository;
        }

        public AdminContentVisibilityUpdateResult SetSongAdminHiddenStatus(int songId, bool isHidden, string? reason)
        {
            if (isHidden && string.IsNullOrWhiteSpace(reason))
            {
                return AdminContentVisibilityUpdateResult.ReasonRequired;
            }

            var song = _adminContentModerationRepository.GetSongById(songId);

            if (song == null)
            {
                return AdminContentVisibilityUpdateResult.ContentNotFound;
            }

            song.IsAdminHidden = isHidden;
            song.AdminHiddenReason = isHidden ? reason!.Trim() : null;
            song.AdminHiddenAtUtc = isHidden ? DateTime.UtcNow : null;

            _adminContentModerationRepository.SaveChanges();

            return AdminContentVisibilityUpdateResult.Success;
        }

        public AdminContentVisibilityUpdateResult SetAlbumAdminHiddenStatus(int albumId, bool isHidden, string? reason)
        {
            if (isHidden && string.IsNullOrWhiteSpace(reason))
            {
                return AdminContentVisibilityUpdateResult.ReasonRequired;
            }

            var album = _adminContentModerationRepository.GetAlbumById(albumId);

            if (album == null)
            {
                return AdminContentVisibilityUpdateResult.ContentNotFound;
            }

            album.IsAdminHidden = isHidden;
            album.AdminHiddenReason = isHidden ? reason!.Trim() : null;
            album.AdminHiddenAtUtc = isHidden ? DateTime.UtcNow : null;

            _adminContentModerationRepository.SaveChanges();

            return AdminContentVisibilityUpdateResult.Success;
        }
    }
}