using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;
using ByteConverter = TrueVote.Api.Helpers.ByteConverter;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;
using JsonConverterAttribute = System.Text.Json.Serialization.JsonConverterAttribute;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class BallotList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Ballots")]
        [JsonPropertyName("Ballots")]
        public List<BallotModel> Ballots { get; set; }

        [Required]
        [MaxLength(2048)]
        [Description("List of Ballot Hashes")]
        [JsonPropertyName("BallotHashes")]
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
        [JsonPropertyName("BallotId")]
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
        [JsonPropertyName("DateCreatedStart")]
        public DateTime DateCreatedStart { get; set; }

        [BindRequired]
        [Required]
        [Description("DateCreatedEnd")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreatedEnd")]
        public DateTime DateCreatedEnd { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CountBallotModelResponse
    {
        [Required]
        [Description("Number of Ballots")]
        [Range(0, long.MaxValue)]
        [JsonPropertyName("BallotCount")]
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
        [JsonPropertyName("BallotId")]
        [Key]
        public string BallotId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(2048)]
        [Description("Election for the Ballot")]
        [JsonPropertyName("Election")]
        public ElectionModel Election { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

    }

    [ExcludeFromCodeCoverage]
    public class SubmitBallotModel {
        [Required]
        [Description("Election")]
        [DataType("ElectionModel")]
        [JsonPropertyName("Election")]
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
        [JsonPropertyName("BallotId")]
        [Key]
        public string BallotId { get; set; }

        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("ElectionId")]
        public string ElectionId { get; set; }

        [Required]
        [Description("Message")]
        [MaxLength(32768)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("Message")]
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
        [JsonPropertyName("BallotHashId")]
        [Key]
        public string BallotHashId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("BallotId")]
        [ForeignKey("BallotId")]
        public string BallotId { get; set; }

        [Required]
        [Description("Server Ballot Hash")]
        [DataType(DataType.Custom)]
        [JsonConverter(typeof(ByteConverter))]
        [JsonPropertyName("ServerBallotHash")]
        public byte[] ServerBallotHash { get; set; }

        [Required]
        [Description("Server Ballot Hash String")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("ServerBallotHashS")]
        public string ServerBallotHashS { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("DateUpdated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateUpdated")]
        public DateTime DateUpdated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("Timestamp Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("TimestampId")]
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
        [JsonPropertyName("BallotId")]
        [Key]
        public string BallotId { get; set; } = Guid.NewGuid().ToString();
    }
}
