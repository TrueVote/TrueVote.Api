using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api2.Models
{
    [ExcludeFromCodeCoverage]
    public class SecureString
    {
        [Required]
        [Description("Value")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Value")]
        public string Value { get; set; } = string.Empty;
    }
}
