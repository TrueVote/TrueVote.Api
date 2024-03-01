using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Models
{
    // TODO Need to describe this for SwaggerUI
    // GraphQL spec is reason for ALL_CAPS below (https://github.com/graphql-dotnet/graphql-dotnet/pull/2773)
    public enum RaceTypes
    {
        [EnumMember(Value = "CHOOSE_ONE")]
        ChooseOne = 0,
        [EnumMember(Value = "CHOOSE_MANY")]
        ChooseMany = 1,
        [EnumMember(Value = "RANKED_CHOICE")]
        RankedChoice = 2
    }

    [ExcludeFromCodeCoverage]
    public class RaceObj
    {
        [JsonPropertyName("Race")]
        public List<RaceModelResponse> race;
    }

    [ExcludeFromCodeCoverage]
    public class RaceModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Races")]
        [JsonPropertyName("Races")]
        public List<RaceModel> Races { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindRaceModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class BaseRaceModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonPropertyName("RaceType")]
        public RaceTypes RaceType { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class RaceModel
    {
        [Required]
        [Description("Race Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("RaceId")]
        [Key]
        public string RaceId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonPropertyName("RaceType")]
        public RaceTypes RaceType { get; set; }

        [Required]
        [Description("Race Type Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [NotMapped]
        [JsonPropertyName("RaceTypeName")]
        public string RaceTypeName => RaceType.ToString();

        [Required]
        [Description("Race Type Metadata")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("RaceTypeMetadata")]
        public string RaceTypeMetadata { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Description("List of Candidates")]
        [DataType("ICollection<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        public ICollection<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();
    }

    // Same as above model but without required properties
    [ExcludeFromCodeCoverage]
    public class RaceModelResponse
    {
        [Required]
        [Description("Race Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("RaceId")]
        [Key]
        public string RaceId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonPropertyName("RaceType")]
        public RaceTypes RaceType { get; set; }

        [Required]
        [Description("Race Type Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [NotMapped]
        [JsonPropertyName("RaceTypeName")]
        public string RaceTypeName => RaceType.ToString();

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("List of Candidates")]
        [DataType("ICollection<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        public ICollection<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();
    }

    [ExcludeFromCodeCoverage]
    public class AddCandidatesModel
    {
        [Required]
        [Description("Race Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("RaceId")]
        public string RaceId { get; set; }

        [Required]
        [Description("Candidate Ids")]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("CandidateIds")]
        public List<string> CandidateIds { get; set; }
    }
}
