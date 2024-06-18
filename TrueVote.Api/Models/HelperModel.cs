using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TrueVote.Api.Models
{
    public class SecureString
    {
        [Required]
        [Description("Value")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Value")]
        [JsonProperty(nameof(Value), Required = Required.Always)]
        public required string Value { get; set; } = string.Empty;
    }

    public class Error500Flag
    {
        [Required]
        [Description("Error")]
        [JsonPropertyName("Error")]
        [JsonProperty(nameof(Error), Required = Required.Always)]
        public required bool Error { get; set; } = false;
    }
}
