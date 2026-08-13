using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.ArtistDashboard;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class ArtistAlbumWorkflowManager : IArtistAlbumWorkflowService
    {
        private const string AlbumNotFoundMessage =
            "Albüm bulunamadı veya bu albüm üzerinde yetkiniz yok.";

        private readonly IAlbumService _albumService;
        private readonly IPublicationService _publicationService;
        private readonly IPublicationJobScheduler _publicationJobScheduler;

        public ArtistAlbumWorkflowManager(
            IAlbumService albumService,
            IPublicationService publicationService,
            IPublicationJobScheduler publicationJobScheduler)
        {
            _albumService = albumService;
            _publicationService = publicationService;
            _publicationJobScheduler = publicationJobScheduler;
        }

        public ArtistAlbumWorkflowResult CreateAlbum(CreateArtistAlbumRequest request)
        {
            if (!TryConvertSchedule(
                    request.RequestedStatus,
                    request.ScheduledPublishAtLocal,
                    out var scheduledPublishAtUtc,
                    out var scheduleError))
            {
                return scheduleError!;
            }

            var album = new Album
            {
                Name = request.Name.Trim(),
                Description = Normalize(request.Description),
                CoverImageUrl = Normalize(request.CoverImageUrl),
                ReleaseDate = request.ReleaseDate,
                PublicationStatus = request.RequestedStatus,
                ScheduledPublishAtUtc = scheduledPublishAtUtc,
                PublishedAtUtc = null,
                ArtistId = request.ArtistId
            };

            try
            {
                _albumService.AddAlbum(album);

                if (album.PublicationStatus == PublicationStatus.Scheduled &&
                    album.ScheduledPublishAtUtc.HasValue)
                {
                    album.PublicationJobId =
                        _publicationJobScheduler.ScheduleAlbumPublication(
                            album.Id,
                            album.ScheduledPublishAtUtc.Value);

                    _albumService.UpdatePublication(album);
                }
            }
            catch (InvalidOperationException exception)
            {
                return ArtistAlbumWorkflowResult.Failure(
                    ArtistAlbumWorkflowField.Name,
                    exception.Message);
            }

            return ArtistAlbumWorkflowResult.Success(BuildCreationMessage(album));
        }

        public ArtistAlbumWorkflowResult UpdateAlbum(UpdateArtistAlbumRequest request)
        {
            var existingAlbum = _albumService.GetArtistAlbumDetails(
                request.AlbumId,
                request.ArtistId);

            if (existingAlbum == null)
            {
                return ArtistAlbumWorkflowResult.Failure(
                    ArtistAlbumWorkflowField.None,
                    AlbumNotFoundMessage);
            }

            if (!TryConvertSchedule(
                    request.RequestedStatus,
                    request.ScheduledPublishAtLocal,
                    out var scheduledPublishAtUtc,
                    out var scheduleError))
            {
                return scheduleError!;
            }

            var oldPublicationJobId = existingAlbum.PublicationJobId;

            string? newPublicationJobId = null;

            if (request.RequestedStatus == PublicationStatus.Scheduled &&
                scheduledPublishAtUtc.HasValue)
            {
                newPublicationJobId =
                    _publicationJobScheduler.ScheduleAlbumPublication(
                        request.AlbumId,
                        scheduledPublishAtUtc.Value);
            }

            var updateRequest = new UpdateAlbumRequest
            {
                AlbumId = request.AlbumId,
                ArtistId = request.ArtistId,
                Name = request.Name,
                Description = request.Description,
                CoverImageUrl = request.CoverImageUrl,
                ReleaseDate = request.ReleaseDate,
                PublicationStatus = request.RequestedStatus,
                ScheduledPublishAtUtc = scheduledPublishAtUtc,
                PublishedAtUtc = existingAlbum.PublishedAtUtc,
                PublicationJobId = newPublicationJobId
            };

            try
            {
                var updated = _albumService.UpdateArtistAlbum(updateRequest);

                if (!updated)
                {
                    _publicationJobScheduler.Cancel(newPublicationJobId);

                    return ArtistAlbumWorkflowResult.Failure(
                        ArtistAlbumWorkflowField.None,
                        AlbumNotFoundMessage);
                }

                if (!string.IsNullOrWhiteSpace(oldPublicationJobId) &&
                    oldPublicationJobId != newPublicationJobId)
                {
                    _publicationJobScheduler.Cancel(oldPublicationJobId);
                }
            }
            catch (InvalidOperationException exception)
            {
                _publicationJobScheduler.Cancel(newPublicationJobId);

                return ArtistAlbumWorkflowResult.Failure(
                    ArtistAlbumWorkflowField.Name,
                    exception.Message);
            }
            catch
            {
                _publicationJobScheduler.Cancel(newPublicationJobId);

                throw;
            }

            return ArtistAlbumWorkflowResult.Success(BuildUpdateMessage(updateRequest));
        }

        public ArtistAlbumWorkflowResult DeleteAlbum(int albumId, int artistId)
        {
            var album = _albumService.GetArtistAlbumDetails(albumId, artistId);

            if (album == null)
            {
                return ArtistAlbumWorkflowResult.Failure(
                    ArtistAlbumWorkflowField.None,
                    "Albüm bulunamadı veya bu albümü silme yetkiniz yok.");
            }

            var publicationJobId = album.PublicationJobId;

            try
            {
                var deleted = _albumService.DeleteArtistAlbum(albumId, artistId);

                if (!deleted)
                {
                    return ArtistAlbumWorkflowResult.Failure(
                        ArtistAlbumWorkflowField.None,
                        "Albüm bulunamadı veya bu albümü silme yetkiniz yok.");
                }

                _publicationJobScheduler.Cancel(publicationJobId);
            }
            catch (InvalidOperationException exception)
            {
                return ArtistAlbumWorkflowResult.Failure(
                    ArtistAlbumWorkflowField.None,
                    exception.Message);
            }

            return ArtistAlbumWorkflowResult.Success("Albüm başarıyla silindi.");
        }

        private bool TryConvertSchedule(
            PublicationStatus requestedStatus,
            DateTime? scheduledPublishAtLocal,
            out DateTime? scheduledPublishAtUtc,
            out ArtistAlbumWorkflowResult? error)
        {
            error = null;

            try
            {
                scheduledPublishAtUtc = _publicationService.ValidateAndConvertToUtc(
                    requestedStatus,
                    scheduledPublishAtLocal);

                return true;
            }
            catch (InvalidOperationException exception)
            {
                scheduledPublishAtUtc = null;

                error = ArtistAlbumWorkflowResult.Failure(
                    ArtistAlbumWorkflowField.ScheduledPublishAt,
                    exception.Message);

                return false;
            }
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private string BuildCreationMessage(Album album)
        {
            return album.PublicationStatus switch
            {
                PublicationStatus.Draft =>
                    $"'{album.Name}' albümü taslak olarak kaydedildi.",

                PublicationStatus.Scheduled when album.ScheduledPublishAtUtc.HasValue =>
                    $"'{album.Name}' albümü " +
                    $"{_publicationService.ConvertUtcToTurkeyTime(album.ScheduledPublishAtUtc.Value):dd.MM.yyyy HH:mm} " +
                    "tarihine planlandı.",

                PublicationStatus.Published =>
                    $"'{album.Name}' albümü yayınlandı.",

                _ =>
                    $"'{album.Name}' albümü başarıyla oluşturuldu."
            };
        }

        private string BuildUpdateMessage(UpdateAlbumRequest request)
        {
            return request.PublicationStatus switch
            {
                PublicationStatus.Draft =>
                    $"'{request.Name.Trim()}' albümü taslak olarak güncellendi.",

                PublicationStatus.Scheduled when request.ScheduledPublishAtUtc.HasValue =>
                    $"'{request.Name.Trim()}' albümü " +
                    $"{_publicationService.ConvertUtcToTurkeyTime(request.ScheduledPublishAtUtc.Value):dd.MM.yyyy HH:mm} " +
                    "tarihine planlandı.",

                PublicationStatus.Published =>
                    $"'{request.Name.Trim()}' albümü yayınlandı.",

                PublicationStatus.Archived =>
                    $"'{request.Name.Trim()}' albümü arşivlendi.",

                _ =>
                    $"'{request.Name.Trim()}' albümü başarıyla güncellendi."
            };
        }
    }
}
