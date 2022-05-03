using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class BuildInfo
    {
        [OpenApiProperty(Description = "Git branch of instance")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonProperty(PropertyName = "Branch")]
        public string Branch { get; set; } = string.Empty;

        [OpenApiProperty(Description = "Timestamp build was created")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonProperty(PropertyName = "BuildTime")]
        public string BuildTime { get; set; } = string.Empty;

        [OpenApiProperty(Description = "Git tag of instance")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonProperty(PropertyName = "LastTag")]
        public string LastTag { get; set; } = string.Empty;

        [OpenApiProperty(Description = "Git commit hash of instance")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonProperty(PropertyName = "Commit")]
        public string Commit { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class StatusModel
    {
        [OpenApiProperty(Description = "Current Time")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonProperty(PropertyName = "CurrentTime")]
        public string CurrentTime { get; internal set; }

        [OpenApiProperty(Description = "Stopwatch time to run this method")]
        [Range(0, Int32.MaxValue)]
        [JsonProperty(PropertyName = "ExecutionTime")]
        public long ExecutionTime { get; internal set; }

        [OpenApiProperty(Description = "Stopwatch time to run this method (message)")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonProperty(PropertyName = "ExecutionTimeMsg")]
        public string ExecutionTimeMsg { get; internal set; }

        [OpenApiProperty(Description = "True if method responds. Likely never false")]
        [JsonProperty(PropertyName = "Responds")]
        public bool Responds { get; internal set; }

        [OpenApiProperty(Description = "True if method responds. Likely never false (message)")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonProperty(PropertyName = "RespondsMsg")]
        public string RespondsMsg { get; internal set; }

        [OpenApiProperty(Description = "Build information model")]
        [JsonProperty(PropertyName = "BuildInfo")]
        public BuildInfo BuildInfo { get; internal set; }

        [OpenApiProperty(Description = "Timestamp this Build information data model was populated")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonProperty(PropertyName = "BuildInfoReadTime")]
        public string BuildInfoReadTime { get; set; } = string.Empty;
    }
}
