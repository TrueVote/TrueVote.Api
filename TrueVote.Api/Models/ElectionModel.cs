using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Models
{
    public class ElectionModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Elections")]
        [JsonPropertyName("Elections")]
        [JsonProperty(nameof(Elections), Required = Required.Always)]
        public required List<ElectionModel> Elections { get; set; }
    }

    public class AddRacesModel
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        [Key]
        public required string ElectionId { get; set; }

        [Required]
        [Description("Race Ids")]
        [JsonPropertyName("RaceIds")]
        [JsonProperty(nameof(RaceIds), Required = Required.Always)]
        public required List<string> RaceIds { get; set; } = new List<string>();
    }

    public class FindElectionModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;
    }

    public abstract class RootElectionBaseModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;

        [Required]
        [Description("Description")]
        [MaxLength(32768)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Description")]
        [JsonProperty(nameof(Description), Required = Required.Always)]
        public required string Description { get; set; } = string.Empty;

        [Required]
        [Description("HeaderImageUrl")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonPropertyName("HeaderImageUrl")]
        [JsonProperty(nameof(HeaderImageUrl), Required = Required.Always)]
        public required string HeaderImageUrl { get; set; } = string.Empty;

        [Required]
        [Description("StartDate")]
        [DataType(DataType.Date)]
        [JsonPropertyName("StartDate")]
        [JsonProperty(nameof(StartDate), Required = Required.Always)]
        public required DateTime StartDate { get; set; }

        [Required]
        [Description("EndDate")]
        [DataType(DataType.Date)]
        [JsonPropertyName("EndDate")]
        [JsonProperty(nameof(EndDate), Required = Required.Always)]
        public required DateTime EndDate { get; set; }
    }

    public class BaseElectionModel : RootElectionBaseModel
    {
        [Required]
        [Description("List of BaseRaces")]
        [DataType("List<BaseRaceModel>")]
        [JsonPropertyName("Races")]
        [JsonProperty(nameof(BaseRaces), Required = Required.Always)]
        public required List<BaseRaceModel> BaseRaces { get; set; } = new List<BaseRaceModel>();
    }

    public class ElectionModel : RootElectionBaseModel
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        [Key]
        public required string ElectionId { get; set; }

        [Description("Parent Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ParentElectionId")]
        [JsonProperty(nameof(ParentElectionId), Required = Required.Default)]
        public string? ParentElectionId { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; }

        [Required]
        [Description("List of Races")]
        [DataType("List<RaceModel>")]
        [JsonPropertyName("Races")]
        [JsonProperty(nameof(Races), Required = Required.Always)]
        public required List<RaceModel> Races { get; set; } = new List<RaceModel>();
    }

    public static class ElectionModelExtensions
    {
        public static ElectionModel DTOToElection(this BaseElectionModel baseElection, List<RaceModel> races)
        {
            var election = new ElectionModel
            {
                ElectionId = Guid.NewGuid().ToString(),
                Name = baseElection.Name,
                Description = baseElection.Description,
                HeaderImageUrl = baseElection.HeaderImageUrl,
                StartDate = baseElection.StartDate,
                EndDate = baseElection.EndDate,
                Races = races,
                DateCreated = UtcNowProviderFactory.GetProvider().UtcNow
            };

            return election;
        }
    }
}
