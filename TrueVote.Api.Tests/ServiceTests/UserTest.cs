using Microsoft.Extensions.Logging;
using Moq;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(baseUserObj));

            _ = await _userApi.CreateUser(requestData);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsUser()
        {
            var baseUserObj = new BaseUserModel { FirstName = "Joe", Email = "joe@joe.com" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(baseUserObj));

            var ret = await _userApi.CreateUser(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.Created, ret.StatusCode);

            var val = await ret.ReadAsJsonAsync<UserModel>();
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

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesInvalidUserCreate()
        {
            // This object is missing required property (email)
            var fakeBaseUserObj = new FakeBaseUserModel { FirstName = "Joe" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(fakeBaseUserObj));

            var ret = await _userApi.CreateUser(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
            Assert.Contains("Required", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsUser()
        {
            var findUserObj = new FindUserModel { FirstName = "Foo" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(findUserObj));

            var userApi = new User(_logHelper.Object, _moqDataAccessor.mockUserContext.Object, _mockServiceBus.Object);

            var ret = await userApi.UserFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.OK, ret.StatusCode);

            var val = await ret.ReadAsJsonAsync<List<UserModel>>();
            Assert.NotEmpty(val);
            Assert.Equal(2, val.Count);
            Assert.Equal("Foo2", val[0].FirstName);
            Assert.Equal("foo2@bar.com", val[0].Email);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundUser()
        {
            var findUserObj = new FindUserModel { FirstName = "not going to find anything" };
            var requestData = new MockHttpRequestData(JsonConvert.SerializeObject(findUserObj));

            var userApi = new User(_logHelper.Object, _moqDataAccessor.mockUserContext.Object, _mockServiceBus.Object);

            var ret = await userApi.UserFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.NotFound, ret.StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesFindUserError()
        {
            var findUserObj = "blah";
            var requestData = new MockHttpRequestData(findUserObj);

            var ret = await _userApi.UserFind(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public void GetHashValid()
        {
            var keyPair = new Key();

            var signInEventModel = new SignInEventModel
            {
                Kind = new StringWrapper { Value = "1" },
                PubKey = new PubKeyWrapper { Value = keyPair.PubKey.ToBytes() },
                CreatedAt = new UInt64Wrapper { Value = (ulong) DateTime.Now.Ticks },
                Signature = null
            };

            var hash = User.GetHash(signInEventModel);

            Assert.NotNull(hash);
        }

        [Fact]
        public void GetHashValidDifferentHashesForDifferentInputs()
        {
            var keyPair1 = new Key();
            var keyPair2 = new Key();
            var tod = (ulong) DateTime.Now.Ticks;

            var signInEventModel1 = new SignInEventModel
            {
                Kind = new StringWrapper { Value = "1" },
                PubKey = new PubKeyWrapper { Value = keyPair1.PubKey.ToBytes() },
                CreatedAt = new UInt64Wrapper { Value = tod },
                Signature = null
            };

            var signInEventModel2 = new SignInEventModel
            {
                Kind = new StringWrapper { Value = "1" },
                PubKey = new PubKeyWrapper { Value = keyPair2.PubKey.ToBytes() },
                CreatedAt = new UInt64Wrapper { Value = tod },
                Signature = null
            };

            var hash1 = User.GetHash(signInEventModel1);
            var hash2 = User.GetHash(signInEventModel2);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void GetHashIgnoresSignatureProperty()
        {
            var keyPair = new Key();
            var tod = (ulong) DateTime.Now.Ticks;

            var signInEventModel1 = new SignInEventModel
            {
                Kind = new StringWrapper { Value = "1" },
                PubKey = new PubKeyWrapper { Value = keyPair.PubKey.ToBytes() },
                CreatedAt = new UInt64Wrapper { Value = tod },
                Signature = Encoding.UTF8.GetBytes("InvalidSignature")
            };

            var signInEventModel2 = new SignInEventModel
            {
                Kind = new StringWrapper { Value = "1" },
                PubKey = new PubKeyWrapper { Value = keyPair.PubKey.ToBytes() },
                CreatedAt = new UInt64Wrapper { Value = tod },
                Signature = null
            };

            var hash1 = User.GetHash(signInEventModel1);
            var hash2 = User.GetHash(signInEventModel2);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public async Task HandlesSignInEventModelError()
        {
            var signInEventModel = "blah";
            var requestData = new MockHttpRequestData(signInEventModel);

            var ret = await _userApi.SignIn(requestData);
            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSignInFailOnValidKeyInvalidSignature()
        {
            var keyPair = new Key();

            var signInEventModel = new SignInEventModel
            {
                Kind = new StringWrapper { Value = "1" },
                PubKey = new PubKeyWrapper { Value = keyPair.PubKey.ToBytes() },
                CreatedAt = new UInt64Wrapper { Value = (ulong) DateTime.Now.Ticks },
                Signature = Encoding.UTF8.GetBytes("InvalidSignature")
            };

            var serialized = JsonConvert.SerializeObject(signInEventModel);

            var requestData = new MockHttpRequestData(serialized);

            var ret = await _userApi.SignIn(requestData);

            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
            Assert.Contains("Invalid DER signature", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSignInFailOnInvalidKey()
        {
            var signInEventModel = new SignInEventModel
            {
                Kind = new StringWrapper { Value = "1" },
                PubKey = new PubKeyWrapper { Value = Encoding.UTF8.GetBytes("INVALID KEY") },
                CreatedAt = new UInt64Wrapper { Value = (ulong) DateTime.Now.Ticks },
                Signature = Encoding.UTF8.GetBytes("InvalidSignature")
            };

            var serialized = JsonConvert.SerializeObject(signInEventModel);

            var requestData = new MockHttpRequestData(serialized);

            var ret = await _userApi.SignIn(requestData);

            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
            Assert.Contains("Invalid public key", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task SignInSuccess()
        {
            var keyPair = new Key();
            var signInEventModel = new SignInEventModel
            {
                Kind = new StringWrapper { Value = "1" },
                PubKey = new PubKeyWrapper { Value = keyPair.PubKey.ToBytes() },
                CreatedAt = new UInt64Wrapper { Value = (ulong) DateTime.Now.Ticks },
            };

            var hash = User.GetHash(signInEventModel);
            var signature = keyPair.Sign(hash);
            signInEventModel.Signature = signature.ToDER();

            var serialized = JsonConvert.SerializeObject(signInEventModel);

            var requestData = new MockHttpRequestData(serialized);

            var ret = await _userApi.SignIn(requestData);

            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.OK, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
            Assert.NotEmpty(val.Value);

            // TODO Confirm valid token

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSignInFailOnValidKeyWrongSignature()
        {
            var keyPair = new Key();
            var keyPair2 = new Key();

            var signInEventModel = new SignInEventModel
            {
                Kind = new StringWrapper { Value = "1" },
                PubKey = new PubKeyWrapper { Value = keyPair.PubKey.ToBytes() },
                CreatedAt = new UInt64Wrapper { Value = (ulong) DateTime.Now.Ticks },
            };

            var hash = User.GetHash(signInEventModel);

            // Sign with a different key than the one passed in
            var signature = keyPair2.Sign(hash);
            signInEventModel.Signature = signature.ToDER();

            var serialized = JsonConvert.SerializeObject(signInEventModel);

            var requestData = new MockHttpRequestData(serialized);

            var ret = await _userApi.SignIn(requestData);

            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
            Assert.Contains("Signature did not verify", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}
