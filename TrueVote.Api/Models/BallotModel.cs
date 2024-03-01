using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api2.Helpers;
using ByteConverter = TrueVote.Api2.Helpers.ByteConverter;

namespace TrueVote.Api2.Models
{
    [ExcludeFromCodeCoverage]
    public class BallotList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Ballots")]
        [JsonProperty(PropertyName = "Ballots")]
        public List<BallotModel> Ballots { get; set; }

        [Required]
        [MaxLength(2048)]
        [Description("List of Ballot Hashes")]
        [JsonProperty(PropertyName = "BallotHashes")]
        public List<BallotHashModel> BallotHashes { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindBallotModel
    {
        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "BallotId")]
        [Key]
        public string BallotId { get; set; } = Guid.NewGuid().ToString();
    }

    [ExcludeFromCodeCoverage]
    public class CountBallotModel
    {
        [Required]
        [Description("DateCreatedStart")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreatedStart", Required = Required.Always)]
        public DateTime DateCreatedStart { get; set; }

        [BindRequired]
        [Required]
        [Description("DateCreatedEnd")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreatedEnd", Required = Required.Always)]
        public DateTime DateCreatedEnd { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CountBallotModelResponse
    {
        [Required]
        [Description("Number of Ballots")]
        [Range(0, long.MaxValue)]
        [JsonProperty(PropertyName = "BallotCount")]
        public long BallotCount { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BallotModel
    {
        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "BallotId", Required = Required.Always)]
        [Key]
        public string BallotId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(2048)]
        [Description("Election for the Ballot")]
        [JsonProperty(PropertyName = "Election")]
        public ElectionModel Election { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

    }

    [ExcludeFromCodeCoverage]
    public class SubmitBallotModel {
        [Required]
        [Description("Election")]
        [DataType("ElectionModel")]
        [JsonProperty(PropertyName = "Election", Required = Required.Always)]
        public ElectionModel Election { get; set; }

        // TODO Add Bindings of User / Ballot connection
        // Requires encryption for binding stored at client and server for match
        // public string UserId { get; set; }
        // public string UserIdBallotIdHashed { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SubmitBallotModelResponse {
        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "BallotId")]
        [Key]
        public string BallotId { get; set; }

        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "ElectionId")]
        public string ElectionId { get; set; }

        [Required]
        [Description("Message")]
        [MaxLength(32768)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BallotHashModel
    {
        [Required]
        [Description("Ballot Hash Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "BallotHashId")]
        [Key]
        public string BallotHashId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "BallotId")]
        [ForeignKey("BallotId")]
        public string BallotId { get; set; }

        [Required]
        [Description("Server Ballot Hash")]
        [DataType(DataType.Custom)]
        [JsonConverter(typeof(ByteConverter))]
        [JsonProperty(PropertyName = "ServerBallotHash")]
        public byte[] ServerBallotHash { get; set; }

        [Required]
        [Description("Server Ballot Hash String")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "ServerBallotHashS")]
        public string ServerBallotHashS { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("DateUpdated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateUpdated")]
        public DateTime DateUpdated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("Timestamp Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "TimestampId", Required = Required.AllowNull)]
        [ForeignKey("TimestampId")]
        public string TimestampId { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindBallotHashModel
    {
        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "BallotId")]
        [Key]
        public string BallotId { get; set; } = Guid.NewGuid().ToString();
    }
}
