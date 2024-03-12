using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using System;
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

            _ = await _userApi.CreateUser(baseUserObj);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsUser()
        {
            var baseUserObj = new BaseUserModel { FirstName = "Joe", Email = "joe@joe.com" };

            var ret = await _userApi.CreateUser(baseUserObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status201Created, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (UserModel) (ret as CreatedAtActionResult).Value;
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
        public async Task FindsUser()
        {
            var findUserObj = new FindUserModel { FirstName = "Foo" };

            var userApi = new User(_logHelper.Object, _moqDataAccessor.mockUserContext.Object, _mockServiceBus.Object, _mockJwtHandler.Object);

            var ret = await userApi.UserFind(findUserObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (UserModelList) (ret as OkObjectResult).Value;
            Assert.NotEmpty(val.Users);
            Assert.Equal(2, val.Users.Count);
            Assert.Equal("Foo2", val.Users[0].FirstName);
            Assert.Equal("foo2@bar.com", val.Users[0].Email);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundUser()
        {
            var findUserObj = new FindUserModel { FirstName = "not going to find anything" };

            var userApi = new User(_logHelper.Object, _moqDataAccessor.mockUserContext.Object, _mockServiceBus.Object, _mockJwtHandler.Object);

            var ret = await userApi.UserFind(findUserObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
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

            var ret = await _userApi.SignIn(signInEventModel);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status400BadRequest, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as BadRequestObjectResult).Value;
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

            var ret = await _userApi.SignIn(signInEventModel);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status400BadRequest, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as BadRequestObjectResult).Value;
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

            var ret = await _userApi.SignIn(signInEventModel);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as OkObjectResult).Value;
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

            var ret = await _userApi.SignIn(signInEventModel);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status400BadRequest, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as BadRequestObjectResult).Value;
            Assert.Contains("Signature did not verify", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}
