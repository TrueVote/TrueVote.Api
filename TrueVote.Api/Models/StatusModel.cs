using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class BuildInfo
    {
        [Description("Git branch of instance")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonPropertyName("Branch")]
        public string Branch { get; set; } = string.Empty;

        [Description("Timestamp build was created")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonPropertyName("BuildTime")]
        public string BuildTime { get; set; } = string.Empty;

        [Description("Git tag of instance")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonPropertyName("LastTag")]
        public string LastTag { get; set; } = string.Empty;

        [Description("Git commit hash of instance")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonPropertyName("Commit")]
        public string Commit { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class StatusModel
    {
        [Description("Current Time")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonPropertyName("CurrentTime")]
        public string CurrentTime { get; set; }

        [Description("Stopwatch time to run this method")]
        [Range(0, long.MaxValue)]
        [JsonPropertyName("ExecutionTime")]
        public long ExecutionTime { get; set; }

        [Description("Stopwatch time to run this method (message)")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonPropertyName("ExecutionTimeMsg")]
        public string ExecutionTimeMsg { get; set; }

        [Description("True if method responds. Likely never false")]
        [JsonPropertyName("Responds")]
        public bool Responds { get; set; }

        [Description("True if method responds. Likely never false (message)")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonPropertyName("RespondsMsg")]
        public string RespondsMsg { get; set; }

        [Description("Build information model")]
        [JsonPropertyName("BuildInfo")]
        public BuildInfo BuildInfo { get; set; }

        [Description("Timestamp this Build information data model was populated")]
        [RegularExpression(Constants.GenericStringRegex)]
        [MaxLength(2048)]
        [JsonPropertyName("BuildInfoReadTime")]
        public string BuildInfoReadTime { get; set; } = string.Empty;
    }
}
