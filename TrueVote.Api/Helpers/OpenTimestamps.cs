using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TrueVote.Api.Helpers
{
    public interface IOpenTimestampsClient
    {
        Task<byte[]> Stamp(byte[] hash);
    }

    public class OpenTimestampsClient : IOpenTimestampsClient
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
            // TODO Temp for debugging - remove next line:
            // return hash;

            // Construct the request URI by combining the base URI with the "/digest" endpoint
            var requestUri = new Uri(_uri, "/digest");

            using var content = new ByteArrayContent(hash);

            // Send a POST request to the specified URI with the hash as the request content
            var response = await _httpClient.PostAsync(requestUri, content).ConfigureAwait(false);

            // Ensure the response has a successful status code
            response.EnsureSuccessStatusCode();

            // Read the response content as byte array, representing the timestamp bytes
            var timestampBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

            return timestampBytes;
        }
    }
}
