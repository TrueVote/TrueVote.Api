using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TrueVote.Api.Models;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.ServiceTests
{
    public class CommunicationEventModelTest(ITestOutputHelper output) : TestHelper(output)
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
    }
}
