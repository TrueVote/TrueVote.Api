/*
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace TrueVote.Api.Helpers.Tests
{
    public class OpenTimestampsTests
    {
        private static OpenTimestampsClient CreateOpenTimestampsClient(HttpClient httpClient)
        {
            var uri = new Uri("https://example.com");
            return new OpenTimestampsClient(uri, httpClient);
        }

        private static HttpClient CreateHttpClient(HttpResponseMessage response)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<IActionResult>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            return httpClient;
        }

        [Fact]
        public async Task StampReturnsTimestampBytes()
        {
            var expectedBytes = new byte[] { 0x01, 0x02, 0x03 };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(expectedBytes)
            };
            var httpClient = CreateHttpClient(response);
            var openTimestampsClient = CreateOpenTimestampsClient(httpClient);

            var result = await openTimestampsClient.Stamp(new byte[32]);

            Assert.Equal(expectedBytes, result);
        }

        [Fact]
        public async Task StampRequestFailsThrowsException()
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var httpClient = CreateHttpClient(response);
            var openTimestampsClient = CreateOpenTimestampsClient(httpClient);

            try
            {
                var result = await openTimestampsClient.Stamp(new byte[32]);
                Assert.True(false);
            }
            catch (Exception ex)
            {
                Assert.NotNull(ex);
                Assert.Contains("500 (Internal Server Error).", ex.Message);
            }
        }
    }
}
*/
