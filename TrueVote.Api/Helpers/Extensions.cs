using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.IO;
using TrueVote.Api.Models;

namespace TrueVote.Api
{
    [ExcludeFromCodeCoverage]
    public static partial class Extensions
    {
        public static string ToTitleCase(this string @this)
        {
            return new CultureInfo("en-US").TextInfo.ToTitleCase(@this);
        }

        public static string ExtractUrl(this string @this)
        {
            // Define the regular expression pattern
            var pattern = @"https?://\S+$";

            // Create a Regex object with the pattern
            var regex = new Regex(pattern);

            // Match the pattern against the input string
            var match = regex.Match(@this);

            // Check if a match is found
            return match.Success ? match.Value : string.Empty;
        }
    }

    [ExcludeFromCodeCoverage]
    public static class HttpResponseDataExtensions
    {
        private static HttpResponseData CreateBasicResponse(this HttpRequestData request, HttpStatusCode statusCode)
        {
            var response = request.CreateResponse(statusCode);
            response.Headers = new HttpHeadersCollection
            {
                { "Content-Type", "application/json" }
            };

            return response;
        }

        private static async Task<HttpResponseData> CreateJsonResponseAsync(
            this HttpRequestData request, HttpStatusCode statusCode, object content)
        {
            var response = request.CreateBasicResponse(statusCode);
            var json = JsonSerializer.Serialize(content);
            await response.WriteStringAsync(json);

            return response;
        }

        private static HttpResponseData CreateJsonResponse(
            this HttpRequestData request, HttpStatusCode statusCode, object content)
        {
            var response = request.CreateBasicResponse(statusCode);
            var json = JsonSerializer.Serialize(content);
            response.WriteString(json);

            return response;
        }

        public static HttpResponseData CreateNotFoundResponse(
            this HttpRequestData request)
        {
            return request.CreateBasicResponse(HttpStatusCode.NotFound);
        }

        public static async Task<HttpResponseData> CreateNotFoundResponseAsync(
            this HttpRequestData request, SecureString content)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.NotFound, content);
        }

        public static async Task<HttpResponseData> CreateConflictResponseAsync(
            this HttpRequestData request, SecureString content)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.Conflict, content);
        }

        public static async Task<HttpResponseData> CreateBadRequestResponseAsync(
            this HttpRequestData request, SecureString content)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.BadRequest, content);
        }

        public static HttpResponseData CreateBadRequestResponse(
            this HttpRequestData request, SecureString content)
        {
            return request.CreateJsonResponse(HttpStatusCode.BadRequest, content);
        }

        public static HttpResponseData CreateOkResponse(
            this HttpRequestData request)
        {
            return request.CreateBasicResponse(HttpStatusCode.OK);
        }

        public static async Task<HttpResponseData> CreateOkResponseAsync(
            this HttpRequestData request, object content)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.OK, content);
        }

        public static async Task<HttpResponseData> CreateCreatedResponseAsync(
            this HttpRequestData request, object content)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.Created, content);
        }

        public static async Task<HttpResponseData> CreateUnauthorizedResponseAsync(
            this HttpRequestData request, object content)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.Unauthorized, content);
        }

        public static HttpResponseData CreateUnauthorizedResponse(
            this HttpRequestData request, object content)
        {
            return request.CreateJsonResponse(HttpStatusCode.Unauthorized, content);
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpResponseData response)
        {
            _ = response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(response.Body);
            var responseBody = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<T>(responseBody);
        }
    }
}
