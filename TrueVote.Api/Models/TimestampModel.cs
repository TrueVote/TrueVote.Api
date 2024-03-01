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
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("TimestampId")]
        [Key]
        public string TimestampId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("MerkleRoot")]
        [DataType(DataType.Custom)]
        [JsonPropertyName("MerkleRoot")]
        public byte[] MerkleRoot { get; set; }

        [Required]
        [Description("MerkleRootHash")]
        [DataType(DataType.Custom)]
        [JsonPropertyName("MerkleRootHash")]
        public byte[] MerkleRootHash { get; set; }

        [Required]
        [Description("TimestampHash")]
        [DataType(DataType.Custom)]
        [JsonPropertyName("TimestampHash")]
        public byte[] TimestampHash { get; set; }

        [Required]
        [Description("TimestampHash String")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("TimestampHashS")]
        public string TimestampHashS { get; set; }

        public DateTime TimestampAt { get; set; }

        [Required]
        [Description("CalendarServerUrl")]
        [MaxLength(2048)]
        [DataType(DataType.Url)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("CalendarServerUrl")]
        public string CalendarServerUrl { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;
    }

    public class FindTimestampModel
    {
        [Required]
        [Description("DateCreatedStart")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreatedStart")]
        public DateTime DateCreatedStart { get; set; }

        [Required]
        [Description("DateCreatedEnd")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreatedEnd")]
        public DateTime DateCreatedEnd { get; set; }
    }
}
