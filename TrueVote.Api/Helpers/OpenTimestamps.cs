using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TrueVote.Api.Helpers
{
    public interface IOpenTimestampsClient
    {
        Task<byte[]> Stamp(byte[] hash);
    }

    public class OpenTimestampsClient: IOpenTimestampsClient
    {
        private readonly Uri _uri;
        private readonly HttpClient _httpClient;

        public OpenTimestampsClient(Uri uri, HttpClient httpClient)
        {
            _uri = uri;
            _httpClient = httpClient;
        }

        public async virtual Task<byte[]> Stamp(byte[] hash)
        {
            var requestUri = new Uri(_uri, "/digest");

            using var content = new ByteArrayContent(hash);

            var response = await _httpClient.PostAsync(requestUri, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var timestampBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

            return timestampBytes;
        }
    }
}
