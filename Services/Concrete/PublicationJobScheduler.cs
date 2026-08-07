using Hangfire;
using MusicProject.Services.Background;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class PublicationJobScheduler : IPublicationJobScheduler
    {
        public string ScheduleAlbumPublication(int albumId, DateTime publishAtUtc)
        {
            return BackgroundJob.Schedule<PublicationJob>(
                job => job.PublishAlbum(albumId),
                publishAtUtc);
        }

        public string ScheduleSongPublication(int songId, DateTime publishAtUtc)
        {
            return BackgroundJob.Schedule<PublicationJob>(
                job => job.PublishSong(songId),
                publishAtUtc);
        }

        public void Cancel(string? jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return;
            }

            BackgroundJob.Delete(jobId);
        }
    }
}