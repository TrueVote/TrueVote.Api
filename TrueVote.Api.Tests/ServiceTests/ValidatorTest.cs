using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using NCrontab;
using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class ValidatorTest : TestHelper
    {
        public ValidatorTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task HashesBallotDataAsync()
        {
            var timestamp = await _validatorApi.HashBallotsAsync();

            Assert.NotNull(timestamp);
            Assert.Equal(116, timestamp.MerkleRoot[0]);
        }

        [Fact]
        public async Task HashesBallotThrowsStampingError()
        {
            var mockOpenTimestampsClient = new Mock<IOpenTimestampsClient>();
            mockOpenTimestampsClient.Setup(m => m.Stamp(It.IsAny<byte[]>())).Throws(new Exception("Stamp exception"));

            var validatorApi = new Validator(_logHelper.Object, _moqDataAccessor.mockBallotContext.Object, _mockTelegram.Object, mockOpenTimestampsClient.Object);

            try
            {
                var timestamp = await validatorApi.HashBallotsAsync();
                Assert.True(false);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"{ex}");
                Assert.NotNull(ex);
                Assert.Contains("Stamp exception", ex.Message);
            }
        }

        [Fact]
        public async Task HashesBallotThrowsStoreTimestampException()
        {
            var mockBallotContext = new Mock<MoqTrueVoteDbContext>();
            var mockBallotDataQueryable = MoqData.MockBallotData.AsQueryable();
            var mockBallotHashDataQueryable = MoqData.MockBallotHashData.AsQueryable();
            var MockBallotSet = DbMoqHelper.GetDbSet(mockBallotDataQueryable);
            var MockBallotHashSet = DbMoqHelper.GetDbSet(mockBallotHashDataQueryable);
            mockBallotContext.Setup(m => m.Ballots).Returns(MockBallotSet.Object);
            mockBallotContext.Setup(m => m.BallotHashes).Returns(MockBallotHashSet.Object);
            mockBallotContext.Setup(m => m.EnsureCreatedAsync()).Throws(new Exception("Storing data exception"));

            var validatorApi = new Validator(_logHelper.Object, mockBallotContext.Object, _mockTelegram.Object, _mockOpenTimestampsClient.Object);

            try
            {
                var timestamp = await validatorApi.HashBallotsAsync();
                Assert.True(false);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"{ex}");
                Assert.NotNull(ex);
                Assert.Contains("Storing data exception", ex.Message);
            }
        }

        [Fact]
        public async Task HandlesBallotAlreadyHashed()
        {
            try
            {
                var ballotHashModel = await _validatorApi.HashBallotAsync(MoqData.MockBallotData[0], null);
                Assert.True(false);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"{ex}");
                Assert.NotNull(ex);
                Assert.Contains("already been hashed", ex.Message);
            }
        }

        [Fact]
        public async Task HandlesBallotHashMismatch()
        {
            try
            {
                var ballotHashModel = await _validatorApi.HashBallotAsync(MoqData.MockBallotData[1], "123");
                Assert.True(false);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"{ex}");
                Assert.NotNull(ex);
                Assert.Contains("client hash is different from server hash", ex.Message);
            }
        }

        [Fact]
        public async Task HashesBallot()
        {
            var ballotData2HashVal = "2lJ95YKyxyKjffwTd76bW7mELYv79AXeaTR3K4lKTBI=";

            try
            {
                var ballotHashModel = await _validatorApi.HashBallotAsync(MoqData.MockBallotData[2], ballotData2HashVal);
                Assert.NotNull(ballotHashModel);
                Assert.Equal(ballotData2HashVal, ballotHashModel.ServerBallotHashS);
            }
            catch
            {
                Assert.True(false);
            }
        }

        [Fact]
        public async Task StoreBallotHashAsyncThrowsException()
        {
            var mockBallotHashContext = new Mock<MoqTrueVoteDbContext>();
            var mockBallotHashDataQueryable = MoqData.MockBallotHashData.AsQueryable();
            var MockBallotHashSet = DbMoqHelper.GetDbSet(mockBallotHashDataQueryable);
            mockBallotHashContext.Setup(m => m.BallotHashes).Returns(MockBallotHashSet.Object);
            mockBallotHashContext.Setup(m => m.EnsureCreatedAsync()).Throws(new Exception("Storing data exception"));

            var validatorApi = new Validator(_logHelper.Object, mockBallotHashContext.Object, _mockTelegram.Object, _mockOpenTimestampsClient.Object);

            try
            {
                await validatorApi.StoreBallotHashAsync(MoqData.MockBallotHashData[0]);
                Assert.True(false);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"{ex}");
                Assert.NotNull(ex);
                Assert.Contains("Storing data exception", ex.Message);
            }
        }
    }
}
