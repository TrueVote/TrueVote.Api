using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

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
        private static async Task<HttpResponseData> CreateJsonResponseAsync(
            this HttpRequestData request, HttpStatusCode statusCode, object content)
        {
            var response = request.CreateResponse(statusCode);
            await response.WriteAsJsonAsync(content);
            return response;
        }

        public static HttpResponseData CreateNotFoundResponse(
            this HttpRequestData request)
        {
            return request.CreateResponse(HttpStatusCode.NotFound);
        }

        public static async Task<HttpResponseData> CreateNotFoundResponseAsync(
            this HttpRequestData request, object content)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.NotFound, content);
        }

        public static async Task<HttpResponseData> CreateConflictResponseAsync(
            this HttpRequestData request, object content)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.Conflict, content);
        }

        public static async Task<HttpResponseData> CreateBadRequestJsonResponseAsync(
            this HttpRequestData request, string errorMessage)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.BadRequest, errorMessage);
        }

        public static async Task<HttpResponseData> CreateOkJsonResponseAsync(
            this HttpRequestData request, object content)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.OK, content);
        }

        public static HttpResponseData CreateOkResponse(
            this HttpRequestData request)
        {
            return request.CreateResponse(HttpStatusCode.OK);
        }

        public static async Task<HttpResponseData> CreateCreatedJsonResponseAsync(
            this HttpRequestData request, object content)
        {
            return await request.CreateJsonResponseAsync(HttpStatusCode.Created, content);
        }
    }
}
