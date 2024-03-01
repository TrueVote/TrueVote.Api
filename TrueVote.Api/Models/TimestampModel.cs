using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api2.Helpers;

namespace TrueVote.Api2.Models
{
    [ExcludeFromCodeCoverage]
    public class TimestampModel
    {
        [Required]
        [Description("Timestamp Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "TimestampId")]
        [Key]
        public string TimestampId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("MerkleRoot")]
        [DataType(DataType.Custom)]
        [JsonProperty(PropertyName = "MerkleRoot")]
        public byte[] MerkleRoot { get; set; }

        [Required]
        [Description("MerkleRootHash")]
        [DataType(DataType.Custom)]
        [JsonProperty(PropertyName = "MerkleRootHash")]
        public byte[] MerkleRootHash { get; set; }

        [Required]
        [Description("TimestampHash")]
        [DataType(DataType.Custom)]
        [JsonProperty(PropertyName = "TimestampHash")]
        public byte[] TimestampHash { get; set; }

        [Required]
        [Description("TimestampHash String")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "TimestampHashS")]
        public string TimestampHashS { get; set; }

        public DateTime TimestampAt { get; set; }

        [Required]
        [Description("CalendarServerUrl")]
        [MaxLength(2048)]
        [DataType(DataType.Url)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "CalendarServerUrl", Required = Required.Always)]
        public string CalendarServerUrl { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;
    }

    public class FindTimestampModel
    {
        [Required]
        [Description("DateCreatedStart")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreatedStart", Required = Required.Always)]
        public DateTime DateCreatedStart { get; set; }

        [Required]
        [Description("DateCreatedEnd")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreatedEnd", Required = Required.Always)]
        public DateTime DateCreatedEnd { get; set; }
    }
}
