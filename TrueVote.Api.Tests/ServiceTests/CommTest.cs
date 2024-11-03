using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;
using TrueVote.Api.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using MockQueryable.Moq;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class CommunicationEventTest(ITestOutputHelper output) : TestHelper(output)
    {
        [Fact]
        public void MetadataJsonCoverageTest()
        {
            var commEvent = new CommunicationEventModel
            {
                CommunicationEventId = "test-id",
                Type = "test-type",
                Status = "Queued",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                CommunicationMethod = new Dictionary<string, string>(),
                RelatedEntities = new Dictionary<string, string>(),
                Metadata = null  // Start with null to test that path
            };

            // Test getter - null path
            Assert.Null(commEvent.MetadataJson);

            // Test getter - non-null path
            commEvent.Metadata = new Dictionary<string, string> { { "key", "value" } };
            Assert.NotNull(commEvent.MetadataJson);

            // Test setter - null path
            commEvent.MetadataJson = null;
            Assert.Null(commEvent.Metadata);

            // Test setter - non-null path
            commEvent.MetadataJson = "{\"key\":\"value\"}";
            Assert.NotNull(commEvent.Metadata);
        }

        [Fact]
        public void JsonPropertiesFullCoverageTest()
        {
            var now = DateTime.UtcNow;
            var commEvent = new CommunicationEventModel
            {
                CommunicationEventId = "test-id",
                Type = "test-type",
                Status = "Queued",
                DateCreated = now,
                DateUpdated = now,
                CommunicationMethod = new Dictionary<string, string>(),
                RelatedEntities = new Dictionary<string, string>(),
                Metadata = null
            };

            // Test both gets
            _ = commEvent.CommunicationMethodJson;
            _ = commEvent.RelatedEntitiesJson;

            // This should deserialize to null, triggering the ?? new()
            commEvent.CommunicationMethodJson = "null";
            commEvent.RelatedEntitiesJson = "null";

            // Test with actual dictionaries to hit the non-null path
            commEvent.CommunicationMethodJson = "{\"key\":\"value\"}";
            commEvent.RelatedEntitiesJson = "{\"key\":\"value\"}";

            Assert.NotNull(commEvent.CommunicationMethod);
            Assert.NotNull(commEvent.RelatedEntities);
        }

        [Fact]
        public void JsonPropertiesHandleInvalidJson()
        {
            var now = DateTime.UtcNow;
            var commEvent = new CommunicationEventModel
            {
                CommunicationEventId = "test-id",
                Type = "test-type",
                Status = "Queued",
                DateCreated = now,
                DateUpdated = now,
                CommunicationMethod = new(),
                RelatedEntities = new(),
                Metadata = null
            };

            Assert.Throws<JsonReaderException>(() =>
                commEvent.CommunicationMethodJson = "{invalid json}");

            Assert.Throws<JsonReaderException>(() =>
                commEvent.RelatedEntitiesJson = "{invalid json}");

            Assert.Throws<JsonReaderException>(() =>
                commEvent.MetadataJson = "{invalid json}");
        }

        [Fact]
        public void NullablePropertiesCoverageTest()
        {
            var now = DateTime.UtcNow;
            var commEvent = new CommunicationEventModel
            {
                CommunicationEventId = "test-id",
                Type = "test-type",
                Status = "Queued",
                DateCreated = now,
                DateUpdated = now,
                CommunicationMethod = new Dictionary<string, string>(),
                RelatedEntities = new Dictionary<string, string>(),
                Metadata = null
            };

            // Test DateProcessed
            Assert.Null(commEvent.DateProcessed);
            commEvent.DateProcessed = now;
            Assert.Equal(now, commEvent.DateProcessed);

            // Test ErrorMessage
            Assert.Null(commEvent.ErrorMessage);
            commEvent.ErrorMessage = "test error";
            Assert.Equal("test error", commEvent.ErrorMessage);

            // Test TimeToLive
            Assert.Null(commEvent.TimeToLive);
            commEvent.TimeToLive = 3600;
            Assert.Equal(3600, commEvent.TimeToLive);
        }

        [Fact]
        public async Task UpdatesCommEvent()
        {
            var commEventData = MoqData.MockCommunicationEventData[0];
            var user = MoqData.MockUserData[0];
            _commApi.SetupController(user.UserId);

            var now = DateTime.UtcNow;
            var communicationEventModel = new CommunicationEventUpdateModel
            {
                CommunicationEventId = commEventData.CommunicationEventId,
                Status = "Completed",
                DateProcessed = now,
                ErrorMessage = ""
            };

            var validationResults = ValidationHelper.Validate(communicationEventModel);
            Assert.Empty(validationResults);

            var ret = await _commApi.UpdateCommEventStatus(communicationEventModel);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status200OK, ((IStatusCodeActionResult) ret).StatusCode);

            var updatedCommEvent = (CommunicationEventModel) (ret as OkObjectResult).Value;
            Assert.True(UtcNowProviderFactory.GetProvider().UtcNow - updatedCommEvent.DateUpdated <= TimeSpan.FromSeconds(3));

            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUpdateCommEventNotFound()
        {
            var user = MoqData.MockUserData[0];
            _commApi.SetupController(user.UserId);

            var now = DateTime.UtcNow;
            var communicationEventModel = new CommunicationEventUpdateModel
            {
                CommunicationEventId = "4450baf3-3f89-46ab-842f-b235f0f22941",
                Status = "Completed",
                DateProcessed = now,
                ErrorMessage = ""
            };

            var validationResults = ValidationHelper.Validate(communicationEventModel);
            Assert.Empty(validationResults);

            var ret = await _commApi.UpdateCommEventStatus(communicationEventModel);

            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status404NotFound, ((IStatusCodeActionResult) ret).StatusCode);

            _logHelper.Verify(LogLevel.Debug, Times.Exactly(2));
        }

        [Fact]
        public async Task HandlesUpdateCommEventDatabaseError()
        {
            var now = DateTime.UtcNow;
            var communicationEventModel = new CommunicationEventUpdateModel
            {
                CommunicationEventId = MoqData.MockCommunicationEventData[0].CommunicationEventId,
                Status = "Completed",
                DateProcessed = now,
                ErrorMessage = ""
            };

            var mockCommunicationEventContext = new Mock<MoqTrueVoteDbContext>();

            var MockCommunicationEventSet = MoqData.MockCommunicationEventData.AsQueryable().BuildMockDbSet();
            mockCommunicationEventContext.Setup(m => m.CommunicationEvents).Returns(MockCommunicationEventSet.Object);
            mockCommunicationEventContext.Setup(m => m.SaveChangesAsync()).Throws(new Exception("DB Saving Changes Exception"));

            var commApi = new Comms(_logHelper.Object, mockCommunicationEventContext.Object);
            var user = MoqData.MockUserData[0];
            commApi.SetupController(user.UserId);

            var ret = await commApi.UpdateCommEventStatus(communicationEventModel);
            Assert.NotNull(ret);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, ((IStatusCodeActionResult) ret).StatusCode);

            var val = (SecureString) (ret as UnprocessableEntityObjectResult).Value;
            Assert.NotNull(val);
            Assert.Contains("DB Saving Changes Exception", val.Value);

            _logHelper.Verify(LogLevel.Error, Times.Exactly(1));
            _logHelper.Verify(LogLevel.Debug, Times.Exactly(1));
        }
    }
}
