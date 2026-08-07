using MusicProject.Models.Enums;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Background
{
    public class PublicationJob
    {
        private readonly IAlbumService _albumService;
        private readonly ISongService _songService;

        public PublicationJob(IAlbumService albumService, ISongService songService)
        {
            _albumService = albumService;
            _songService = songService;
        }

        public void PublishAlbum(int albumId)
        {
            var album = _albumService.GetAlbumById(albumId);

            if (album == null)
            {
                return;
            }

            if (album.PublicationStatus != PublicationStatus.Scheduled)
            {
                return;
            }

            var publishedAtUtc = album.ScheduledPublishAtUtc ?? DateTime.UtcNow;

            album.PublicationStatus = PublicationStatus.Published;
            album.PublishedAtUtc ??= publishedAtUtc;
            album.ScheduledPublishAtUtc = null;
            album.PublicationJobId = null;

            _albumService.UpdatePublication(album);

            foreach (var song in album.Songs)
            {
                if (song.PublicationStatus == PublicationStatus.Archived)
                {
                    continue;
                }

                song.PublicationStatus = PublicationStatus.Published;
                song.PublishedAtUtc ??= publishedAtUtc;
                song.ScheduledPublishAtUtc = null;
                song.PublicationJobId = null;

                _songService.UpdatePublication(song);
            }
        }

        public void PublishSong(int songId)
        {
            var song = _songService.GetSongById(songId);

            if (song == null)
            {
                return;
            }

            if (song.PublicationStatus != PublicationStatus.Scheduled)
            {
                return;
            }

            if (song.AlbumId.HasValue)
            {
                return;
            }

            var publishedAtUtc = song.ScheduledPublishAtUtc ?? DateTime.UtcNow;

            song.PublicationStatus = PublicationStatus.Published;
            song.PublishedAtUtc ??= publishedAtUtc;
            song.ScheduledPublishAtUtc = null;
            song.PublicationJobId = null;

            _songService.UpdatePublication(song);
        }
    }
}