using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using TrueVote.Api.Models;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class StatusTest : TestHelper
    {
        public StatusTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FailsIfNullArgs()
        {
            try
            {
                var status = new Status(_fileSystem, _log.Object, true);
                _ = await status.GetStatus(null);
            }
            catch (ArgumentNullException ane)
            {
                _output.WriteLine($"{ane}");
                Assert.NotNull(ane);
                Assert.Contains("Value cannot be null", ane.Message);
            }
            catch (NullReferenceException nre)
            {
                _output.WriteLine($"{nre}");
                Assert.NotNull(nre);
                Assert.Contains("Object reference not set to an instance of an object", nre.Message);
            }
        }

        [Fact]
        public async Task LogsMessages()
        {
            var status = new Status(_fileSystem, _log.Object, true);
            _ = await status.GetStatus(_httpContext.Request);

            _log.Verify(LogLevel.Information, Times.AtLeast(2));
            _log.Verify(LogLevel.Debug, Times.AtLeast(2));
        }

        [Fact]
        public async Task ReturnsValidModel()
        {
            var status = new Status(_fileSystem, _log.Object, true);
            var res = (OkObjectResult) await status.GetStatus(_httpContext.Request);

            Assert.NotNull(res);
            var statusModel = (StatusModel) res.Value;
            Assert.NotNull(statusModel);
            Assert.Equal("TrueVote.Api is responding", statusModel.RespondsMsg);
        }

        [Fact]
        public async Task ReturnsValidBuildInfoModel()
        {
            var status = new Status(_fileSystem, _log.Object, true);
            var res = (OkObjectResult) await status.GetStatus(_httpContext.Request);

            Assert.NotNull(res);
            var statusModel = (StatusModel) res.Value;
            Assert.NotNull(statusModel);
            Assert.NotNull(statusModel.BuildInfo);
        }

        [Fact]
        public async Task ReturnsValidBuildInfoModelNonQualifiedPath()
        {
            var ret = string.Empty;
            var b = false;

            var fileSystemMock = new Mock<IFileSystem>();

            // Store the actual value of .IsPathFullyQualified(), but return false for this test
            fileSystemMock.Setup(x => x.Path.IsPathFullyQualified(It.IsAny<string>())).Callback((string p) =>
            {
                b = _fileSystem.Path.IsPathFullyQualified(p);
            }).Returns(false);

            // Intercept the .Combine() 
            fileSystemMock.Setup(x => x.Path.Combine(It.IsAny<string>(), It.IsAny<string>())).Callback((string p1, string p2) =>
            {
                ret = _fileSystem.Path.Combine(p1, p2);

                // Check the actual value of IsPathFullyQualified. If it is actually true, but we set to false above in the mock
                // then the slash was added and needs to be removed at the combine step here.
                if (b)
                {
                    ret = ret.TrimStart('/');
                }
            }).Returns(() => ret);

            // This mock of .ReadAllText() just calls the base, actual version
            fileSystemMock.Setup(x => x.File.ReadAllText(It.IsAny<string>())).Callback((string p) =>
            {
                ret = _fileSystem.File.ReadAllText(p);
            }).Returns(() => ret);

            var statusMock = new Status(fileSystemMock.Object, _log.Object, true);
            var res = (OkObjectResult) await statusMock.GetStatus(_httpContext.Request);

            Assert.NotNull(res);
            var statusModel = (StatusModel) res.Value;
            Assert.NotNull(statusModel);
            Assert.NotNull(statusModel.BuildInfo);

            _log.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => string.Equals("Added leading '/' to binDir", o.ToString())),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task RunsStopwatch()
        {
            var status = new Status(_fileSystem, _log.Object);
            var res = (OkObjectResult) await status.GetStatus(_httpContext.Request);

            Assert.NotNull(res);
            var statusModel = (StatusModel) res.Value;
            Assert.NotNull(statusModel);
            Assert.True(statusModel.ExecutionTime >= 0);
        }

        [Fact]
        public async Task HandlesMissingVersionFileException()
        {
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(x => x.Path.IsPathFullyQualified(It.IsAny<string>())).Returns(true);
            fileSystemMock.Setup(x => x.File.ReadAllText(It.IsAny<string>())).Throws(new Exception());

            var statusMock = new Status(fileSystemMock.Object, _log.Object, true);
            var res = (OkObjectResult) await statusMock.GetStatus(_httpContext.Request);

            Assert.NotNull(res);
            var statusModel = (StatusModel) res.Value;
            Assert.NotNull(statusModel);
            Assert.Null(statusModel.BuildInfo);
            _log.Verify(LogLevel.Error, Times.AtLeastOnce());
        }
    }
}
