using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TrueVote.Api.Models;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class FakeBaseUserModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string Email { get; set; }
    }

    public class UserTest : TestHelper
    {
        public UserTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LogsMessages()
        {
            var mockSet = new Mock<DbSet<UserModel>>();
            var mockContext = new Mock<TrueVoteDbContext>();
            mockContext.Setup(m => m.Users).Returns(mockSet.Object);

            var userApi = new User(_log.Object, mockContext.Object);

            var baseUserObj = new BaseUserModel { FirstName = "Joe", Email = "joe@joe.com" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            _ = await userApi.CreateUser(_httpContext.Request);

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }

        [Fact]
        public async Task AddsUser()
        {
            var mockSet = new Mock<DbSet<UserModel>>();
            var mockContext = new Mock<TrueVoteDbContext>();
            mockContext.Setup(m => m.Users).Returns(mockSet.Object);

            var userApi = new User(_log.Object, mockContext.Object);

            var baseUserObj = new BaseUserModel { FirstName = "Joe", Email = "joe@joe.com" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            //var ret = await userApi.CreateUser(_httpContext.Request, documentsOut);

            //_output.WriteLine($"Item Count: {documentsOut.Items.Count}");
            //Assert.Single(documentsOut.Items);

            //_output.WriteLine($"Items[0]: {documentsOut.Items[0]}");

            //var json = JsonConvert.SerializeObject(documentsOut.Items[0]);

            //var u = JsonConvert.DeserializeObject<UserObj>(json);

            //_output.WriteLine($"Items[0].FirstName: {u.user.FirstName}");
            //_output.WriteLine($"Items[0].Email: {u.user.Email}");
            //_output.WriteLine($"Items[0].DateCreated: {u.user.DateCreated}");
            //_output.WriteLine($"Items[0].UserId: {u.user.UserId}");

            //Assert.Equal("Joe", u.user.FirstName);
            //Assert.Equal("joe@joe.com", u.user.Email);
            //Assert.NotNull(u.user.DateCreated);
            //Assert.IsType<DateTime>(u.user.DateCreated);
            //Assert.NotEmpty(u.user.UserId);

            //Assert.NotNull(ret);
            //var objectResult = Assert.IsType<CreatedResult>(ret);
            //Assert.Equal((int) HttpStatusCode.Created, objectResult.StatusCode);

            //_log.Verify(LogLevel.Information, Times.AtLeast(1));
            //_log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }

        [Fact]
        public async Task HandlesInvalidUserCreate()
        {
            var mockSet = new Mock<DbSet<UserModel>>();
            var mockContext = new Mock<TrueVoteDbContext>();
            mockContext.Setup(m => m.Users).Returns(mockSet.Object);

            var userApi = new User(_log.Object, mockContext.Object);

            // This object is missing required property (email)
            var fakeBaseUserObj = new FakeBaseUserModel { FirstName = "Joe" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(fakeBaseUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await userApi.CreateUser(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);
            Assert.Contains("Required", objectResult.Value.ToString());

            _log.Verify(LogLevel.Error, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }

        [Fact]
        public async Task FindsUser()
        {
            var mockSet = new Mock<DbSet<UserModel>>();
            var mockContext = new Mock<TrueVoteDbContext>();
            mockContext.Setup(m => m.Users).Returns(mockSet.Object);

            var userApi = new User(_log.Object, mockContext.Object);

            var findUserObj = new FindUserModel { FirstName = "Joe" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await userApi.UserFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<OkObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.OK, objectResult.StatusCode);

            // TODO Inspect objectResult for data

            _log.Verify(LogLevel.Information, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }

        [Fact]
        public async Task HandlesFindUserError()
        {
            var mockSet = new Mock<DbSet<UserModel>>();
            var mockContext = new Mock<TrueVoteDbContext>();
            mockContext.Setup(m => m.Users).Returns(mockSet.Object);

            var userApi = new User(_log.Object, mockContext.Object);

            var findUserObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(findUserObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await userApi.UserFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);


            _log.Verify(LogLevel.Error, Times.AtLeast(1));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }
    }
}
