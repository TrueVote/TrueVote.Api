using HotChocolate.AzureFunctions;
using HotChocolate.Types.Descriptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
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
        protected readonly Mock<TelegramBot> _mockTelegram;
        protected readonly Mock<IOpenTimestampsClient> _mockOpenTimestampsClient;
        protected readonly IGraphQLRequestExecutor requestExecutor;

        public TestHelper(ITestOutputHelper output)
        {
            // This will override the setup shims in Startup.cs
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<ITrueVoteDbContext, MoqTrueVoteDbContext>();
            serviceCollection.TryAddScoped<IFileSystem, FileSystem>();
            serviceCollection.TryAddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.TryAddSingleton(typeof(ILogger), typeof(Logger<Startup>));
            serviceCollection.TryAddSingleton<TelegramBot, TelegramBot>();
            serviceCollection.TryAddSingleton<IOpenTimestampsClient, OpenTimestampsClient>();
            serviceCollection.TryAddScoped<Query, Query>();
            serviceCollection.TryAddSingleton<INamingConventions, TrueVoteNamingConventions>();
            serviceCollection.TryAddScoped<IFileSystem, FileSystem>();
            serviceCollection.AddGraphQLFunction().AddQueryType<Query>();
            serviceCollection.TryAddScoped<Validator, Validator>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            requestExecutor = serviceProvider.GetRequiredService<IGraphQLRequestExecutor>();

            _output = output;

            _fileSystem = new FileSystem();

            _logHelper = new Mock<ILogger<LoggerHelper>>();
            _logHelper.MockLog(LogLevel.Debug);
            _logHelper.MockLog(LogLevel.Information);
            _logHelper.MockLog(LogLevel.Warning);
            _logHelper.MockLog(LogLevel.Error);

            _mockTelegram = new Mock<TelegramBot>();
            _mockTelegram.Setup(m => m.SendChannelMessageAsync(It.IsAny<string>())).ReturnsAsync(new Telegram.Bot.Types.Message());

            _mockOpenTimestampsClient = new Mock<IOpenTimestampsClient>();
            _mockOpenTimestampsClient.Setup(m => m.Stamp(It.IsAny<byte[]>())).Returns<byte[]>(hash => Task.FromResult(hash));

            _moqDataAccessor = new MoqDataAccessor();
            _userApi = new User(_logHelper.Object, _moqDataAccessor.mockUserContext.Object, _mockTelegram.Object);
            _electionApi = new Election(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockTelegram.Object);
            _validatorApi = new Validator(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _mockTelegram.Object, _mockOpenTimestampsClient.Object);
            _ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _mockTelegram.Object, _validatorApi);
            _raceApi = new Race(_logHelper.Object, _moqDataAccessor.mockRaceContext.Object, _mockTelegram.Object);
            _candidateApi = new Candidate(_logHelper.Object, _moqDataAccessor.mockCandidateContext.Object, _mockTelegram.Object);
            _graphQLApi = new GraphQLExecutor(_logHelper.Object, _mockTelegram.Object, requestExecutor);
            _timestampApi = new Timestamp(_logHelper.Object, _moqDataAccessor.mockTimestampContext.Object, _mockTelegram.Object);
        }
    }
}
