using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class BuildInfo
    {
        [OpenApiPropertyDescription("Git branch of instance")]
        public string Branch { get; set; } = string.Empty;

        [OpenApiPropertyDescription("Timestamp build was created")]
        public string BuildTime { get; set; } = string.Empty;

        [OpenApiPropertyDescription("Git tag of instance")]
        public string LastTag { get; set; } = string.Empty;

        [OpenApiPropertyDescription("Git commit hash of instance")]
        public string Commit { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class StatusModel
    {
        [OpenApiPropertyDescription("Current Time")]
        public string CurrentTime { get; internal set; }

        [OpenApiPropertyDescription("Stopwatch time to run this method")]
        public long ExecutionTime { get; internal set; }

        [OpenApiPropertyDescription("Stopwatch time to run this method (message)")]
        public string ExecutionTimeMsg { get; internal set; }

        [OpenApiPropertyDescription("True if method responds. Likely never false")]
        public bool Responds { get; internal set; }

        [OpenApiPropertyDescription("True if method responds. Likely never false (message)")]
        public string RespondsMsg { get; internal set; }

        [OpenApiPropertyDescription("Build information model")]
        public BuildInfo BuildInfo { get; internal set; }

        [OpenApiPropertyDescription("Timestamp this Build information data model was populated")]
        public string BuildInfoReadTime { get; set; } = string.Empty;
    }
}
