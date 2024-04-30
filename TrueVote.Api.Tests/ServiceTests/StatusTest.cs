using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using TrueVote.Api.Models;
using TrueVote.Api.Services;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class StatusTest : TestHelper
    {
        private readonly Status status;

        public StatusTest(ITestOutputHelper output) : base(output)
        {
            status = new Status(_logHelper.Object, _mockServiceBus.Object);
        }

        [Fact]
        public async Task LogsMessages()
        {
            _ = await status.GetStatus();

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task ReturnsValidModel()
        {
            var ret = await status.GetStatus();
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (StatusModel) (ret as OkObjectResult).Value;
            Assert.NotNull(val);
            Assert.Equal("TrueVote.Api is responding", val.RespondsMsg);
        }

        [Fact]
        public async Task ReturnsValidBuildInfoModel()
        {
            var ret = await status.GetStatus();
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (StatusModel) (ret as OkObjectResult).Value;
            Assert.NotNull(val);
            Assert.NotNull(val.BuildInfo);
        }

        [Fact]
        public async Task RunsStopwatch()
        {
            var ret = await status.GetStatus();
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (StatusModel) (ret as OkObjectResult).Value;
            Assert.NotNull(val);
            Assert.True(val.ExecutionTime >= 0);
        }

        [Fact]
        public async Task RespondsFromPing()
        {
            var ret = await status.GetPing();
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));

            var val = (SecureString) (ret as OkObjectResult).Value;
            Assert.NotNull(val);
            Assert.Equal("Reply", val.Value);
        }

        [Fact]
        public async Task CalculatesMathExpression()
        {
            status.ControllerContext = _authControllerContext;
            Assert.NotNull(status.User);

            var ret = await status.GetAdd();
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));

            var val = (SecureString) (ret as OkObjectResult).Value;
            Assert.NotNull(val);
            Assert.Equal("4", val.Value);
        }

        [Fact]
        public async Task HandlesCalculatesMathExpressionWithoutAuthorization()
        {
            var ret = await status.GetAdd();
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status401Unauthorized, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }
    }
}
