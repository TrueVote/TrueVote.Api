using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using TrueVote.Api2.Helpers;

namespace TrueVote.Api2.Models
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
        [JsonProperty(PropertyName = "Race")]
        public List<RaceModelResponse> race;
    }

    [ExcludeFromCodeCoverage]
    public class RaceModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Races")]
        [JsonProperty(PropertyName = "Races")]
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
        [JsonProperty(PropertyName = "Name")]
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
        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonProperty(PropertyName = "RaceType", Required = Required.Always)]
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
        [JsonProperty(PropertyName = "RaceId")]
        [Key]
        public string RaceId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonProperty(PropertyName = "RaceType", Required = Required.Always)]
        public RaceTypes RaceType { get; set; }

        [Required]
        [Description("Race Type Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [NotMapped]
        [JsonProperty(PropertyName = "RaceTypeName")]
        public string RaceTypeName => RaceType.ToString();

        [Required]
        [Description("Race Type Metadata")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "RaceTypeMetadata")]
        public string RaceTypeMetadata { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("List of Candidates")]
        [DataType("ICollection<CandidateModel>")]
        [JsonProperty(PropertyName = "Candidates", Required = Required.AllowNull)]
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
        [JsonProperty(PropertyName = "RaceId")]
        [Key]
        public string RaceId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonProperty(PropertyName = "RaceType")]
        public RaceTypes RaceType { get; set; }

        [Required]
        [Description("Race Type Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [NotMapped]
        [JsonProperty(PropertyName = "RaceTypeName")]
        public string RaceTypeName => RaceType.ToString();

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("List of Candidates")]
        [DataType("ICollection<CandidateModel>")]
        [JsonProperty(PropertyName = "Candidates")]
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
        [JsonProperty(PropertyName = "RaceId", Required = Required.Always)]
        public string RaceId { get; set; }

        [Required]
        [Description("Candidate Ids")]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "CandidateIds", Required = Required.Always)]
        public List<string> CandidateIds { get; set; }
    }
}
