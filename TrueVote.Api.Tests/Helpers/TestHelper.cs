using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Services;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.Helpers
{
    public class TestHelper
    {
        protected readonly ITestOutputHelper _output;
        protected readonly HttpContext _httpContext;
        protected readonly IFileSystem _fileSystem;
        protected readonly Mock<ILogger<LoggerHelper>> logHelper;
        protected readonly User _userApi;
        protected readonly Election _electionApi;
        protected readonly Race _raceApi;
        protected readonly Candidate _candidateApi;
        public Mock<TelegramBot> mockTelegram = new Mock<TelegramBot>();

        public TestHelper(ITestOutputHelper output)
        {
            _output = output;
            _httpContext = new DefaultHttpContext();
            _fileSystem = new FileSystem();
            logHelper = new Mock<ILogger<LoggerHelper>>();
            logHelper.MockLog(LogLevel.Debug);
            logHelper.MockLog(LogLevel.Information);
            logHelper.MockLog(LogLevel.Warning);
            logHelper.MockLog(LogLevel.Error);

            mockTelegram.Setup(m => m.SendChannelMessageAsync(It.IsAny<string>())).ReturnsAsync(new Telegram.Bot.Types.Message());

            var mockUserSet = DbMoqHelper.GetDbSet(MoqData.MockUserData);
            var mockUserContext = new Mock<TrueVoteDbContext>();
            mockUserContext.Setup(m => m.Users).Returns(mockUserSet.Object);
            mockUserContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));
            _userApi = new User(logHelper.Object, mockUserContext.Object, mockTelegram.Object);

            var mockElectionSet = DbMoqHelper.GetDbSet(MoqData.MockElectionData);
            var mockElectionContext = new Mock<TrueVoteDbContext>();
            mockElectionContext.Setup(m => m.Elections).Returns(mockElectionSet.Object);
            mockElectionContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));
            _electionApi = new Election(logHelper.Object, mockElectionContext.Object, mockTelegram.Object);

            var mockRaceSet = DbMoqHelper.GetDbSet(MoqData.MockRaceData);
            var mockRaceContext = new Mock<TrueVoteDbContext>();
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);
            mockRaceContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));
            _raceApi = new Race(logHelper.Object, mockRaceContext.Object, mockTelegram.Object);

            var mockCandidateSet = DbMoqHelper.GetDbSet(MoqData.MockCandidateData);
            var mockCandidateContext = new Mock<TrueVoteDbContext>();
            mockCandidateContext.Setup(m => m.Candidates).Returns(mockCandidateSet.Object);
            mockCandidateContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));
            _candidateApi = new Candidate(logHelper.Object, mockCandidateContext.Object, mockTelegram.Object);
        }
    }
}
