using System.Text.RegularExpressions;
using Immich.Console;
using Immich.Models;
using Microsoft.Extensions.Configuration;

try
{
    Console.WriteLine("Immich Asset Date Updater");
    Console.WriteLine("=========================");
    Console.WriteLine("Starting...");

    const string ConfigName = "config.yaml";

    string configPath = null;
    if (File.Exists(ConfigName))
        configPath = ConfigName;
    else if (File.Exists($"/app/{ConfigName}"))
        configPath = $"/app/{ConfigName}";

    if (string.IsNullOrEmpty(configPath))
    {
        Console.WriteLine($"Could not find config file.");
        Console.WriteLine("Bye!");
        return;
    }

    var configuration = new ConfigurationBuilder()
        .AddYamlFile(configPath, optional: false, reloadOnChange: false)
        .Build();

    var apiUrl = configuration.GetValue<string>("Immich:ApiUrl");
    var apiKey = configuration.GetValue<string>("Immich:ApiKey");
    var dryRun = configuration.GetValue<bool>("Immich:DryRun");
    var logSkipped = configuration.GetValue<bool>("Immich:LogSkipped");
    var logUpdated = configuration.GetValue<bool>("Immich:LogUpdated");
    var deviceIds = configuration.GetSection("Immich:DeviceIds").Get<string[]>() ?? [];
    var moveToAlbum = configuration.GetValue<bool>("Immich:MoveToAlbum");
    var maxUpdateCount = configuration.GetValue<int>("Immich:MaxUpdateCount");
    var parsePatterns = configuration.GetSection("Immich:ParsePatterns").Get<string[]>() ?? [];
    var excludePatterns = configuration.GetSection("Immich:ExcludePatterns").Get<string[]>() ?? [];

    var fileNameParser = new FileNameParser(parsePatterns);
    var ignoredAssetsWithDigits = new List<AssetResponseDto>();
    var service = new ImmichService(apiUrl, apiKey);

    Console.WriteLine("Fetching assets...");
    var assets = await service.GetAllAssets();
    var allUpdatableAssets = assets.Where(c => deviceIds == null || deviceIds.Length == 0 || deviceIds.Contains(c.DeviceId))
        .Where(asset =>
        {
            var fileName = Path.GetFileName(asset.OriginalFileName);
            if (ShouldExclude(fileName))
                return false;

            var dateFromName = GetDesiredDateTimeFromFileName(fileName);

            // Ignore assets that do not have a date in the filename
            if (dateFromName == null)
            {
                // Log ignored assets that have digits in the name
                if (fileName.Count(c => char.IsNumber(c)) >= 6)
                    ignoredAssetsWithDigits.Add(asset);
                return false;
            }

            // If the asset has a date which is the same or older than the one from the filename then ignore it
            if (asset.FileCreatedAt != null && asset.FileCreatedAt.Value.Date <= dateFromName.Value.Date)
                return false;

            return true;
        })
        .ToList();

    var assetsToUpdate = allUpdatableAssets.Take(maxUpdateCount).ToList();

    if (ignoredAssetsWithDigits.Count != 0)
    {
        Console.WriteLine($"Found '{ignoredAssetsWithDigits}' assets that contain 6 or more digits in the name, but were not matched by any pattern (in- or exclude).");
        Console.WriteLine($"You might want to update your patterns so these will be in- excluded in the next run.");

        if (logSkipped)
            foreach (var asset in ignoredAssetsWithDigits)
                Console.WriteLine($" - {asset.OriginalFileName}");
        else
            Console.WriteLine("Enable 'LogSkipped' in the config to see the list of ignored assets.");
    }

    if (assetsToUpdate.Count == 0)
    {
        Console.WriteLine("No assets to update.");
        Console.WriteLine("Bye!");
        return;
    }

    Console.WriteLine($"Found '{Math.Min(assetsToUpdate.Count, maxUpdateCount)} / {allUpdatableAssets.Count}' assets to update...");

    if (dryRun)
    {
        if (logUpdated)
            foreach (var asset in assetsToUpdate)
            {
                var dateFromName = GetDesiredDateTimeFromFileName(asset.OriginalFileName).Value;
                Console.WriteLine($" - Asset: {asset.OriginalFileName} | Current date: {asset.LocalDateTime} | New date: {dateFromName}");
            }

        Console.WriteLine("Stopped because 'DryRun' is enabled.");
        Console.WriteLine("Bye!");
        return;
    }

    Guid? albumId = null;
    if (moveToAlbum)
    {
        var albumName = $"Immich Asset Date Updater: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        Console.WriteLine($"Creating album: {albumName}");
        albumId = await service.CreateAlbum(albumName);
    }

    Console.WriteLine("Updating assets...");
    foreach (var asset in assetsToUpdate)
    {
        var dateFromName = GetDesiredDateTimeFromFileName(asset.OriginalFileName).Value;

        await service.UpdateDateTimeOriginal(asset, dateFromName);

        if (albumId.HasValue)
            await service.AddAssetToAlbum(albumId.Value, asset);

        if (logUpdated)
            Console.WriteLine($" - Updated: {asset.OriginalFileName} | Current date: {asset.LocalDateTime} | New date: {dateFromName}");
    }

    Console.WriteLine("Assets have been updated.");
    Console.WriteLine("Bye!");

    DateTime? GetDesiredDateTimeFromFileName(string fileName)
    {
        var bestResult = fileNameParser.Parse(fileName).OrderByDescending(r => r.Score).FirstOrDefault();
        if (bestResult == null || bestResult.Score < 3)
            return null;
        return bestResult.ToDateTime();
    }

    bool ShouldExclude(string fileName)
    {
        if (excludePatterns == null || excludePatterns.Length == 0)
            return false;

        foreach (var excludePattern in excludePatterns)
        {
            var regex = new Regex(excludePattern);
            if (regex.IsMatch(fileName))
                return true;
        }

        return false;
    }
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("An error occurred:");
    Console.WriteLine(ex.ToString());
}