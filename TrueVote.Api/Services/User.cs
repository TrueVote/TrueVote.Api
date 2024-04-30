using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using System.ComponentModel;
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
        public async Task<IActionResult> CreateUser([FromBody] BaseUserModel baseUser)
        {
            _log.LogDebug("HTTP trigger - CreateUser:Begin");

            _log.LogInformation($"Request Data: {baseUser}");

            var user = await AddNewUser(baseUser, Guid.NewGuid().ToString());

            _log.LogDebug("HTTP trigger - CreateUser:End");

            return CreatedAtAction(null, null, user);
        }

        [HttpGet]
        [Route("user/find")]
        [Produces(typeof(UserModelList))]
        [Description("Returns collection of Users")]
        [ProducesResponseType(typeof(UserModelList), StatusCodes.Status200OK)]
        public async Task<IActionResult> UserFind([FromBody] FindUserModel findUser)
        {
            _log.LogDebug("HTTP trigger - UserFind:Begin");

            _log.LogInformation($"Request Data: {findUser}");

            // TODO Simplify this query by putting the and conditions in an extension methods to build the where clause more idiomatically. It should iterate
            // through all the properties in FindUserModel and build the .Where clause dynamically.
            var items = new UserModelList
            {
                Users = await _trueVoteDbContext.Users
                .Where(u =>
                    (findUser.FullName == null || (u.FullName ?? string.Empty).ToLower().Contains(findUser.FullName.ToLower())) &&
                    (findUser.Email == null || (u.Email ?? string.Empty).ToLower().Contains(findUser.Email.ToLower())))
                .OrderByDescending(u => u.DateCreated).ToListAsync()
            };

            _log.LogDebug("HTTP trigger - UserFind:End");

            return items.Users.Count == 0 ? NotFound() : Ok(items);
        }

        [HttpPost]
        [Route("user/signin")]
        [Produces(typeof(SecureString))]
        [Description("Signs In a User and returns a Token")]
        [ProducesResponseType(typeof(SecureString), StatusCodes.Status200OK)]
        public async Task<IActionResult> SignIn([FromBody] SignInEventModel signInEventModel)
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

                return BadRequest(new SecureString { Value = e.Message });
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

                return BadRequest(new SecureString { Value = e.Message });
            }

            if (!isValid)
            {
                _log.LogError("SignIn: invalid signature");
                _log.LogDebug("HTTP trigger - SignIn:End");

                return BadRequest(new SecureString { Value = "Signature did not verify" });
            }

            // Find the user by PubKey
            var items = new UserModelList
            {
                Users = await _trueVoteDbContext.Users
                .Where(u => u.NostrPubKey == signInEventModel.PubKey)
                .OrderByDescending(u => u.DateCreated).ToListAsync()
            };

            string userId;

            // If the user isn't found, create a new user record
            if (items.Users.Count == 0)
            {
                BaseUserModel baseUserModel;

                // DeSerialize the signed content into model
                try
                {
                    baseUserModel = JsonConvert.DeserializeObject<BaseUserModel>(signInEventModel.Content);
                }
                catch (Exception e)
                {
                    _log.LogError($"SignIn: deserialization error: {e.Message}");
                    _log.LogDebug("HTTP trigger - SignIn:End");

                    return BadRequest(new SecureString { Value = "Could not deserialize signature content into BaseUserModel" });
                }

                userId = Guid.NewGuid().ToString();

                await AddNewUser(new BaseUserModel { Email = baseUserModel.Email, FullName = baseUserModel.FullName, NostrPubKey = signInEventModel.PubKey }, userId);
            }
            else
            {
                userId = items.Users.FirstOrDefault().UserId;
            }

            // TODO ["User"] is the only "role" for now. Here's where we could assign an elevated role
            var token = _jwtHandler.GenerateToken(userId, signInEventModel.PubKey, ["User"]);

            _log.LogDebug("HTTP trigger - SignIn:End");

            return Ok(new SecureString { Value = token });
        }

        private async Task<UserModel> AddNewUser(BaseUserModel baseUser, string userId)
        {
            var user = new UserModel { FullName = baseUser.FullName, Email = baseUser.Email, UserId = userId, NostrPubKey = baseUser.NostrPubKey, DateCreated = UtcNowProviderFactory.GetProvider().UtcNow };

            await _trueVoteDbContext.EnsureCreatedAsync();

            await _trueVoteDbContext.Users.AddAsync(user);
            await _trueVoteDbContext.SaveChangesAsync();

            await _serviceBus.SendAsync($"New TrueVote User created: {user.FullName}");

            return user;
        }

        [HttpPut]
        [Authorize]
        [ServiceFilter(typeof(ValidateUserIdFilter))]
        [Route("user/saveuser")]
        [Produces(typeof(UserModel))]
        [Description("Saves an existing User preferences and returns the same updated User")]
        [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveUser([FromBody] UserModel user)
        {
            _log.LogDebug("HTTP trigger - SaveUser:Begin");

            _log.LogInformation($"Request Data: {user}");

            if (User == null || User.Identity == null)
            {
                _log.LogDebug("HTTP trigger - SaveUser:End");
                return Unauthorized();
            }

            // Determine if User is found
            var foundUser = await _trueVoteDbContext.Users.Where(u => u.UserId == user.UserId).FirstOrDefaultAsync();
            if (foundUser == null)
            {
                _log.LogDebug("HTTP trigger - SaveUser:End");
                return NotFound();
            }

            foundUser.FullName = user.FullName;

            // TODO Confirm Preferences saved too

            foundUser.DateUpdated = UtcNowProviderFactory.GetProvider().UtcNow;

            // TODO Add Versioning for each update
            _trueVoteDbContext.Users.Update(foundUser);
            await _trueVoteDbContext.SaveChangesAsync();

            await _serviceBus.SendAsync($"TrueVote User updated: {foundUser.FullName}");

            _log.LogDebug("HTTP trigger - SaveUser:End");

            return Ok(foundUser);
        }
    }
}
