using MusicProject.Models.Enums;

namespace MusicProject.Services.Interface
{
    public interface IPublicationService
    {
        DateTime? ValidateAndConvertToUtc(PublicationStatus publicationStatus, DateTime? scheduledPublishAtLocal);

        DateTime ConvertUtcToTurkeyTime(DateTime utcDateTime);
    }
}