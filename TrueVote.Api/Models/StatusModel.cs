using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class BuildInfo
    {
        [OpenApiProperty(Description = "Git branch of instance")]
        public string Branch { get; set; } = string.Empty;

        [OpenApiProperty(Description = "Timestamp build was created")]
        public string BuildTime { get; set; } = string.Empty;

        [OpenApiProperty(Description = "Git tag of instance")]
        public string LastTag { get; set; } = string.Empty;

        [OpenApiProperty(Description = "Git commit hash of instance")]
        public string Commit { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class StatusModel
    {
        [OpenApiProperty(Description = "Current Time")]
        public string CurrentTime { get; internal set; }

        [OpenApiProperty(Description = "Stopwatch time to run this method")]
        public long ExecutionTime { get; internal set; }

        [OpenApiProperty(Description = "Stopwatch time to run this method (message)")]
        public string ExecutionTimeMsg { get; internal set; }

        [OpenApiProperty(Description = "True if method responds. Likely never false")]
        public bool Responds { get; internal set; }

        [OpenApiProperty(Description = "True if method responds. Likely never false (message)")]
        public string RespondsMsg { get; internal set; }

        [OpenApiProperty(Description = "Build information model")]
        public BuildInfo BuildInfo { get; internal set; }

        [OpenApiProperty(Description = "Timestamp this Build information data model was populated")]
        public string BuildInfoReadTime { get; set; } = string.Empty;
    }
}
