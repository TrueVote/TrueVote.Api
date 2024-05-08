using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class FeedbackModel
    {
        [Required]
        [Description("Feedback Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("FeedbackId")]
        [JsonProperty(nameof(FeedbackId), Required = Required.Always)]
        [Key]
        [JsonIgnore]
        public required string FeedbackId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("User Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("UserId")]
        [JsonProperty(nameof(UserId), Required = Required.Always)]
        public required string UserId { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        [JsonIgnore]
        public required DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("Feedback")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Feedback")]
        [JsonProperty(nameof(Feedback), Required = Required.Always)]
        public required string Feedback { get; set; }
    }
}
