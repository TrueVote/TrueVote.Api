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
        public string Value { get; set; } = string.Empty;
    }
}
