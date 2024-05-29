using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TrueVote.Api.Models
{
    public class BuildInfo
    {
        [Description("Git branch of instance")]
        [MaxLength(2048)]
        [JsonPropertyName("Branch")]
        [JsonProperty(nameof(Branch), Required = Required.Default)]
        public string Branch { get; set; } = string.Empty;

        [Description("Timestamp build was created")]
        [MaxLength(2048)]
        [JsonPropertyName("BuildTime")]
        [JsonProperty(nameof(BuildTime), Required = Required.Default)]
        public string BuildTime { get; set; } = string.Empty;

        [Description("Git tag of instance")]
        [MaxLength(2048)]
        [JsonPropertyName("LastTag")]
        [JsonProperty(nameof(LastTag), Required = Required.Default)]
        public string LastTag { get; set; } = string.Empty;

        [Description("Git commit hash of instance")]
        [MaxLength(2048)]
        [JsonPropertyName("Commit")]
        [JsonProperty(nameof(Commit), Required = Required.Default)]
        public string Commit { get; set; } = string.Empty;
    }

    public class StatusModel
    {
        [Description("Current Time")]
        [MaxLength(2048)]
        [JsonPropertyName("CurrentTime")]
        [JsonProperty(nameof(CurrentTime), Required = Required.Default)]
        public string? CurrentTime { get; set; }

        [Description("Stopwatch time to run this method")]
        [Range(0, long.MaxValue)]
        [JsonPropertyName("ExecutionTime")]
        [JsonProperty(nameof(ExecutionTime), Required = Required.Default)]
        public long ExecutionTime { get; set; }

        [Description("Stopwatch time to run this method (message)")]
        [MaxLength(2048)]
        [JsonPropertyName("ExecutionTimeMsg")]
        [JsonProperty(nameof(ExecutionTimeMsg), Required = Required.Default)]
        public string? ExecutionTimeMsg { get; set; }

        [Description("True if method responds. Likely never false")]
        [JsonPropertyName("Responds")]
        [JsonProperty(nameof(Responds), Required = Required.Default)]
        public bool Responds { get; set; }

        [Description("True if method responds. Likely never false (message)")]
        [MaxLength(2048)]
        [JsonPropertyName("RespondsMsg")]
        [JsonProperty(nameof(RespondsMsg), Required = Required.Default)]
        public string? RespondsMsg { get; set; }

        [Description("Build information model")]
        [JsonPropertyName("BuildInfo")]
        [JsonProperty(nameof(BuildInfo), Required = Required.Default)]
        public BuildInfo? BuildInfo { get; set; }

        [Description("Timestamp this Build information data model was populated")]
        [MaxLength(2048)]
        [JsonPropertyName("BuildInfoReadTime")]
        [JsonProperty(nameof(BuildInfoReadTime), Required = Required.Default)]
        public string BuildInfoReadTime { get; set; } = string.Empty;
    }
}
