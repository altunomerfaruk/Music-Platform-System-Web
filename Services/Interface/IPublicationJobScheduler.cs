namespace MusicProject.Services.Interface
{
    public interface IPublicationJobScheduler
    {
        string ScheduleAlbumPublication(int albumId, DateTime publishAtUtc);

        string ScheduleSongPublication(int songId, DateTime publishAtUtc);

        void Cancel(string? jobId);
    }
}