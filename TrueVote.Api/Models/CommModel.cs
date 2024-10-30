using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TrueVote.Api.Models
{
    public abstract class RootCommunicationEventBaseModel
    {
        [Required]
        [Description("Type of communication (Email, SMS, Push)")]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Type")]
        [JsonProperty(nameof(Type), Required = Required.Always)]
        public required string Type { get; set; } = string.Empty;

        [Required]
        [Description("Communication method and address/id (e.g., Email: user@domain.com, MobileDeviceId: abc123)")]
        [JsonPropertyName("CommunicationMethod")]
        [JsonProperty(nameof(CommunicationMethod), Required = Required.Always)]
        public required Dictionary<string, string> CommunicationMethod { get; set; } = []; // Example: { "Email": "user@domain.com", "MobileDeviceId": "device_123", "PhoneNumber": "+1234567890" }

        [Required]
        [Description("Dictionary of related entity IDs and their types")]
        [JsonPropertyName("RelatedEntities")]
        [JsonProperty(nameof(RelatedEntities), Required = Required.Always)]
        public required Dictionary<string, string> RelatedEntities { get; set; } = [];

        [Description("Additional metadata for the communication event")]
        [JsonPropertyName("Metadata")]
        [JsonProperty(nameof(Metadata), Required = Required.Default)]
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class CommunicationEventModel : RootCommunicationEventBaseModel
    {
        [Required]
        [Description("Communication Event Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("CommunicationEventId")]
        [JsonProperty(nameof(CommunicationEventId), Required = Required.Always)]
        [Key]
        public required string CommunicationEventId { get; set; }

        [Required]
        [Description("Status of the communication")]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Status")]
        [JsonProperty(nameof(Status), Required = Required.Always)]
        public required string Status { get; set; } = "Queued";

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; }

        [Required]
        [Description("DateUpdated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateUpdated")]
        [JsonProperty(nameof(DateUpdated), Required = Required.Default)]
        public required DateTime DateUpdated{ get; set; }


        [Description("DateProcessed")]
        [DataType(DataType.DateTime)]
        [JsonPropertyName("DateProcessed")]
        [JsonProperty(nameof(DateProcessed), Required = Required.Default)]
        public DateTime? DateProcessed{ get; set; }

        [Description("Error message if failed")]
        [MaxLength(4096)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ErrorMessage")]
        [JsonProperty(nameof(ErrorMessage), Required = Required.Default)]
        public string? ErrorMessage { get; set; }

        [Description("Time To Live in seconds (null = forever)")]
        [JsonPropertyName("TimeToLive")]
        [JsonProperty(nameof(TimeToLive), Required = Required.Default)]
        public int? TimeToLive { get; set; }
    }

    public class VoterElectionAccessCodeRequest
    {
        [Required]
        [Description("Election ID")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        public required string ElectionId { get; set; }

        [Required]
        [Description("Voter email address")]
        [MaxLength(256)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [JsonPropertyName("VoterEmail")]
        [JsonProperty(nameof(VoterEmail), Required = Required.Always)]
        public required string VoterEmail { get; set; }
    }
}
