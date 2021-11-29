using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class MockAsyncCollector<T> : IAsyncCollector<T>
    {
        public readonly List<T> Items = new();

        public Task AddAsync(T item, CancellationToken cancellationToken = default)
        {
            Items.Add(item);

            return Task.FromResult(true);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    public class UserObj
    {
        public Models.UserModel user;
    }

    public class User
    {
        private readonly ITestOutputHelper _output;
        private readonly HttpContext _httpContext;
        private readonly Mock<ILogger<Api.Error500>> _log;

        public User(ITestOutputHelper output)
        {
            _output = output;
            _httpContext = new DefaultHttpContext();
            _log = new Mock<ILogger<Api.Error500>>();
            _log.MockLog(LogLevel.Debug);
            _log.MockLog(LogLevel.Information);
            _log.MockLog(LogLevel.Warning);
            _log.MockLog(LogLevel.Error);
        }

        [Fact]
        public async Task LogsMessages()
        {
            var documentsOut = new MockAsyncCollector<dynamic>();
            var userApi = new Api.User(_log.Object);
            _ = await userApi.Run(_httpContext.Request, documentsOut);

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }

        [Fact]
        public async Task AddsUser()
        {
            var documentsOut = new MockAsyncCollector<dynamic>();
            var userApi = new Api.User(_log.Object);

            var userObj = new Models.UserModel { FirstName = "Joe", Email = "joe@joe.com" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(userObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            _ = await userApi.Run(_httpContext.Request, documentsOut);

            _output.WriteLine($"Item Count: {documentsOut.Items.Count}");
            Assert.Single(documentsOut.Items);

            _output.WriteLine($"Items[0]: {documentsOut.Items[0]}");

            var json = JsonConvert.SerializeObject(documentsOut.Items[0]);

            var u = JsonConvert.DeserializeObject<UserObj>(json);

            _output.WriteLine($"Items[0].FirstName: {u.user.FirstName}");
            _output.WriteLine($"Items[0].Email: {u.user.Email}");

            Assert.Equal("Joe", u.user.FirstName);
            Assert.Equal("joe@joe.com", u.user.Email);

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }
    }
}
