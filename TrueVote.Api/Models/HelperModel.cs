using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class SecureString
    {
        [Required]
        [Description("Value")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("Value")]
        [JsonProperty("Value", Required = Required.Always)]
        public required string Value { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class Error500Flag
    {
        [Required]
        [Description("Error")]
        [JsonPropertyName("Error")]
        [JsonProperty("Error", Required = Required.Always)]
        public required bool Error { get; set; } = false;
    }
}
