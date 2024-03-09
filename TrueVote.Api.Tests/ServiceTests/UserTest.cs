/*
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using System;
using System.Collections.Generic;
using System.Net;
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
            _ = Assert.IsType<DateTime>(val.DateCreated);
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

            var userApi = new User(_logHelper.Object, _moqDataAccessor.mockUserContext.Object, _mockServiceBus.Object, _mockJwtHandler.Object);

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

            var userApi = new User(_logHelper.Object, _moqDataAccessor.mockUserContext.Object, _mockServiceBus.Object, _mockJwtHandler.Object);

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
            var utcTime = DateTimeOffset.UtcNow;
            var keyPair = NostrKeyPair.GenerateNew();

            var signInEventModel = new SignInEventModel
            {
                Kind = NostrKind.ShortTextNote,
                PubKey = keyPair.PublicKey.Bech32,
                CreatedAt = utcTime.DateTime,
                Signature = "INVALID SIG",
                Content = ""
            };

            var serialized = JsonConvert.SerializeObject(signInEventModel);

            var requestData = new MockHttpRequestData(serialized);

            var ret = await _userApi.SignIn(requestData);

            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
            Assert.Contains("The binary key cannot have an odd number of digits", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSignInFailOnInvalidKey()
        {
            var utcTime = DateTimeOffset.UtcNow;

            var signInEventModel = new SignInEventModel
            {
                Kind = NostrKind.ShortTextNote,
                PubKey = "INVALID KEY",
                CreatedAt = utcTime.DateTime,
                Signature = "INVALID SIG",
                Content = ""
            };

            var serialized = JsonConvert.SerializeObject(signInEventModel);

            var requestData = new MockHttpRequestData(serialized);

            var ret = await _userApi.SignIn(requestData);

            Assert.NotNull(ret);
            Assert.Equal(HttpStatusCode.BadRequest, ret.StatusCode);
            var val = await ret.ReadAsJsonAsync<SecureString>();
            Assert.Contains("Provided bech32 key is not 'npub'", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task SignInSuccess()
        {
            var keyPair = NostrKeyPair.GenerateNew();
            var utcTime = DateTimeOffset.UtcNow;

            // Simulate a client (e.g. TypeScript)
            var nostrEvent = new NostrEvent
            {
                Kind = NostrKind.ShortTextNote,
                CreatedAt = utcTime.DateTime,
                Pubkey = keyPair.PublicKey.Hex,
                Content = "SIGNIN"
            };
            var signature = nostrEvent.Sign(keyPair.PrivateKey);
            var valid = signature.IsSignatureValid();
            Assert.True(valid);

            // Package it up into a TrueVote model the way the client side TypeScript would
            var signInEventModel = new SignInEventModel
            {
                Kind = nostrEvent.Kind,
                PubKey = keyPair.PublicKey.Bech32,
                CreatedAt = utcTime.DateTime,
                Content = nostrEvent.Content,
                Signature = signature.Sig
            };

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
            var keyPair = NostrKeyPair.GenerateNew();
            var keyPair2 = NostrKeyPair.GenerateNew();
            var utcTime = DateTimeOffset.UtcNow;

            // Simulate a client (e.g. TypeScript)
            var nostrEvent = new NostrEvent
            {
                Kind = NostrKind.ShortTextNote,
                CreatedAt = utcTime.DateTime,
                Pubkey = keyPair.PublicKey.Hex,
                Content = "SIGNIN"
            };
            var signature = nostrEvent.Sign(keyPair.PrivateKey);
            var valid = signature.IsSignatureValid();
            Assert.True(valid);

            var signature2 = nostrEvent.Sign(keyPair2.PrivateKey);
            var valid2 = signature2.IsSignatureValid();
            Assert.True(valid2);

            // Package it up into a TrueVote model the way the client side TypeScript would
            var signInEventModel = new SignInEventModel
            {
                Kind = nostrEvent.Kind,
                PubKey = keyPair.PublicKey.Bech32,
                CreatedAt = utcTime.DateTime,
                Content = nostrEvent.Content,
                Signature = signature2.Sig
            };

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
*/
