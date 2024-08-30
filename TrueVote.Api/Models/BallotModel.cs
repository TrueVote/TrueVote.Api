using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;
using ByteConverter = TrueVote.Api.Helpers.ByteConverter;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;
using JsonConverterAttribute = System.Text.Json.Serialization.JsonConverterAttribute;

namespace TrueVote.Api.Models
{
    public class BallotList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Ballots")]
        [JsonPropertyName("Ballots")]
        [JsonProperty(nameof(Ballots), Required = Required.Always)]
        public required List<BallotModel> Ballots { get; set; }

        [Required]
        [MaxLength(2048)]
        [Description("List of Ballot Hashes")]
        [JsonPropertyName("BallotHashes")]
        [JsonProperty(nameof(BallotHashes), Required = Required.Always)]
        public required List<BallotHashModel> BallotHashes { get; set; }
    }

    public class FindBallotModel
    {
        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("BallotId")]
        [JsonProperty(nameof(BallotId), Required = Required.Always)]
        [Key]
        public required string BallotId { get; set; }
    }

    public class CountBallotModel
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

    public class CountBallotModelResponse
    {
        [Required]
        [Description("Number of Ballots")]
        [Range(0, long.MaxValue)]
        [JsonPropertyName("BallotCount")]
        [JsonProperty(nameof(BallotCount), Required = Required.Always)]
        public required long BallotCount { get; set; }
    }

    public class BallotModel
    {
        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("BallotId")]
        [JsonProperty(nameof(BallotId), Required = Required.Always)]
        [Key]
        public required string BallotId { get; set; }

        [Required]
        [MaxLength(2048)]
        [Description("Election for the Ballot")]
        [JsonPropertyName("Election")]
        [JsonProperty(nameof(Election), Required = Required.Always)]
        public required ElectionModel Election { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; }
    }

    public class SubmitBallotModel
    {
        [Required]
        [Description("Election")]
        [DataType("ElectionModel")]
        [JsonPropertyName("Election")]
        [JsonProperty(nameof(Election), Required = Required.Always)]
        [BallotIntegrityChecker(nameof(Election))]
        public required ElectionModel Election { get; set; }

        // We don't store the AccessCode with the ballot. That could identify the user that submitted the ballot. It's passed in with the payload, but stored decoupled. See SubmitBallot()
        [Required]
        [Description("Access Code")]
        [MaxLength(16)]
        [DataType(DataType.Text)]
        [JsonPropertyName("AccessCode")]
        [JsonProperty(nameof(AccessCode), Required = Required.Always)]
        public required string AccessCode { get; set; }

        // TODO Add Bindings of User / Ballot connection
        // Requires encryption for binding stored at client and server for match
        // public string UserId { get; set; }
        // public string UserIdBallotIdHashed { get; set; }
    }

    public class SubmitBallotModelResponse {
        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("BallotId")]
        [JsonProperty(nameof(BallotId), Required = Required.Always)]
        [Key]
        public required string BallotId { get; set; }

        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        public required string ElectionId { get; set; }

        [Required]
        [Description("Message")]
        [MaxLength(32768)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Message")]
        [JsonProperty(nameof(Message), Required = Required.Always)]
        public required string Message { get; set; }
    }

    public class BallotHashModel
    {
        [Required]
        [Description("Ballot Hash Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("BallotHashId")]
        [JsonProperty(nameof(BallotHashId), Required = Required.Always)]
        [Key]
        public required string BallotHashId { get; set; }

        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("BallotId")]
        [JsonProperty(nameof(BallotId), Required = Required.Always)]
        [ForeignKey("BallotId")]
        public required string BallotId { get; set; }

        [Required]
        [Description("Server Ballot Hash")]
        [DataType(DataType.Custom)]
        [JsonConverter(typeof(ByteConverter))]
        [JsonPropertyName("ServerBallotHash")]
        [JsonProperty(nameof(ServerBallotHash), Required = Required.Always)]
        public required byte[] ServerBallotHash { get; set; }

        [Required]
        [Description("Server Ballot Hash String")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ServerBallotHashS")]
        [JsonProperty(nameof(ServerBallotHashS), Required = Required.Always)]
        public required string ServerBallotHashS { get; set; }

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
        public required DateTime DateUpdated { get; set; }

        [Description("Timestamp Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("TimestampId")]
        [JsonProperty(nameof(TimestampId), Required = Required.Default)]
        [ForeignKey("TimestampId")]
        public string? TimestampId { get; set; }
    }

    public class FindBallotHashModel
    {
        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("BallotId")]
        [JsonProperty(nameof(BallotId), Required = Required.Always)]
        [Key]
        public required string BallotId { get; set; }
    }
}
