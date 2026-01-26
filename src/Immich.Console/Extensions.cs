using NodaTime;

namespace Immich.Console
{
    public static class Extensions
    {
        public static string ToImmichString(this Instant instant, string timeZone)
        {
            if (string.IsNullOrEmpty(timeZone))
                return instant.ToString();

            var tz = DateTimeZoneProviders.Tzdb[timeZone];
            var offset = tz.GetUtcOffset(instant);
            var offsetDuration = Duration.FromTimeSpan(offset.ToTimeSpan());
            return instant.Minus(offsetDuration).WithOffset(offset).ToString();
        }
    }
}
