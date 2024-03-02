using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using System.ComponentModel;
using System.Net.Http.Formatting;
using System.Net;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(SecureString), StatusCodes.Status409Conflict)]
    public class User : ControllerBase
    {
        private readonly ILogger _log;
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly IServiceBus _serviceBus;
        private readonly IJwtHandler _jwtHandler;

        public User(ILogger log, ITrueVoteDbContext trueVoteDbContext, IServiceBus serviceBus, IJwtHandler jwtHandler)
        {
            _log = log;
            _trueVoteDbContext = trueVoteDbContext;
            _serviceBus = serviceBus;
            _jwtHandler = jwtHandler;
        }

        [HttpPost]
        [Route("user")]
        [Produces(typeof(UserModel))]
        [Description("Creates a new User and returns the added User")]
        [ProducesResponseType(typeof(UserModel), StatusCodes.Status201Created)]
        public async Task<HttpResponseMessage> CreateUser([FromBody] BaseUserModel baseUser)
        {
            _log.LogDebug("HTTP trigger - CreateUser:Begin");

            _log.LogInformation($"Request Data: {baseUser}");

            var user = new UserModel { FirstName = baseUser.FirstName, Email = baseUser.Email };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Users.AddAsync(user);
            await _trueVoteDbContext.SaveChangesAsync();

            await _serviceBus.SendAsync($"New TrueVote User created: {user.FirstName}");

            _log.LogDebug("HTTP trigger - CreateUser:End");

            return new HttpResponseMessage { StatusCode = HttpStatusCode.Created, Content = new ObjectContent<UserModel>(user, new JsonMediaTypeFormatter()) };
        }

        [HttpGet]
        [Route("user/find")]
        [Produces(typeof(UserModelList))]
        [Description("Returns collection of Users")]
        [ProducesResponseType(typeof(UserModelList), StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> UserFind([FromBody] FindUserModel findUser)
        {
            _log.LogDebug("HTTP trigger - UserFind:Begin");

            _log.LogInformation($"Request Data: {findUser}");

            // TODO Simplify this query by putting the and conditions in an extension methods to build the where clause more idiomatically. It should iterate
            // through all the properties in FindUserModel and build the .Where clause dynamically.
            var items = new UserModelList
            {
                Users = await _trueVoteDbContext.Users
                .Where(u =>
                    (findUser.FirstName == null || (u.FirstName ?? string.Empty).ToLower().Contains(findUser.FirstName.ToLower())) &&
                    (findUser.Email == null || (u.Email ?? string.Empty).ToLower().Contains(findUser.Email.ToLower())))
                .OrderByDescending(u => u.DateCreated).ToListAsync()
            };

            _log.LogDebug("HTTP trigger - UserFind:End");

            return items.Users.Count == 0 ? new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound } : new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new ObjectContent<UserModelList>(items, new JsonMediaTypeFormatter()) };
        }

        [HttpPost]
        [Route("user/signin")]
        [Produces(typeof(SecureString))]
        [Description("Signs In a User and returns a Token")]
        [ProducesResponseType(typeof(SecureString), StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> SignIn([FromBody] SignInEventModel signInEventModel)
        {
            _log.LogDebug("HTTP trigger - SignIn:Begin");

            _log.LogInformation($"Request Data: {signInEventModel}");

            NostrPublicKey publicKey;
            try
            {
                publicKey = NostrPublicKey.FromBech32(signInEventModel.PubKey);
            }
            catch (Exception e)
            {
                _log.LogError($"SignIn: publicKey resolver failure: {e.Message}");
                _log.LogDebug("HTTP trigger - SignIn:End");

                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new ObjectContent<SecureString>(new SecureString { Value = e.Message }, new JsonMediaTypeFormatter()) };
            }

            bool isValid;
            try
            {
                // Create the Nostr Event same as the client did
                var nostrEvent = new NostrEvent
                {
                    Kind = signInEventModel.Kind,
                    CreatedAt = signInEventModel.CreatedAt,
                    Pubkey = publicKey.Hex,
                    Content = signInEventModel.Content,
                    Sig = signInEventModel.Signature
                };

                isValid = nostrEvent.IsSignatureValid();
            }
            catch (Exception e)
            {
                _log.LogError($"SignIn: Verification exception: {e.Message}");
                _log.LogDebug("HTTP trigger - SignIn:End");

                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new ObjectContent<SecureString>(new SecureString { Value = e.Message }, new JsonMediaTypeFormatter()) };
            }

            if (!isValid)
            {
                _log.LogError("SignIn: invalid signature");
                _log.LogDebug("HTTP trigger - SignIn:End");

                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new ObjectContent<SecureString>(new SecureString { Value = "Signature did not verify" }, new JsonMediaTypeFormatter()) };
            }

            // TODO - Find the user by PubKey

            // TODO - SignIn the user and return token for API access
            // TODO - Need to use TrueVote UserID here
            var token = _jwtHandler.GenerateToken(signInEventModel.PubKey, ["User"]);

            _log.LogDebug("HTTP trigger - SignIn:End");

            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new ObjectContent<SecureString>(new SecureString { Value = token }, new JsonMediaTypeFormatter()) };
        }
    }
}
