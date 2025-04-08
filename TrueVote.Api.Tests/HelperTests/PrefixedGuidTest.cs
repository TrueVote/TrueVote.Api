using System;
using TrueVote.Api.Helpers;
using Xunit;

namespace TrueVote.Api.Tests.HelperTests
{
    public class PrefixedGuidTest
    {
        [Fact]
        public void NewPrefixedGuid_GeneratesCorrectFormat()
        {
            // Arrange
            var entityTypes = Enum.GetValues<PrefixedGuid.EntityType>();

            foreach (var entityType in entityTypes)
            {
                // Act
                var prefixedGuid = PrefixedGuid.NewPrefixedGuid(entityType);

                // Assert
                Assert.NotNull(prefixedGuid);
                Assert.StartsWith($"{(char) entityType}-", prefixedGuid);
                Assert.True(Guid.TryParse(prefixedGuid[2..], out _));
                Assert.Equal(38, prefixedGuid.Length); // prefix + hyphen + 36 guid chars
            }
        }

        [Theory]
        [InlineData(PrefixedGuid.EntityType.Ballot)]
        [InlineData(PrefixedGuid.EntityType.Election)]
        [InlineData(PrefixedGuid.EntityType.User)]
        [InlineData(PrefixedGuid.EntityType.Race)]
        [InlineData(PrefixedGuid.EntityType.Candidate)]
        [InlineData(PrefixedGuid.EntityType.Message)]
        [InlineData(PrefixedGuid.EntityType.Role)]
        [InlineData(PrefixedGuid.EntityType.Feedback)]
        [InlineData(PrefixedGuid.EntityType.Timestamp)]
        [InlineData(PrefixedGuid.EntityType.Hash)]
        public void IsValidPrefixedGuid_ValidInput_ReturnsTrue(PrefixedGuid.EntityType entityType)
        {
            // Arrange
            var validGuid = PrefixedGuid.NewPrefixedGuid(entityType);

            // Act
            var isValid = PrefixedGuid.IsValidPrefixedGuid(validGuid, entityType);

            // Assert
            Assert.True(isValid);
        }

        [Theory]
        [InlineData(null, PrefixedGuid.EntityType.Election)]
        [InlineData("", PrefixedGuid.EntityType.Election)]
        [InlineData("e", PrefixedGuid.EntityType.Election)]
        [InlineData("e-", PrefixedGuid.EntityType.Election)]
        [InlineData("x-invalid", PrefixedGuid.EntityType.Election)]
        [InlineData("e-not-a-guid", PrefixedGuid.EntityType.Election)]
        [InlineData("e-12345678-1234-1234-1234-1234567890ab", PrefixedGuid.EntityType.Ballot)] // Wrong type
        [InlineData("e12345678-1234-1234-1234-1234567890ab", PrefixedGuid.EntityType.Election)] // Missing dash
        [InlineData("-e-12345678-1234-1234-1234-1234567890ab", PrefixedGuid.EntityType.Election)] // Incorrect dash position
        public void IsValidPrefixedGuid_InvalidInput_ReturnsFalse(string prefixedGuid, PrefixedGuid.EntityType expectedType)
        {
            // Act
            var isValid = PrefixedGuid.IsValidPrefixedGuid(prefixedGuid, expectedType);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ExtractGuid_ValidInput_ReturnsGuid()
        {
            // Arrange
            var originalGuid = Guid.NewGuid();
            var prefixedGuid = $"{(char) PrefixedGuid.EntityType.Election}-{originalGuid}";

            // Act
            var extractedGuid = PrefixedGuid.ExtractGuid(prefixedGuid);

            // Assert
            Assert.Equal(originalGuid, extractedGuid);
        }

        [Theory]
        [InlineData(null, "Invalid prefixed GUID format")]
        [InlineData("", "Invalid prefixed GUID format")]
        [InlineData("e", "Invalid prefixed GUID format")]
        [InlineData("e123", "Invalid prefixed GUID format - missing dash")]
        [InlineData("e-not-a-guid", "Invalid GUID format")]
        public void ExtractGuid_InvalidInput_ThrowsArgumentException(string prefixedGuid, string expectedMessage)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => PrefixedGuid.ExtractGuid(prefixedGuid));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Theory]
        [InlineData(PrefixedGuid.EntityType.Ballot)]
        [InlineData(PrefixedGuid.EntityType.Election)]
        [InlineData(PrefixedGuid.EntityType.User)]
        [InlineData(PrefixedGuid.EntityType.Race)]
        [InlineData(PrefixedGuid.EntityType.Candidate)]
        [InlineData(PrefixedGuid.EntityType.Message)]
        [InlineData(PrefixedGuid.EntityType.Role)]
        [InlineData(PrefixedGuid.EntityType.Feedback)]
        [InlineData(PrefixedGuid.EntityType.Timestamp)]
        [InlineData(PrefixedGuid.EntityType.Hash)]
        public void GetEntityType_ValidInput_ReturnsCorrectType(PrefixedGuid.EntityType expectedType)
        {
            // Arrange
            var prefixedGuid = PrefixedGuid.NewPrefixedGuid(expectedType);

            // Act
            var actualType = PrefixedGuid.GetEntityType(prefixedGuid);

            // Assert
            Assert.Equal(expectedType, actualType);
        }

        [Theory]
        [InlineData(null, "Invalid prefixed GUID format")]
        [InlineData("", "Invalid prefixed GUID format")]
        [InlineData("e", "Invalid prefixed GUID format")]
        [InlineData("e123", "Invalid prefixed GUID format - missing dash")]
        public void GetEntityType_InvalidInput_ThrowsArgumentException(string prefixedGuid, string expectedMessage)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => PrefixedGuid.GetEntityType(prefixedGuid));
            Assert.Equal(expectedMessage, exception.Message);
        }
    }
} 