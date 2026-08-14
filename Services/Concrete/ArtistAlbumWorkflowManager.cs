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
        private readonly ILogger<ArtistAlbumWorkflowManager> _logger;

        public ArtistAlbumWorkflowManager(
            IAlbumService albumService,
            IPublicationService publicationService,
            IPublicationJobScheduler publicationJobScheduler,
            ILogger<ArtistAlbumWorkflowManager> logger)
        {
            _albumService = albumService;
            _publicationService = publicationService;
            _publicationJobScheduler = publicationJobScheduler;
            _logger = logger;
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
            }
            catch (InvalidOperationException exception)
            {
                // Henuz provisional kaynak (Hangfire job) uretilmedi; yalnizca isim/
                // dogrulama hatasi. Geri alinacak bir sey yok.
                return ArtistAlbumWorkflowResult.Failure(
                    ArtistAlbumWorkflowField.Name,
                    exception.Message);
            }

            // Album DB'ye yazildi. Scheduled ise Hangfire job'i "provisional" bir kaynaktir:
            // job olusturulduktan sonra publication DB update'i patlarsa, Song create
            // akisindaki gibi yeni job iptal edilir ve yarim kalan album kaydi geri alinir;
            // asil hata korunur, cleanup hatalari loglanir.
            if (album.PublicationStatus == PublicationStatus.Scheduled &&
                album.ScheduledPublishAtUtc.HasValue)
            {
                string? provisionalJobId = null;

                try
                {
                    provisionalJobId =
                        _publicationJobScheduler.ScheduleAlbumPublication(
                            album.Id,
                            album.ScheduledPublishAtUtc.Value);

                    album.PublicationJobId = provisionalJobId;

                    _albumService.UpdatePublication(album);
                }
                catch (InvalidOperationException exception)
                {
                    DiscardCreatedAlbum(album.Id, request.ArtistId, provisionalJobId);

                    return ArtistAlbumWorkflowResult.Failure(
                        ArtistAlbumWorkflowField.Name,
                        exception.Message);
                }
                catch
                {
                    DiscardCreatedAlbum(album.Id, request.ArtistId, provisionalJobId);

                    throw;
                }
            }

            return ArtistAlbumWorkflowResult.Success(BuildCreationMessage(album));
        }

        /// <summary>
        /// Olusturma, album DB'ye yazildiktan SONRAKI bir adimda (job planlama /
        /// publication update) basarisiz oldugunda geride yarim kayit birakmamak icin
        /// bu istekte uretilen provisional job'i iptal eder ve album kaydini geri alir.
        /// Buradaki ikincil hatalar yalnizca loglanir; cagiran asil hatayi dondurmeye
        /// devam eder.
        /// </summary>
        private void DiscardCreatedAlbum(
            int albumId,
            int artistId,
            string? provisionalJobId)
        {
            TryCancelPublicationJob(
                provisionalJobId,
                albumId,
                "album olusturma geri alinirken yeni yayin job'i iptal edilemedi");

            try
            {
                _albumService.DeleteArtistAlbum(albumId, artistId);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Albüm {AlbumId}: olusturma geri alinirken kayit silinemedi.",
                    albumId);
            }
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

            bool updated;

            try
            {
                updated = _albumService.UpdateArtistAlbum(updateRequest);
            }
            catch (InvalidOperationException exception)
            {
                DiscardProvisionalPublicationJob(request.AlbumId, newPublicationJobId);

                return ArtistAlbumWorkflowResult.Failure(
                    ArtistAlbumWorkflowField.Name,
                    exception.Message);
            }
            catch
            {
                DiscardProvisionalPublicationJob(request.AlbumId, newPublicationJobId);

                throw;
            }

            if (!updated)
            {
                DiscardProvisionalPublicationJob(request.AlbumId, newPublicationJobId);

                return ArtistAlbumWorkflowResult.Failure(
                    ArtistAlbumWorkflowField.None,
                    AlbumNotFoundMessage);
            }

            CleanUpReplacedPublicationJob(
                request.AlbumId,
                oldPublicationJobId,
                newPublicationJobId);

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

        /// <summary>
        /// Album DB yazmasi BASARISIZ oldugunda calisir.
        /// Yalnizca bu istekte planlanan yeni job geri alinir; eski job korunur.
        /// </summary>
        private void DiscardProvisionalPublicationJob(
            int albumId,
            string? newPublicationJobId)
        {
            TryCancelPublicationJob(
                newPublicationJobId,
                albumId,
                "guncelleme geri alinirken yeni yayin job'i iptal edilemedi");
        }

        /// <summary>
        /// Album DB yazmasi BASARIYLA tamamlandiktan sonra calisir.
        /// Yerini yeni job'in aldigi eski job iptal edilir.
        /// Buradaki hata guncellemeyi gecersiz kilmaz: yeni job korunur,
        /// kullaniciya basarili sonuc doner, hata loglanir.
        /// </summary>
        private void CleanUpReplacedPublicationJob(
            int albumId,
            string? oldPublicationJobId,
            string? newPublicationJobId)
        {
            if (string.IsNullOrWhiteSpace(oldPublicationJobId) ||
                oldPublicationJobId == newPublicationJobId)
            {
                return;
            }

            TryCancelPublicationJob(
                oldPublicationJobId,
                albumId,
                "guncelleme sonrasi eski yayin job'i iptal edilemedi");
        }

        private void TryCancelPublicationJob(
            string? jobId,
            int albumId,
            string failureDescription)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return;
            }

            try
            {
                _publicationJobScheduler.Cancel(jobId);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Albüm {AlbumId}: {FailureDescription} (job {JobId}).",
                    albumId,
                    failureDescription,
                    jobId);
            }
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
