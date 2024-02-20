using HotChocolate.AzureFunctions;
using HotChocolate.Types.Descriptors;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO.Abstractions;
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
        protected readonly Validator _validatorApi;
        protected readonly Timestamp _timestampApi;
        protected readonly GraphQLExecutor _graphQLApi;
        protected readonly MoqDataAccessor _moqDataAccessor;
        protected readonly Mock<IOpenTimestampsClient> _mockOpenTimestampsClient;
        protected readonly Mock<IServiceBus> _mockServiceBus;
        protected readonly Mock<IJwtHandler> _mockJwtHandler;
        protected readonly IGraphQLRequestExecutor requestExecutor;
        private static readonly FunctionContext Context = Mock.Of<FunctionContext>();

        public TestHelper(ITestOutputHelper output)
        {
            // This will override the setup shims in Startup.cs
            var serviceCollection = new ServiceCollection();

            _ = serviceCollection.AddDbContext<ITrueVoteDbContext, MoqTrueVoteDbContext>();
            serviceCollection.TryAddScoped<IFileSystem, FileSystem>();
            serviceCollection.TryAddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.TryAddSingleton(typeof(ILogger), typeof(Logger<Startup>));
            serviceCollection.TryAddSingleton<IOpenTimestampsClient, OpenTimestampsClient>();
            serviceCollection.TryAddScoped<Query, Query>();
            serviceCollection.TryAddSingleton<INamingConventions, TrueVoteNamingConventions>();
            serviceCollection.TryAddScoped<IFileSystem, FileSystem>();
            _ = serviceCollection.AddGraphQLFunction().AddQueryType<Query>();
            serviceCollection.TryAddScoped<Validator, Validator>();
            serviceCollection.TryAddScoped<IServiceBus, ServiceBus>();
            serviceCollection.TryAddScoped<IJwtHandler, JwtHandler>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            requestExecutor = serviceProvider.GetRequiredService<IGraphQLRequestExecutor>();

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
            _ = _mockJwtHandler.Setup(m => m.GenerateToken(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns((string userId, IEnumerable<string> roles) => "mocked_token_value");

            _ = _mockJwtHandler.Setup(m => m.ProcessTokenValidationAsync(It.IsAny<HttpRequestData>()))
                .ReturnsAsync((HttpRequestData req) => (new MockHttpResponseData(Context), "mocked_renewed_token"));

            _moqDataAccessor = new MoqDataAccessor();
            _userApi = new User(_logHelper.Object, _moqDataAccessor.mockUserContext.Object, _mockServiceBus.Object, _mockJwtHandler.Object);
            _electionApi = new Election(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockServiceBus.Object);
            _validatorApi = new Validator(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _mockOpenTimestampsClient.Object, _mockServiceBus.Object);
            _ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _validatorApi, _mockServiceBus.Object);
            _raceApi = new Race(_logHelper.Object, _moqDataAccessor.mockRaceContext.Object, _mockServiceBus.Object);
            _candidateApi = new Candidate(_logHelper.Object, _moqDataAccessor.mockCandidateContext.Object, _mockServiceBus.Object);
            _graphQLApi = new GraphQLExecutor(_logHelper.Object, requestExecutor, _mockServiceBus.Object);
            _timestampApi = new Timestamp(_logHelper.Object, _moqDataAccessor.mockTimestampContext.Object, _mockServiceBus.Object);
        }
    }
}
