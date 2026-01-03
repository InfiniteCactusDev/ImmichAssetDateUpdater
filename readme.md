# Immich Asset Date Updater
### 📅 About

The Immich Asset Date Updater searches an Immich library for assets that have a date in the filename. It then checks if the asset has no date, or if its date is older than that of the filebame. It will then change the date with the one of the filename. There are several checks and options in place that make the tool safe and easy to use and customizable. This tool was initially made for private use, but made public due to high demand. Even though I've tested it thouroughly there are no guarantees.

### ⚠️ Readme

Make sure you test first with `DryRun=true` and/or with a low MaxUpdateCount to ensure this tool fits your needs. And obviously ensure you have proper backups in place.

Special attention should be paid to assets with differing timezones. This is not something I have tested much myself. Since v0.3 it does have limited timezone support.

If no time can be retrieved from the fimename it will default to 12:00:00.

If this tool doesn't quite fit your needs, there are alternatives available. I have not tried or reviewed them:
 - https://github.com/FlorianKrauseResearch/Immich-Metadata-Update
 - https://github.com/veryaner/immich-updater

### ▶️ Runing the tool

Pull the image locally from ghcr:

```bash
docker pull ghcr.io/infinitecactusdev/immichassetdateupdater:0.3
```

Download a copy of [config.example.yaml](https://github.com/InfiniteCactusDev/ImmichAssetDateUpdater/blob/main/config.example.yaml) locally and change it to match your requirements.

Run the docker image and make sure it has access to the config file:

```bash
docker run -v "./config.yaml:/app/config.yaml:ro" --rm "ghcr.io/infinitecactusdev/immichassetdateupdater:0.3"
```

### ⚒️ Build the image yourself

Clone the repo and build it:

```bash
docker build -t immichassetdateupdater:0.3 .
```
