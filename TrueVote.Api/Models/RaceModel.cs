using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;
using JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

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

    public class RaceModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Races")]
        [JsonPropertyName("Races")]
        public required List<RaceModel> Races { get; set; }
    }

    public class FindRaceModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;
    }

    public class BaseRaceModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;

        [Required]
        [Description("Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonPropertyName("RaceType")]
        [JsonProperty(nameof(RaceType), Required = Required.Always)]
        public required RaceTypes RaceType { get; set; }
    }

    public class RaceModel
    {
        [Required]
        [Description("Race Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RaceId")]
        [JsonProperty(nameof(RaceId), Required = Required.Default)]
        [Key]
        [JsonIgnore]
        public required string RaceId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;

        [Required]
        [Description("Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonPropertyName("RaceType")]
        [JsonProperty(nameof(RaceType), Required = Required.Always)]
        public required RaceTypes RaceType { get; set; }

        [Required]
        [Description("Race Type Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [NotMapped]
        [JsonPropertyName("RaceTypeName")]
        [JsonProperty(nameof(RaceTypeName), Required = Required.Default)]
        public string RaceTypeName => RaceType.ToString();

        [Description("Max Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [JsonPropertyName("MaxNumberOfChoices")]
        [JsonProperty(nameof(MaxNumberOfChoices), Required = Required.Default)]
        [MaxNumberOfChoicesValidator(nameof(Candidates), nameof(Name))]
        public int? MaxNumberOfChoices { get; set; }

        [Description("Min Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [JsonPropertyName("MinNumberOfChoices")]
        [JsonProperty(nameof(MinNumberOfChoices), Required = Required.Default)]
        [MinNumberOfChoicesValidator(nameof(Candidates), nameof(Name))]
        public int? MinNumberOfChoices { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        [JsonIgnore]
        public required DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Description("List of Candidates")]
        [DataType("List<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Default)]
        public List<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();
    }

    public class AddCandidatesModel
    {
        [Required]
        [Description("Race Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RaceId")]
        [JsonProperty(nameof(RaceId), Required = Required.Always)]
        public required string RaceId { get; set; }

        [Required]
        [Description("Candidate Ids")]
        [JsonPropertyName("CandidateIds")]
        [JsonProperty(nameof(CandidateIds), Required = Required.Always)]
        public required List<string> CandidateIds { get; set; }
    }
}
