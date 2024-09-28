using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    public class RaceModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Races")]
        [JsonPropertyName("Races")]
        public required List<RaceModel> Races { get; set; }
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

    public abstract class RootRaceBaseModel
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

        [Required]
        [Description("Race Type Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [NotMapped]
        [JsonPropertyName("RaceTypeName")]
        [JsonProperty(nameof(RaceTypeName), Required = Required.Default)]
        public string RaceTypeName => RaceType.ToString();

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; }
    }

    public class BaseRaceModel : RootRaceBaseModel
    {
        [Description("Max Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [JsonPropertyName("MaxNumberOfChoices")]
        [JsonProperty(nameof(MaxNumberOfChoices), Required = Required.Always)]
        public required int MaxNumberOfChoices { get; set; }

        [Description("Min Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [JsonPropertyName("MinNumberOfChoices")]
        [JsonProperty(nameof(MinNumberOfChoices), Required = Required.Always)]
        public required int MinNumberOfChoices { get; set; }

        [Required]
        [Description("List of BaseCandidates")]
        [DataType("List<BaseCandidateModel>")]
        [JsonPropertyName("BaseCandidates")]
        [JsonProperty(nameof(BaseCandidates), Required = Required.Always)]
        public required List<BaseCandidateModel> BaseCandidates { get; set; } = new List<BaseCandidateModel>();
    }

    public class RaceModel : RootRaceBaseModel
    {
        [Required]
        [Description("Race Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RaceId")]
        [JsonProperty(nameof(RaceId), Required = Required.Default)]
        [Key]
        public required string RaceId { get; set; }

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
        [Description("List of Candidates")]
        [DataType("List<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Always)]
        public required List<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();
    }

    public static class RaceModelExtensions
    {
        public static List<RaceModel> DTOToRaces(this List<BaseRaceModel> baseRaces)
        {
            return baseRaces.Select(DTOToRace).ToList();
        }

        public static RaceModel DTOToRace(this BaseRaceModel baseRaceModel)
        {
            return new RaceModel
            {
                RaceId = Guid.NewGuid().ToString(),
                Name = baseRaceModel.Name,
                DateCreated = UtcNowProviderFactory.GetProvider().UtcNow,
                Candidates = baseRaceModel.BaseCandidates.DTOToCandidates(),
                MaxNumberOfChoices = baseRaceModel.MaxNumberOfChoices,
                MinNumberOfChoices = baseRaceModel.MinNumberOfChoices,
                RaceType = baseRaceModel.RaceType
            };
        }
    }
}
