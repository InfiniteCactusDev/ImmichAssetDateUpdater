# Immich Asset Date Updater
### ⚠️ Readme

This tool was made for private use, but made public due to high demand. Even though I've tested it thouroughly there are no guarantees. Make sure you test first with DryRun=true` or a low MaxUpdateCount to ensure this tool fits your needs. And ensure you have backed up your system.

Special attention should be paid to assets with differing timezones. This is not something I have paid much attention to.

If no time can be retrieved from the fimename it will default to 12:00:00.

In case you're looking for alternatives, there's also this repo that does basically the same with a python script (not tested).
https://github.com/FlorianKrauseResearch/Immich-Metadata-Update

### ▶️ Runing the tool

Make sure you have pulled the image locally from ghcr:

```bash
docker pull ghcr.io/infinitecactusdev/immichassetdateupdater:0.2
```

Create a copy of `config.example.yaml` locally and update it to match your requirements.

Run the docker image and make sure it has access to the config file,

```bash
docker run -v "./config.yaml:/app/config.yaml:ro" --rm "ghcr.io/infinitecactusdev/immichassetdateupdater:0.2"
```

### ⚒️ Build the image yourself

Clone the repo and build it:

```bash
docker build -t immichassetdateupdater:0.2 .
```
