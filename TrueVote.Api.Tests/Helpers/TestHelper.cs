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
        protected readonly Mock<ILogger<LoggerHelper>> _log;
        protected readonly User _userApi;
        protected readonly Election _electionApi;
        protected readonly Race _raceApi;
        protected readonly Candidate _candidateApi;

        public TestHelper(ITestOutputHelper output)
        {
            _output = output;
            _httpContext = new DefaultHttpContext();
            _fileSystem = new FileSystem();
            _log = new Mock<ILogger<LoggerHelper>>();
            _log.MockLog(LogLevel.Debug);
            _log.MockLog(LogLevel.Information);
            _log.MockLog(LogLevel.Warning);
            _log.MockLog(LogLevel.Error);

            var mockUserSet = DbMoqHelper.GetDbSet(MoqData.MockUserData);
            var mockUserContext = new Mock<TrueVoteDbContext>();
            mockUserContext.Setup(m => m.Users).Returns(mockUserSet.Object);
            mockUserContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));
            _userApi = new User(_log.Object, mockUserContext.Object);

            var mockElectionSet = DbMoqHelper.GetDbSet(MoqData.MockElectionData);
            var mockElectionContext = new Mock<TrueVoteDbContext>();
            mockElectionContext.Setup(m => m.Elections).Returns(mockElectionSet.Object);
            mockElectionContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));
            _electionApi = new Election(_log.Object, mockElectionContext.Object);

            var mockRaceSet = DbMoqHelper.GetDbSet(MoqData.MockRaceData);
            var mockRaceContext = new Mock<TrueVoteDbContext>();
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);
            mockRaceContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));
            _raceApi = new Race(_log.Object, mockRaceContext.Object);

            var mockCandidateSet = DbMoqHelper.GetDbSet(MoqData.MockCandidateData);
            var mockCandidateContext = new Mock<TrueVoteDbContext>();
            mockCandidateContext.Setup(m => m.Candidates).Returns(mockCandidateSet.Object);
            mockCandidateContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));
            _candidateApi = new Candidate(_log.Object, mockCandidateContext.Object);
        }
    }
}
