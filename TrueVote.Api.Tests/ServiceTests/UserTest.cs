using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

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

    public class UserTest : TestHelper
    {
        public UserTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LogsMessages()
        {
            var documentsOut = new MockAsyncCollector<dynamic>();
            var userApi = new User(_log.Object);

            var baseUserObj = new Models.BaseUserModel { FirstName = "Joe", Email = "joe@joe.com" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            _ = await userApi.Run(_httpContext.Request, documentsOut);

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }

        [Fact]
        public async Task AddsUser()
        {
            var documentsOut = new MockAsyncCollector<dynamic>();
            var userApi = new User(_log.Object);

            var baseUserObj = new Models.BaseUserModel { FirstName = "Joe", Email = "joe@joe.com" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            _ = await userApi.Run(_httpContext.Request, documentsOut);

            _output.WriteLine($"Item Count: {documentsOut.Items.Count}");
            Assert.Single(documentsOut.Items);

            _output.WriteLine($"Items[0]: {documentsOut.Items[0]}");

            var json = JsonConvert.SerializeObject(documentsOut.Items[0]);

            var u = JsonConvert.DeserializeObject<UserObj>(json);

            _output.WriteLine($"Items[0].FirstName: {u.user.FirstName}");
            _output.WriteLine($"Items[0].Email: {u.user.Email}");
            _output.WriteLine($"Items[0].DateCreated: {u.user.DateCreated}");
            _output.WriteLine($"Items[0].UserId: {u.user.UserId}");

            Assert.Equal("Joe", u.user.FirstName);
            Assert.Equal("joe@joe.com", u.user.Email);
            Assert.NotNull(u.user.DateCreated);
            Assert.NotEmpty(u.user.UserId);

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }
    }
}
