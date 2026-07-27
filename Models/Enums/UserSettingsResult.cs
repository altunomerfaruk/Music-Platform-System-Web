namespace MusicProject.Models.Enums
{
    public enum UserSettingsResult
    {
        Success = 1,
        UserNotFound = 2,
        UsernameAlreadyExists = 3,
        EmailAlreadyExists = 4,
        CurrentPasswordIncorrect = 5,
        NewPasswordRequired = 6
    }
}