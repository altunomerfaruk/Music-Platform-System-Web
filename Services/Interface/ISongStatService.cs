namespace MusicProject.Services.Interface
{
    public interface ISongStatService
    {
        void UpdateLikeCount(
            int songId,
            int totalLikes
        );
    }
}