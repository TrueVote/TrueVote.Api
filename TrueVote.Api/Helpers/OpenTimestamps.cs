using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TrueVote.Api.Helpers
{
    public class OpenTimestampsClient : IDisposable
    {
        private readonly Uri _uri;
        private readonly HttpClient _httpClient;

        public OpenTimestampsClient(Uri uri)
        {
            _uri = uri;
            _httpClient = new HttpClient();
        }

        public async Task<byte[]> Stamp(byte[] hash)
        {
            var requestUri = new Uri(_uri, "/digest");

            using var content = new ByteArrayContent(hash);

            var response = await _httpClient.PostAsync(requestUri, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var timestampBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

            return timestampBytes;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
