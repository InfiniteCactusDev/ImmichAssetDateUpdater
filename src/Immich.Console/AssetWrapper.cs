using Immich.Models;
using NodaTime;

namespace Immich.Console
{
    public class AssetWrapper(AssetResponseDto _asset)
    {
        public DateTimeOffset? GetUTC()
        {
            return _asset.FileCreatedAt?.ToUniversalTime();
        }

        public Guid Id => Guid.Parse(_asset.Id);
        public string DeviceId => _asset.DeviceId;
        public string OriginalFileName => _asset.OriginalFileName;

        public Instant? DateTimeOriginalUTC => _asset.FileCreatedAt.HasValue ? Instant.FromDateTimeOffset(_asset.FileCreatedAt.Value) : null;

        public Instant? GetUtcDateTimeFromFileName(FileNameParser fileNameParser)
        {
            var bestResult = fileNameParser.Parse(_asset.OriginalFileName)
                .OrderByDescending(r => r.Score)
                .FirstOrDefault();

            if (bestResult == null || bestResult.Score < 3)
                return null;

            return bestResult.ToDateTimeUTC();
        }
    }
}
