using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.ArtistDashboard;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    /// <summary>
    /// Sarki ekleme / guncelleme / silme is akisi.
    ///
    /// Akis iki asamali dusunulur:
    /// 1) DB yazmasindan ONCE ayrilan mp3 ve Hangfire job'i "provisional" kaynaktir;
    ///    DB yazmasi patlarsa bunlar geri alinir, eski kaynaklara dokunulmaz.
    /// 2) DB yazmasi BASARILI olduktan sonra yerini yeni kaynaklarin aldigi eski
    ///    mp3/job temizlenir. Bu temizlik post-success cleanup'tir: patlarsa
    ///    loglanir, islem basarisiz sayilmaz ve yeni kaynaklar geri alinmaz.
    /// </summary>
    public class ArtistSongWorkflowManager : IArtistSongWorkflowService
    {
        private readonly ISongService _songService;
        private readonly IAlbumService _albumService;
        private readonly IPublicationService _publicationService;
        private readonly IPublicationJobScheduler _publicationJobScheduler;
        private readonly IAudioStorageService _audioStorageService;
        private readonly ILogger<ArtistSongWorkflowManager> _logger;

        public ArtistSongWorkflowManager(
            ISongService songService,
            IAlbumService albumService,
            IPublicationService publicationService,
            IPublicationJobScheduler publicationJobScheduler,
            IAudioStorageService audioStorageService,
            ILogger<ArtistSongWorkflowManager> logger)
        {
            _songService = songService;
            _albumService = albumService;
            _publicationService = publicationService;
            _publicationJobScheduler = publicationJobScheduler;
            _audioStorageService = audioStorageService;
            _logger = logger;
        }

        public async Task<ArtistSongWorkflowResult> CreateSongAsync(
            CreateArtistSongRequest request)
        {
            var selectedAlbum = FindArtistAlbum(request.AlbumId, request.ArtistId);

            var albumOwnershipError = ValidateAlbumOwnership(request.AlbumId, selectedAlbum);

            if (albumOwnershipError != null)
            {
                return albumOwnershipError;
            }

            if (!TryResolvePublication(
                    selectedAlbum,
                    request.RequestedStatus,
                    request.ScheduledPublishAtLocal,
                    currentPublishedAtUtc: null,
                    out var publication,
                    out var publicationError))
            {
                return publicationError!;
            }

            string storedAudioFileName;

            try
            {
                storedAudioFileName =
                    await _audioStorageService.SaveMp3Async(request.AudioFile);
            }
            catch (InvalidOperationException exception)
            {
                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.AudioFile,
                    exception.Message);
            }

            var song = new Song
            {
                Title = request.Title.Trim(),
                AudioFileName = storedAudioFileName,
                AlbumId = request.AlbumId,
                LabelId = request.LabelId,
                PublicationStatus = publication.Status,
                ScheduledPublishAtUtc = publication.ScheduledPublishAtUtc,
                PublishedAtUtc = publication.PublishedAtUtc
            };

            var songCreated = false;
            string? scheduledJobId = null;

            try
            {
                _songService.AddSongWithRelations(
                    song,
                    request.ArtistId,
                    request.GenreIds);

                songCreated = true;

                if (song.AlbumId == null &&
                    song.PublicationStatus == PublicationStatus.Scheduled &&
                    song.ScheduledPublishAtUtc.HasValue)
                {
                    scheduledJobId =
                        _publicationJobScheduler.ScheduleSongPublication(
                            song.Id,
                            song.ScheduledPublishAtUtc.Value);

                    song.PublicationJobId = scheduledJobId;

                    _songService.UpdatePublication(song);
                }
            }
            catch (InvalidOperationException exception)
            {
                DiscardCreatedSong(
                    song,
                    songCreated,
                    request.ArtistId,
                    scheduledJobId,
                    storedAudioFileName);

                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.Title,
                    exception.Message);
            }
            catch
            {
                DiscardCreatedSong(
                    song,
                    songCreated,
                    request.ArtistId,
                    scheduledJobId,
                    storedAudioFileName);

                throw;
            }

            return ArtistSongWorkflowResult.Success(
                BuildCreationMessage(song),
                song.AlbumId);
        }

        public async Task<ArtistSongWorkflowResult> UpdateSongAsync(
            UpdateArtistSongRequest request)
        {
            var existingSong = _songService.GetArtistSongForEdit(
                request.SongId,
                request.ArtistId);

            if (existingSong == null)
            {
                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.None,
                    "Şarkı bulunamadı veya bu şarkıyı düzenleme yetkiniz yok.");
            }

            var selectedAlbum = FindArtistAlbum(request.AlbumId, request.ArtistId);

            var albumOwnershipError = ValidateAlbumOwnership(request.AlbumId, selectedAlbum);

            if (albumOwnershipError != null)
            {
                return albumOwnershipError;
            }

            if (existingSong.Album?.IsAdminHidden == true &&
                request.AlbumId != existingSong.AlbumId)
            {
                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.AlbumId,
                    "Admin tarafından gizlenen albüme bağlı bir şarkının albüm bağlantısını değiştiremezsiniz.");
            }

            if (!TryResolvePublication(
                    selectedAlbum,
                    request.RequestedStatus,
                    request.ScheduledPublishAtLocal,
                    existingSong.PublishedAtUtc,
                    out var publication,
                    out var publicationError))
            {
                return publicationError!;
            }

            var oldPublicationJobId = existingSong.PublicationJobId;
            var oldAudioFileName = existingSong.AudioFileName;
            var newAudioFileName = oldAudioFileName;

            if (request.AudioFile != null && request.AudioFile.Length > 0)
            {
                try
                {
                    newAudioFileName =
                        await _audioStorageService.SaveMp3Async(request.AudioFile);
                }
                catch (InvalidOperationException exception)
                {
                    return ArtistSongWorkflowResult.Failure(
                        ArtistSongWorkflowField.AudioFile,
                        exception.Message);
                }
            }

            var audioFileChanged = !string.Equals(
                oldAudioFileName,
                newAudioFileName,
                StringComparison.Ordinal);

            string? newPublicationJobId = null;

            try
            {
                if (selectedAlbum == null &&
                    publication.Status == PublicationStatus.Scheduled &&
                    publication.ScheduledPublishAtUtc.HasValue)
                {
                    newPublicationJobId =
                        _publicationJobScheduler.ScheduleSongPublication(
                            request.SongId,
                            publication.ScheduledPublishAtUtc.Value);
                }

                var song = new Song
                {
                    Id = request.SongId,
                    Title = request.Title,
                    AudioFileName = newAudioFileName,
                    AlbumId = request.AlbumId,
                    LabelId = request.LabelId,
                    PublicationStatus = publication.Status,
                    ScheduledPublishAtUtc = publication.ScheduledPublishAtUtc,
                    PublishedAtUtc = publication.PublishedAtUtc,
                    PublicationJobId = newPublicationJobId
                };

                _songService.UpdateArtistSong(
                    song,
                    request.ArtistId,
                    request.GenreIds);
            }
            catch (InvalidOperationException exception)
            {
                DiscardProvisionalUpdateResources(
                    request.SongId,
                    newPublicationJobId,
                    newAudioFileName,
                    audioFileChanged);

                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.Title,
                    exception.Message);
            }
            catch
            {
                DiscardProvisionalUpdateResources(
                    request.SongId,
                    newPublicationJobId,
                    newAudioFileName,
                    audioFileChanged);

                throw;
            }

            CleanUpReplacedUpdateResources(
                request.SongId,
                oldPublicationJobId,
                newPublicationJobId,
                oldAudioFileName,
                audioFileChanged);

            return ArtistSongWorkflowResult.Success(
                BuildUpdateMessage(
                    request.Title,
                    publication.Status,
                    publication.ScheduledPublishAtUtc));
        }

        public ArtistSongWorkflowResult DeleteSong(int songId, int artistId)
        {
            var song = _songService.GetArtistSongForEdit(songId, artistId);

            if (song == null)
            {
                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.None,
                    "Şarkı bulunamadı veya bu şarkıyı silme yetkiniz yok.");
            }

            var publicationJobId = song.PublicationJobId;
            var audioFileName = song.AudioFileName;

            try
            {
                _songService.DeleteArtistSong(songId, artistId);
            }
            catch (InvalidOperationException exception)
            {
                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.None,
                    exception.Message);
            }

            TryCancelPublicationJob(publicationJobId, songId, "silme sonrasi yayin job'i iptal edilemedi");
            TryDeleteAudioFile(audioFileName, songId, "silme sonrasi mp3 silinemedi");

            return ArtistSongWorkflowResult.Success("Şarkı başarıyla silindi.");
        }

        private Album? FindArtistAlbum(int? albumId, int artistId)
        {
            if (!albumId.HasValue)
            {
                return null;
            }

            return _albumService.GetArtistAlbumDetails(albumId.Value, artistId);
        }

        /// <summary>
        /// Istekte album secilmis ama album bu sanatciya ait degilse hata dondurur.
        /// Controller da ayni kontrolu yapiyor; bu workflow seviyesindeki savunmadir.
        /// Kontrol olmasaydi sarki, sahibi olmadigi bir albumun id'siyle kaydedilebilirdi.
        /// </summary>
        private static ArtistSongWorkflowResult? ValidateAlbumOwnership(
            int? requestedAlbumId,
            Album? selectedAlbum)
        {
            if (requestedAlbumId.HasValue && selectedAlbum == null)
            {
                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.AlbumId,
                    "Seçilen albüm bu sanatçı hesabına ait değil.");
            }

            return null;
        }

        private bool TryResolvePublication(
            Album? selectedAlbum,
            PublicationStatus requestedStatus,
            DateTime? scheduledPublishAtLocal,
            DateTime? currentPublishedAtUtc,
            out SongPublicationPlan publication,
            out ArtistSongWorkflowResult? error)
        {
            error = null;

            var publishedAtUtc = currentPublishedAtUtc;

            if (selectedAlbum != null)
            {
                var status = selectedAlbum.PublicationStatus == PublicationStatus.Published
                    ? PublicationStatus.Published
                    : PublicationStatus.Draft;

                if (status == PublicationStatus.Published && !publishedAtUtc.HasValue)
                {
                    publishedAtUtc = DateTime.UtcNow;
                }

                publication = new SongPublicationPlan(status, null, publishedAtUtc);

                return true;
            }

            DateTime? scheduledPublishAtUtc;

            try
            {
                scheduledPublishAtUtc = _publicationService.ValidateAndConvertToUtc(
                    requestedStatus,
                    scheduledPublishAtLocal);
            }
            catch (InvalidOperationException exception)
            {
                publication = default;

                error = ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.ScheduledPublishAt,
                    exception.Message);

                return false;
            }

            if (requestedStatus == PublicationStatus.Published && !publishedAtUtc.HasValue)
            {
                publishedAtUtc = DateTime.UtcNow;
            }

            publication = new SongPublicationPlan(
                requestedStatus,
                scheduledPublishAtUtc,
                publishedAtUtc);

            return true;
        }

        /// <summary>
        /// Olusturma basarisiz sonuclandiginda geride yarim kayit birakmamak icin
        /// bu istekte uretilen her seyi geri alir. Buradaki ikincil hatalar
        /// yalnizca loglanir; cagiran asil hatayi dondurmeye devam eder.
        /// </summary>
        private void DiscardCreatedSong(
            Song song,
            bool songCreated,
            int artistId,
            string? publicationJobId,
            string? storedAudioFileName)
        {
            TryCancelPublicationJob(
                publicationJobId,
                song.Id,
                "olusturma geri alinirken yayin job'i iptal edilemedi");

            if (songCreated)
            {
                try
                {
                    _songService.DeleteArtistSong(song.Id, artistId);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Sarki {SongId}: olusturma geri alinirken kayit silinemedi.",
                        song.Id);
                }
            }

            TryDeleteAudioFile(
                storedAudioFileName,
                song.Id,
                "olusturma geri alinirken mp3 silinemedi");
        }

        /// <summary>
        /// DB guncellemesi BASARISIZ oldugunda calisir.
        /// Yalnizca bu istekte ayrilan yeni kaynaklari geri alir;
        /// eski mp3 ve eski job oldugu gibi korunur.
        /// </summary>
        private void DiscardProvisionalUpdateResources(
            int songId,
            string? newPublicationJobId,
            string? newAudioFileName,
            bool audioFileChanged)
        {
            TryCancelPublicationJob(
                newPublicationJobId,
                songId,
                "guncelleme geri alinirken yeni yayin job'i iptal edilemedi");

            if (audioFileChanged)
            {
                TryDeleteAudioFile(
                    newAudioFileName,
                    songId,
                    "guncelleme geri alinirken yeni mp3 silinemedi");
            }
        }

        /// <summary>
        /// DB guncellemesi BASARIYLA tamamlandiktan sonra calisir.
        /// Yerini yeni kaynaklarin aldigi eski job ve eski mp3 temizlenir.
        /// Buradaki hata guncellemeyi gecersiz kilmaz: yeni kaynaklar korunur,
        /// kullaniciya basarili sonuc doner, hata loglanir.
        /// </summary>
        private void CleanUpReplacedUpdateResources(
            int songId,
            string? oldPublicationJobId,
            string? newPublicationJobId,
            string? oldAudioFileName,
            bool audioFileChanged)
        {
            if (!string.IsNullOrWhiteSpace(oldPublicationJobId) &&
                oldPublicationJobId != newPublicationJobId)
            {
                TryCancelPublicationJob(
                    oldPublicationJobId,
                    songId,
                    "guncelleme sonrasi eski yayin job'i iptal edilemedi");
            }

            if (audioFileChanged)
            {
                TryDeleteAudioFile(
                    oldAudioFileName,
                    songId,
                    "guncelleme sonrasi eski mp3 silinemedi");
            }
        }

        private void TryCancelPublicationJob(
            string? jobId,
            int songId,
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
                    "Sarki {SongId}: {FailureDescription} (job {JobId}).",
                    songId,
                    failureDescription,
                    jobId);
            }
        }

        private void TryDeleteAudioFile(
            string? storedFileName,
            int songId,
            string failureDescription)
        {
            if (string.IsNullOrWhiteSpace(storedFileName))
            {
                return;
            }

            try
            {
                _audioStorageService.Delete(storedFileName);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Sarki {SongId}: {FailureDescription} (dosya {AudioFileName}).",
                    songId,
                    failureDescription,
                    storedFileName);
            }
        }

        private string BuildCreationMessage(Song song)
        {
            return song.PublicationStatus switch
            {
                PublicationStatus.Draft =>
                    $"'{song.Title}' şarkısı taslak olarak kaydedildi.",

                PublicationStatus.Scheduled when song.ScheduledPublishAtUtc.HasValue =>
                    $"'{song.Title}' şarkısı " +
                    $"{_publicationService.ConvertUtcToTurkeyTime(song.ScheduledPublishAtUtc.Value):dd.MM.yyyy HH:mm} " +
                    "tarihine planlandı.",

                PublicationStatus.Published =>
                    $"'{song.Title}' şarkısı yayınlandı.",

                _ =>
                    $"'{song.Title}' şarkısı başarıyla oluşturuldu."
            };
        }

        private string BuildUpdateMessage(
            string title,
            PublicationStatus publicationStatus,
            DateTime? scheduledPublishAtUtc)
        {
            return publicationStatus switch
            {
                PublicationStatus.Draft =>
                    $"'{title.Trim()}' şarkısı taslak olarak güncellendi.",

                PublicationStatus.Scheduled when scheduledPublishAtUtc.HasValue =>
                    $"'{title.Trim()}' şarkısı " +
                    $"{_publicationService.ConvertUtcToTurkeyTime(scheduledPublishAtUtc.Value):dd.MM.yyyy HH:mm} " +
                    "tarihine planlandı.",

                PublicationStatus.Published =>
                    $"'{title.Trim()}' şarkısı yayınlandı.",

                PublicationStatus.Archived =>
                    $"'{title.Trim()}' şarkısı arşivlendi.",

                _ =>
                    $"'{title.Trim()}' şarkısı başarıyla güncellendi."
            };
        }

        private readonly record struct SongPublicationPlan(
            PublicationStatus Status,
            DateTime? ScheduledPublishAtUtc,
            DateTime? PublishedAtUtc);
    }
}
