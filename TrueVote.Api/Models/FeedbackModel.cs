using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TrueVote.Api.Models
{
    public class FeedbackModel
    {
        [Required]
        [Description("Feedback Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("FeedbackId")]
        [JsonProperty(nameof(FeedbackId), Required = Required.Always)]
        [Key]
        public required string FeedbackId { get; set; }

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
        public required DateTime DateCreated { get; set; }

        [Required]
        [Description("Feedback")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Feedback")]
        [JsonProperty(nameof(Feedback), Required = Required.Always)]
        public required string Feedback { get; set; }
    }
}
