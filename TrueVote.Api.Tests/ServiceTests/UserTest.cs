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
        public int x;
        public Models.User user;
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
            var user = new Api.User(_log.Object);
            _ = await user.Run(_httpContext.Request, documentsOut);

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }

        [Fact]
        public async Task AddsUser()
        {
            var documentsOut = new MockAsyncCollector<dynamic>();
            var user = new Api.User(_log.Object);

            var userObj = new Models.User { FirstName = "Joe", Email = "joe@joe.com" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(userObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            _ = await user.Run(_httpContext.Request, documentsOut);

            _output.WriteLine($"Item Count: {documentsOut.Items.Count}");
            Assert.Single(documentsOut.Items);

            _output.WriteLine($"Items[0]: {documentsOut.Items[0]}");

            var u = JsonConvert.DeserializeObject<UserObj>(documentsOut.Items[0]);

            _output.WriteLine($"Items[0].FirstName: {u.FirstName}");
            _output.WriteLine($"Items[0].Email: {u.Email}");

            Assert.Equal("Joe", u.FirstName);
            Assert.Equal("joe@joe.com", u.Email);

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }
    }
}
