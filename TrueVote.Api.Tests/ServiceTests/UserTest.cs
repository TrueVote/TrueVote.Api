using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class FakeBaseUserModel
    {
        public string FullName { get; set; } = string.Empty;
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
            var baseUserObj = new BaseUserModel { FullName = "Joe Blow", Email = "joe@joe.com", NostrPubKey = "nostr-key" };
            var validationResults = ValidationHelper.Validate(baseUserObj);
            Assert.Empty(validationResults);

            _ = await _userApi.CreateUser(baseUserObj);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task AddsUser()
        {
            var baseUserObj = new BaseUserModel { FullName = "Joe Blow", Email = "joe@joe.com", NostrPubKey = "nostr-key" };
            var validationResults = ValidationHelper.Validate(baseUserObj);
            Assert.Empty(validationResults);

            var ret = await _userApi.CreateUser(baseUserObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status201Created, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (UserModel) (ret as CreatedAtActionResult).Value;
            Assert.NotNull(val);

            _output.WriteLine($"Item: {val}");

            _output.WriteLine($"Item.FirstName: {val.FullName}");
            _output.WriteLine($"Item.Email: {val.Email}");
            _output.WriteLine($"Item.DateCreated: {val.DateCreated}");
            _output.WriteLine($"Item.UserId: {val.UserId}");

            Assert.Equal("Joe Blow", val.FullName);
            Assert.Equal("joe@joe.com", val.Email);
            _ = Assert.IsType<DateTime>(val.DateCreated);
            Assert.NotEmpty(val.UserId);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task FindsUser()
        {
            var findUserObj = new FindUserModel { FullName = "Foo Bar", Email = "foo@foo.com" };
            var validationResults = ValidationHelper.Validate(findUserObj);
            Assert.Empty(validationResults);

            var ret = await _userApi.UserFind(findUserObj);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (UserModelList) (ret as OkObjectResult).Value;
            Assert.NotEmpty(val.Users);
            Assert.Single(val.Users);
            Assert.Equal("Foo Bar", val.Users[0].FullName);
            Assert.Equal("foo@foo.com", val.Users[0].Email);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUnfoundUser()
        {
            var findUserObj = new FindUserModel { FullName = "not going to find anything", Email = "foo@whatever.com" };
            var validationResults = ValidationHelper.Validate(findUserObj);
            Assert.Empty(validationResults);

            var ret = await _userApi.UserFind(findUserObj);
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
                Content = "CONTENT"
            };
            var validationResults = ValidationHelper.Validate(signInEventModel);
            Assert.Empty(validationResults);

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
                Content = "CONTENT"
            };
            var validationResults = ValidationHelper.Validate(signInEventModel);
            Assert.Empty(validationResults);

            var ret = await _userApi.SignIn(signInEventModel);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status400BadRequest, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as BadRequestObjectResult).Value;
            Assert.Contains("Provided bech32 key is not 'npub'", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task SignInSuccessUnfoundUser()
        {
            var keyPair = NostrKeyPair.GenerateNew();
            var utcTime = DateTimeOffset.UtcNow;

            // Simulate a client (e.g. TypeScript)
            var content = new BaseUserModel
            {
                Email = "unknown@truevote.org",
                FullName = "Joe Blow",
                NostrPubKey = keyPair.PublicKey.Bech32,
            };
            var validationResults = ValidationHelper.Validate(content);
            Assert.Empty(validationResults);

            var nostrEvent = new NostrEvent
            {
                Kind = NostrKind.ShortTextNote,
                CreatedAt = utcTime.DateTime,
                Pubkey = keyPair.PublicKey.Hex,
                Content = JsonConvert.SerializeObject(content)
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
            var validationResults2 = ValidationHelper.Validate(signInEventModel);
            Assert.Empty(validationResults2);

            var ret = await _userApi.SignIn(signInEventModel);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SignInResponse) (ret as OkObjectResult).Value;
            Assert.NotEmpty(val.Token);
            Assert.NotNull(val.User);
            Assert.Equal(MockedTokenValue, val.Token);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task SignInSuccessFoundUser()
        {
            var keyPair = NostrKeyPair.GenerateNew();
            var userId = Guid.NewGuid().ToString();
            var utcTime = DateTimeOffset.UtcNow;

            var newUser = new UserModel
            {
                UserId = userId,
                DateCreated = utcTime.DateTime,
                DateUpdated = utcTime.DateTime,
                Email = "foo4@bar.com",
                FullName = "Foo Bar",
                NostrPubKey = keyPair.PublicKey.Bech32,
                UserPreferences = new UserPreferencesModel()
            };
            var validationResults = ValidationHelper.Validate(newUser);
            Assert.Empty(validationResults);

            // Create own MoqData so it finds it later below
            var mockUserData = new List<UserModel> { newUser };
            var mockUserContext = new Mock<MoqTrueVoteDbContext>();
            var mockUserDataQueryable = mockUserData.AsQueryable();
            var mockUserDataCollection = mockUserData;
            var MockUserSet = DbMoqHelper.GetDbSet(mockUserDataQueryable);
            mockUserContext.Setup(m => m.Users).Returns(MockUserSet.Object);

            // Simulate a client (e.g. TypeScript)
            var content = new BaseUserModel
            {
                Email = newUser.Email,
                FullName = newUser.FullName,
                NostrPubKey = keyPair.PublicKey.Bech32,
            };
            var nostrEvent = new NostrEvent
            {
                Kind = NostrKind.ShortTextNote,
                CreatedAt = utcTime.DateTime,
                Pubkey = keyPair.PublicKey.Hex,
                Content = JsonConvert.SerializeObject(content)
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
            var validationResults2 = ValidationHelper.Validate(signInEventModel);
            Assert.Empty(validationResults2);

            var userApi = new User(_logHelper.Object, mockUserContext.Object, _mockServiceBus.Object, _mockJwtHandler.Object);
            var ret = await userApi.SignIn(signInEventModel);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SignInResponse) (ret as OkObjectResult).Value;
            Assert.NotEmpty(val.Token);
            Assert.NotNull(val.User);
            Assert.Equal(MockedTokenValue, val.Token);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSignInSuccessFoundUserInvalidContent()
        {
            var keyPair = NostrKeyPair.GenerateNew();
            var utcTime = DateTimeOffset.UtcNow;

            // Simulate a client (e.g. TypeScript)
            var nostrEvent = new NostrEvent
            {
                Kind = NostrKind.ShortTextNote,
                CreatedAt = utcTime.DateTime,
                Pubkey = keyPair.PublicKey.Hex,
                Content = "INVALID CONTENT - NOT A PROPER BaseUserModel"
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
            var validationResults = ValidationHelper.Validate(signInEventModel);
            Assert.Empty(validationResults);

            var ret = await _userApi.SignIn(signInEventModel);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status400BadRequest, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as BadRequestObjectResult).Value;
            Assert.Contains("Could not deserialize signature content", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
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
            var validationResults = ValidationHelper.Validate(signInEventModel);
            Assert.Empty(validationResults);

            var ret = await _userApi.SignIn(signInEventModel);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status400BadRequest, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as BadRequestObjectResult).Value;
            Assert.Contains("Signature did not verify", val.Value.ToString());

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task SavesUser()
        {
            var user = MoqData.MockUserData[0];
            user.FullName = "Joe Jones";
            Assert.Equal(DateTime.MinValue, user.DateUpdated);
            Assert.Equal("Joe Jones", user.FullName);
            var validationResults = ValidationHelper.Validate(user);
            Assert.Empty(validationResults);

            _userApi.ControllerContext = _authControllerContext;
            var ret = await _userApi.SaveUser(user);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var updatedUser = (UserModel) (ret as OkObjectResult).Value;

            Assert.True(UtcNowProviderFactory.GetProvider().UtcNow - updatedUser.DateUpdated <= TimeSpan.FromSeconds(3));
            Assert.Equal("Joe Jones", updatedUser.FullName);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task SavesUserWithNewEmail()
        {
            var user = MoqData.MockUserData[0];
            user.FullName = "Joe Jones";
            user.Email = "anewemail@anywhere.com";
            Assert.Equal(DateTime.MinValue, user.DateUpdated);
            Assert.Equal("Joe Jones", user.FullName);
            var validationResults = ValidationHelper.Validate(user);
            Assert.Empty(validationResults);

            _userApi.ControllerContext = _authControllerContext;
            var ret = await _userApi.SaveUser(user);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var updatedUser = (UserModel) (ret as OkObjectResult).Value;

            Assert.True(UtcNowProviderFactory.GetProvider().UtcNow - updatedUser.DateUpdated <= TimeSpan.FromSeconds(3));
            Assert.Equal("Joe Jones", updatedUser.FullName);
            Assert.Equal("anewemail@anywhere.com", updatedUser.Email);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(2));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSavesUserWithoutAuthorization()
        {
            var user = MoqData.MockUserData[0];
            user.FullName = "Joe Jones";
            Assert.Equal(DateTime.MinValue, user.DateUpdated);
            Assert.Equal("Joe Jones", user.FullName);
            var validationResults = ValidationHelper.Validate(user);
            Assert.Empty(validationResults);

            var ret = await _userApi.SaveUser(user);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status401Unauthorized, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSavesUserUserNotFound()
        {
            var user = MoqData.MockUserData[0];
            user.UserId = "blah1";
            user.FullName = "Joe Jones";
            Assert.Equal(DateTime.MinValue, user.DateUpdated);
            Assert.Equal("Joe Jones", user.FullName);
            var validationResults = ValidationHelper.Validate(user);
            Assert.Empty(validationResults);

            _userApi.ControllerContext = _authControllerContext;
            var ret = await _userApi.SaveUser(user);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task SavesFeedback()
        {
            var feedback = MoqData.MockFeedbackData[0];
            var validationResults = ValidationHelper.Validate(feedback);
            Assert.Empty(validationResults);

            _userApi.ControllerContext = _authControllerContext;
            var ret = await _userApi.SaveFeedback(feedback);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var res = (SecureString) (ret as OkObjectResult).Value;

            Assert.Equal("Success", res.Value);

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSavesFeedbackUserNotFound()
        {
            var feedback = MoqData.MockFeedbackData[0];
            feedback.UserId = "blah";
            var validationResults = ValidationHelper.Validate(feedback);
            Assert.Empty(validationResults);

            _userApi.ControllerContext = _authControllerContext;
            var ret = await _userApi.SaveFeedback(feedback);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesSavesFeedbackWithoutAuthorization()
        {
            var feedback = MoqData.MockFeedbackData[0];
            var validationResults = ValidationHelper.Validate(feedback);
            Assert.Empty(validationResults);

            var ret = await _userApi.SaveFeedback(feedback);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status401Unauthorized, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public void EmailShouldReturnDefaultValueWhenNotSet()
        {
            var userModel = new BaseUserModel
            {
                FullName = "John Doe",
                NostrPubKey = "some-public-key",
                Email = ""
            };

            var email = userModel.Email;
            Assert.Equal("unknown@truevote.org", email);
        }
    }
}
