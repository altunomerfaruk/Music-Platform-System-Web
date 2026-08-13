using MusicProject.Contracts.Requests;
using MusicProject.Contracts.Responses.ArtistDashboard;
using MusicProject.Models.Concrete;
using MusicProject.Models.Enums;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class ArtistSongWorkflowManager : IArtistSongWorkflowService
    {
        private readonly ISongService _songService;
        private readonly IAlbumService _albumService;
        private readonly IPublicationService _publicationService;
        private readonly IPublicationJobScheduler _publicationJobScheduler;
        private readonly IAudioStorageService _audioStorageService;

        public ArtistSongWorkflowManager(
            ISongService songService,
            IAlbumService albumService,
            IPublicationService publicationService,
            IPublicationJobScheduler publicationJobScheduler,
            IAudioStorageService audioStorageService)
        {
            _songService = songService;
            _albumService = albumService;
            _publicationService = publicationService;
            _publicationJobScheduler = publicationJobScheduler;
            _audioStorageService = audioStorageService;
        }

        public async Task<ArtistSongWorkflowResult> CreateSongAsync(
            CreateArtistSongRequest request)
        {
            var selectedAlbum = FindArtistAlbum(request.AlbumId, request.ArtistId);

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
                    song.PublicationJobId =
                        _publicationJobScheduler.ScheduleSongPublication(
                            song.Id,
                            song.ScheduledPublishAtUtc.Value);

                    _songService.UpdatePublication(song);
                }
            }
            catch (InvalidOperationException exception)
            {
                if (!songCreated)
                {
                    _audioStorageService.Delete(storedAudioFileName);
                }

                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.Title,
                    exception.Message);
            }
            catch
            {
                if (!songCreated)
                {
                    _audioStorageService.Delete(storedAudioFileName);
                }

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

                if (!string.IsNullOrWhiteSpace(oldPublicationJobId) &&
                    oldPublicationJobId != newPublicationJobId)
                {
                    _publicationJobScheduler.Cancel(oldPublicationJobId);
                }

                if (audioFileChanged)
                {
                    _audioStorageService.Delete(oldAudioFileName);
                }
            }
            catch (InvalidOperationException exception)
            {
                RollbackUpdate(newPublicationJobId, newAudioFileName, audioFileChanged);

                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.Title,
                    exception.Message);
            }
            catch
            {
                RollbackUpdate(newPublicationJobId, newAudioFileName, audioFileChanged);

                throw;
            }

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

                _publicationJobScheduler.Cancel(publicationJobId);
                _audioStorageService.Delete(audioFileName);
            }
            catch (InvalidOperationException exception)
            {
                return ArtistSongWorkflowResult.Failure(
                    ArtistSongWorkflowField.None,
                    exception.Message);
            }

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

        private void RollbackUpdate(
            string? newPublicationJobId,
            string? newAudioFileName,
            bool audioFileChanged)
        {
            if (!string.IsNullOrWhiteSpace(newPublicationJobId))
            {
                _publicationJobScheduler.Cancel(newPublicationJobId);
            }

            if (audioFileChanged)
            {
                _audioStorageService.Delete(newAudioFileName);
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
