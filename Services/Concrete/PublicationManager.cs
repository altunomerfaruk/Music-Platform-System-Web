using MusicProject.Models.Enums;
using MusicProject.Services.Interface;

namespace MusicProject.Services.Concrete
{
    public class PublicationManager : IPublicationService
    {
        private readonly TimeZoneInfo _turkeyTimeZone;

        public PublicationManager()
        {
            _turkeyTimeZone = GetTurkeyTimeZone();
        }

        public DateTime? ValidateAndConvertToUtc(
            PublicationStatus publicationStatus,
            DateTime? scheduledPublishAtLocal)
        {
            if (publicationStatus != PublicationStatus.Scheduled)
            {
                return null;
            }

            if (!scheduledPublishAtLocal.HasValue)
            {
                throw new InvalidOperationException(
                    "Planlı yayın için yayın tarihi ve saati seçmelisiniz.");
            }

            var localPublishTime = DateTime.SpecifyKind(
                scheduledPublishAtLocal.Value,
                DateTimeKind.Unspecified);

            var currentTurkeyTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                _turkeyTimeZone);

            if (localPublishTime <= currentTurkeyTime)
            {
                throw new InvalidOperationException(
                    "Planlanan yayın zamanı gelecekte olmalıdır.");
            }

            if (localPublishTime.DayOfWeek != DayOfWeek.Friday)
            {
                throw new InvalidOperationException(
                    "Planlı yayınlar yalnızca cuma günü yapılabilir.");
            }

            return TimeZoneInfo.ConvertTimeToUtc(
                localPublishTime,
                _turkeyTimeZone);
        }

        public DateTime ConvertUtcToTurkeyTime(DateTime utcDateTime)
        {
            var specifiedUtcDateTime = DateTime.SpecifyKind(
                utcDateTime,
                DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(
                specifiedUtcDateTime,
                _turkeyTimeZone);
        }

        private static TimeZoneInfo GetTurkeyTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            }
        }
    }
}