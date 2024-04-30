using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
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
            Assert.Equal(18, timestamp.MerkleRoot[0]);
        }

        [Fact]
        public async Task HashesBallotThrowsStampingError()
        {
            _mockOpenTimestampsClient.Setup(m => m.Stamp(It.IsAny<byte[]>())).Throws(new Exception("Stamp exception"));

            try
            {
                var timestamp = await _validatorApi.HashBallotsAsync();
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

            var validatorApi = new Validator(_logHelper.Object, mockBallotContext.Object, _mockOpenTimestampsClient.Object, _mockServiceBus.Object);

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
                var ballotHashModel = await _validatorApi.HashBallotAsync(MoqData.MockBallotData[0]);
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
        public async Task HashesBallot()
        {
            try
            {
                var ballotHashModel = await _validatorApi.HashBallotAsync(MoqData.MockBallotData[2]);
                Assert.NotNull(ballotHashModel);
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

            var validatorApi = new Validator(_logHelper.Object, mockBallotHashContext.Object, _mockOpenTimestampsClient.Object, _mockServiceBus.Object);

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

        [Fact]
        public async Task StoreTimestampAsyncThrowsException()
        {
            var mockTimestampContext = new Mock<MoqTrueVoteDbContext>();
            var mockTimestampDataQueryable = MoqData.MockTimestampData.AsQueryable();
            var MockTimestampSet = DbMoqHelper.GetDbSet(mockTimestampDataQueryable);
            mockTimestampContext.Setup(m => m.Timestamps).Returns(MockTimestampSet.Object);
            mockTimestampContext.Setup(m => m.EnsureCreatedAsync()).Throws(new Exception("Storing data exception"));

            var validatorApi = new Validator(_logHelper.Object, mockTimestampContext.Object, _mockOpenTimestampsClient.Object, _mockServiceBus.Object);

            try
            {
                await validatorApi.StoreTimestampAsync(MoqData.MockTimestampData[0]);
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
