using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class TimestampModel
    {
        [Required]
        [Description("Timestamp Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("TimestampId")]
        [JsonProperty(nameof(TimestampId), Required = Required.Always)]
        [Key]
        public required string TimestampId { get; set; }

        [Required]
        [Description("MerkleRoot")]
        [DataType(DataType.Custom)]
        [JsonPropertyName("MerkleRoot")]
        [JsonProperty(nameof(MerkleRoot), Required = Required.Always)]
        public required byte[] MerkleRoot { get; set; }

        [Required]
        [Description("MerkleRootHash")]
        [DataType(DataType.Custom)]
        [JsonPropertyName("MerkleRootHash")]
        [JsonProperty(nameof(MerkleRootHash), Required = Required.Always)]
        public required byte[] MerkleRootHash { get; set; }

        [Required]
        [Description("TimestampHash")]
        [DataType(DataType.Custom)]
        [JsonPropertyName("TimestampHash")]
        [JsonProperty(nameof(TimestampHash), Required = Required.Always)]
        public required byte[] TimestampHash { get; set; }

        [Required]
        [Description("TimestampHash String")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("TimestampHashS")]
        [JsonProperty(nameof(TimestampHashS), Required = Required.Always)]
        public required string TimestampHashS { get; set; }

        public DateTime TimestampAt { get; set; }

        [Required]
        [Description("CalendarServerUrl")]
        [MaxLength(2048)]
        [DataType(DataType.Url)]
        [JsonPropertyName("CalendarServerUrl")]
        [JsonProperty(nameof(CalendarServerUrl), Required = Required.Always)]
        public required string CalendarServerUrl { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Always)]
        public required DateTime DateCreated { get; set; }
    }

    public class FindTimestampModel
    {
        [Required]
        [Description("DateCreatedStart")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreatedStart")]
        [JsonProperty(nameof(DateCreatedStart), Required = Required.Always)]
        public required DateTime DateCreatedStart { get; set; }

        [Required]
        [Description("DateCreatedEnd")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreatedEnd")]
        [JsonProperty(nameof(DateCreatedEnd), Required = Required.Always)]
        public required DateTime DateCreatedEnd { get; set; }
    }
}
