using Immich.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Immich.Console
{
    public class ImmichService
    {
        private readonly string _apiKey;
        private readonly ImmichClient _client;

        public ImmichService(string apiUrl, string apiKey)
        {
            var authProvider = new AnonymousAuthenticationProvider();
            var requestAdapter = new HttpClientRequestAdapter(authProvider)
            {
                BaseUrl = apiUrl
            };

            _client = new ImmichClient(requestAdapter);
            _apiKey = apiKey;
        }

        public async Task<List<AssetResponseDto>> GetAllAssets()
        {
            var items = new List<AssetResponseDto>();

            var page = 1;
            while (true)
            {
                var response = await _client.Search.Metadata.PostAsync(new MetadataSearchDto
                {
                    Size = 1000,
                    Page = page,
                }, x => x.Headers.Add("x-api-key", _apiKey));

                items.AddRange(response.Assets.Items);

                page++;

                if (response.Assets.NextPage == null)
                    break;
            }

            return items;
        }

        public async Task UpdateDateTimeOriginal(AssetResponseDto asset, string dateTime)
        {
            await _client.Assets[Guid.Parse(asset.Id)].PutAsync(new UpdateAssetDto
            {
                DateTimeOriginal = dateTime
            }, x => x.Headers.Add("x-api-key", _apiKey));
        }

        public async Task<Guid> CreateAlbum(string albumName)
        {
            var response = await _client.Albums.PostAsync(new CreateAlbumDto
            {
                AlbumName = albumName
            }, x => x.Headers.Add("x-api-key", _apiKey));

            return Guid.Parse(response.Id);
        }

        public async Task AddAssetToAlbum(Guid albumId, AssetResponseDto asset)
        {
            await _client.Albums[albumId].Assets.PutAsync(new BulkIdsDto
            {
                Ids = [(Guid?)Guid.Parse(asset.Id)]
            }, x => x.Headers.Add("x-api-key", _apiKey));
        }
    }
}