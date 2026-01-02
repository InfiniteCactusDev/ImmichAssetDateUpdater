using NodaTime;

namespace Immich.Console
{
    public class DateHelper
    {
        public static string ToTimeZoneString(DateTime dateTime, string timeZone)
        {
            var tz = DateTimeZoneProviders.Tzdb[timeZone];
            var instant = Instant.FromDateTimeUtc(dateTime);
            var offset = tz.GetUtcOffset(instant);
            var offsetDuration = Duration.FromTimeSpan(offset.ToTimeSpan());
            return instant.Minus(offsetDuration).WithOffset(offset).ToString();
        }
    }
}
