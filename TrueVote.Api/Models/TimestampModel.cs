using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class TimestampModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Timestamp Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "TimestampId")]
        [Key]
        public string TimestampId { get; set; } = Guid.NewGuid().ToString();

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "MerkleRoot")]
        [DataType(DataType.Custom)]
        [JsonProperty(PropertyName = "MerkleRoot")]
        public byte[] MerkleRoot { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "MerkleRootHash")]
        [DataType(DataType.Custom)]
        [JsonProperty(PropertyName = "MerkleRootHash")]
        public byte[] MerkleRootHash { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "TimestampHash")]
        [DataType(DataType.Custom)]
        [JsonProperty(PropertyName = "TimestampHash")]
        public byte[] TimestampHash { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "TimestampHash String")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "TimestampHashS")]
        public string TimestampHashS { get; set; }

        public DateTime TimestampAt { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "CalendarServerUrl")]
        [MaxLength(2048)]
        [DataType(DataType.Url)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "CalendarServerUrl", Required = Required.Always)]
        public string CalendarServerUrl { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;
    }

    public class FindTimestampModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreatedStart")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreatedStart", Required = Required.Always)]
        public DateTime DateCreatedStart { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreatedEnd")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreatedEnd", Required = Required.Always)]
        public DateTime DateCreatedEnd { get; set; }
    }
}
