using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var baseUserObj = new BaseUserModel { FirstName = "Joe", Email = "joe@joe.com" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            _ = await _userApi.CreateUser(_httpContext.Request);

            logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsUser()
        {
            var baseUserObj = new BaseUserModel { FirstName = "Joe", Email = "joe@joe.com" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(baseUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _userApi.CreateUser(_httpContext.Request) as CreatedResult;
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<CreatedResult>(ret);
            Assert.Equal((int) HttpStatusCode.Created, objectResult.StatusCode);

            var val = ret.Value as UserModel;
            Assert.NotNull(val);

            _output.WriteLine($"Item: {val}");

            _output.WriteLine($"Item.FirstName: {val.FirstName}");
            _output.WriteLine($"Item.Email: {val.Email}");
            _output.WriteLine($"Item.DateCreated: {val.DateCreated}");
            _output.WriteLine($"Item.UserId: {val.UserId}");

            Assert.Equal("Joe", val.FirstName);
            Assert.Equal("joe@joe.com", val.Email);
            Assert.IsType<DateTime>(val.DateCreated);
            Assert.NotEmpty(val.UserId);

            logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesInvalidUserCreate()
        {
            // This object is missing required property (email)
            var fakeBaseUserObj = new FakeBaseUserModel { FirstName = "Joe" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(fakeBaseUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _userApi.CreateUser(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);
            Assert.Contains("Required", objectResult.Value.ToString());

            logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsUser()
        {
            var findUserData = new List<UserModel>
            {
                new UserModel { Email = "foo@bar.com", DateCreated = DateTime.Now, FirstName = "Foo", UserId = "1" },
                new UserModel { Email = "foo2@bar.com", DateCreated = DateTime.Now.AddSeconds(1), FirstName = "Foo2", UserId = "2" },
                new UserModel { Email = "boo@bar.com", DateCreated = DateTime.Now.AddSeconds(2), FirstName = "Boo", UserId = "3" }
            }.AsQueryable();
            var mockUserSet = DbMoqHelper.GetDbSet(findUserData);

            var mockUserContext = new Mock<TrueVoteDbContext>();
            mockUserContext.Setup(m => m.Users).Returns(mockUserSet.Object);

            var findUserObj = new FindUserModel { FirstName = "Foo" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var userApi = new User(logHelper.Object, mockUserContext.Object, mockTelegram.Object);

            var ret = await userApi.UserFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<OkObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.OK, objectResult.StatusCode);

            var val = objectResult.Value as List<UserModel>;
            Assert.NotEmpty(val);
            Assert.Equal(2, val.Count);
            Assert.Equal("Foo2", val[0].FirstName);
            Assert.Equal("foo2@bar.com", val[0].Email);

            logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundUser()
        {
            var findUserData = new List<UserModel>
            {
                new UserModel { Email = "foo@bar.com", DateCreated = DateTime.Now, FirstName = "Foo", UserId = "1" },
                new UserModel { Email = "foo2@bar.com", DateCreated = DateTime.Now.AddSeconds(1), FirstName = "Foo2", UserId = "2" },
                new UserModel { Email = "boo@bar.com", DateCreated = DateTime.Now.AddSeconds(2), FirstName = "Boo", UserId = "3" }
            }.AsQueryable();
            var mockUserSet = DbMoqHelper.GetDbSet(findUserData);

            var mockUserContext = new Mock<TrueVoteDbContext>();
            mockUserContext.Setup(m => m.Users).Returns(mockUserSet.Object);

            var findUserObj = new FindUserModel { FirstName = "not going to find anything" };
            var byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(findUserObj));
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var userApi = new User(logHelper.Object, mockUserContext.Object, mockTelegram.Object);

            var ret = await userApi.UserFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<NotFoundResult>(ret);
            Assert.Equal((int) HttpStatusCode.NotFound, objectResult.StatusCode);

            logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesFindUserError()
        {
            var findUserObj = "blah";
            var byteArray = Encoding.ASCII.GetBytes(findUserObj);
            _httpContext.Request.Body = new MemoryStream(byteArray);

            var ret = await _userApi.UserFind(_httpContext.Request);
            Assert.NotNull(ret);
            var objectResult = Assert.IsType<BadRequestObjectResult>(ret);
            Assert.Equal((int) HttpStatusCode.BadRequest, objectResult.StatusCode);

            logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}
