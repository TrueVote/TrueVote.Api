using HotChocolate.Types.Descriptors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Security.Claims;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Services;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.Helpers
{
    public class TestHelper
    {
        protected readonly ITestOutputHelper _output;
        protected readonly IFileSystem _fileSystem;
        protected readonly Mock<ILogger<LoggerHelper>> _logHelper;
        protected readonly User _userApi;
        protected readonly Election _electionApi;
        protected readonly Ballot _ballotApi;
        protected readonly Race _raceApi;
        protected readonly Candidate _candidateApi;
        protected readonly BallotValidator _validatorApi;
        protected readonly Timestamp _timestampApi;
        protected readonly Query _queryService;
        protected readonly MoqDataAccessor _moqDataAccessor;
        protected readonly Mock<IOpenTimestampsClient> _mockOpenTimestampsClient;
        protected readonly Mock<IServiceBus> _mockServiceBus;
        protected readonly Mock<IJwtHandler> _mockJwtHandler;
        protected readonly ControllerContext _authControllerContext;

        public const string MockedTokenValue = "mocked_token_value";

        public TestHelper(ITestOutputHelper output)
        {
            // This will override the setup shims in Startup.cs
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<ITrueVoteDbContext, MoqTrueVoteDbContext>();
            serviceCollection.TryAddScoped<IFileSystem, FileSystem>();
            serviceCollection.TryAddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.TryAddSingleton(typeof(ILogger), typeof(Logger<Startup>));
            serviceCollection.TryAddSingleton<IOpenTimestampsClient, OpenTimestampsClient>();
            serviceCollection.TryAddScoped<Query, Query>();
            serviceCollection.TryAddSingleton<INamingConventions, TrueVoteNamingConventions>();
            serviceCollection.TryAddScoped<IFileSystem, FileSystem>();
            serviceCollection.TryAddScoped<BallotValidator, BallotValidator>();
            serviceCollection.TryAddScoped<IServiceBus, ServiceBus>();
            serviceCollection.TryAddScoped<IJwtHandler, JwtHandler>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _output = output;

            _fileSystem = new FileSystem();

            _logHelper = new Mock<ILogger<LoggerHelper>>();
            _ = _logHelper.MockLog(LogLevel.Debug);
            _ = _logHelper.MockLog(LogLevel.Information);
            _ = _logHelper.MockLog(LogLevel.Warning);
            _ = _logHelper.MockLog(LogLevel.Error);

            _mockOpenTimestampsClient = new Mock<IOpenTimestampsClient>();
            _ = _mockOpenTimestampsClient.Setup(m => m.Stamp(It.IsAny<byte[]>())).Returns<byte[]>(hash => Task.FromResult(hash));

            _mockServiceBus = new Mock<IServiceBus>();
            _ = _mockServiceBus.Setup(m => m.SendAsync(It.IsAny<string>())).Returns(Task.FromResult(""));

            _mockJwtHandler = new Mock<IJwtHandler>();
            _ = _mockJwtHandler.Setup(m => m.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns((string userId, string npub, IEnumerable<string> roles) => MockedTokenValue);

            _moqDataAccessor = new MoqDataAccessor();
            _userApi = new User(_logHelper.Object, _moqDataAccessor.mockUserContext.Object, _mockServiceBus.Object, _mockJwtHandler.Object);
            _electionApi = new Election(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockServiceBus.Object);
            _validatorApi = new BallotValidator(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _mockOpenTimestampsClient.Object, _mockServiceBus.Object);
            _ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _validatorApi, _mockServiceBus.Object);
            _raceApi = new Race(_logHelper.Object, _moqDataAccessor.mockRaceContext.Object, _mockServiceBus.Object);
            _candidateApi = new Candidate(_logHelper.Object, _moqDataAccessor.mockCandidateContext.Object, _mockServiceBus.Object);
            _timestampApi = new Timestamp(_logHelper.Object, _moqDataAccessor.mockTimestampContext.Object);
            _queryService = new Query((MoqTrueVoteDbContext) serviceProvider.GetService(typeof(MoqTrueVoteDbContext)));

            // For endpoints that require Authorization [Authorize]
            // Mock a user principal with desired claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, MoqData.MockUserData[0].UserId),
                new Claim(ClaimTypes.Role, "User"), // Role
            };
            var identity = new ClaimsIdentity(claims, "TestAuthentication");
            var principal = new ClaimsPrincipal(identity);

            // Create context for controllers to attach to
            _authControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
}
