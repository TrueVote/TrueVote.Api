using HotChocolate.AzureFunctions;
using HotChocolate.Types.Descriptors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.IO.Abstractions;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Services;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.Helpers
{
    public class TestHelper
    {
        protected readonly ITestOutputHelper _output;
        protected readonly HttpContext _httpContext;
        protected readonly IFileSystem _fileSystem;
        protected readonly Mock<ILogger<LoggerHelper>> _logHelper;
        protected readonly User _userApi;
        protected readonly Election _electionApi;
        protected readonly Ballot _ballotApi;
        protected readonly Race _raceApi;
        protected readonly Candidate _candidateApi;
        protected readonly GraphQLExecutor _graphQLApi;
        protected readonly MoqDataAccessor _moqDataAccessor;
        protected readonly Mock<TelegramBot> _mockTelegram;
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
            serviceCollection.TryAddScoped<Query, Query>();
            serviceCollection.TryAddSingleton<INamingConventions, TrueVoteNamingConventions>();
            serviceCollection.AddGraphQLFunction().AddQueryType<Query>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            requestExecutor = serviceProvider.GetRequiredService<IGraphQLRequestExecutor>();

            _output = output;

            _httpContext = new DefaultHttpContext();
            _httpContext.Request.ContentType = "application/json";
            // https://stackoverflow.com/questions/59159565/initializing-defaulthttpcontext-response-body-to-memorystream-throws-nullreferen
            _httpContext.Features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(new MemoryStream()));

            _fileSystem = new FileSystem();

            _logHelper = new Mock<ILogger<LoggerHelper>>();
            _logHelper.MockLog(LogLevel.Debug);
            _logHelper.MockLog(LogLevel.Information);
            _logHelper.MockLog(LogLevel.Warning);
            _logHelper.MockLog(LogLevel.Error);

            _mockTelegram = new Mock<TelegramBot>();
            _mockTelegram.Setup(m => m.SendChannelMessageAsync(It.IsAny<string>())).ReturnsAsync(new Telegram.Bot.Types.Message());

            _moqDataAccessor = new MoqDataAccessor();
            _userApi = new User(_logHelper.Object, _moqDataAccessor.mockUserContext.Object, _mockTelegram.Object);
            _electionApi = new Election(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockTelegram.Object);
            _ballotApi = new Ballot(_logHelper.Object, _moqDataAccessor.mockElectionContext.Object, _mockTelegram.Object);
            _raceApi = new Race(_logHelper.Object, _moqDataAccessor.mockRaceContext.Object, _mockTelegram.Object);
            _candidateApi = new Candidate(_logHelper.Object, _moqDataAccessor.mockCandidateContext.Object, _mockTelegram.Object);
            _graphQLApi = new GraphQLExecutor(_logHelper.Object, _mockTelegram.Object);
        }
    }
}
