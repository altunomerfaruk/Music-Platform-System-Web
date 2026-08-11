namespace MusicProject.Models.Enums
{
    public enum AdminUserStatusUpdateResult
    {
        Success = 1,
        UserNotFound = 2,
        CannotChangeOwnStatus = 3
    }
}