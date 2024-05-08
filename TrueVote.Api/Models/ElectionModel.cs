using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;
using JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class ElectionObj
    {
        [JsonPropertyName("Election")]
        [JsonProperty("Election", Required = Required.Default)]
        public List<ElectionModelResponse>? election;
    }

    [ExcludeFromCodeCoverage]
    public class ElectionModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Elections")]
        [JsonPropertyName("Elections")]
        [JsonProperty(nameof(Elections), Required = Required.Always)]
        public required List<ElectionModel> Elections { get; set; }
    }

    [ExcludeFromCodeCoverage]
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

    [ExcludeFromCodeCoverage]
    public class BaseElectionModel
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

        [Required]
        [Description("List of Races")]
        [DataType("ICollection<RaceModel>")]
        [JsonPropertyName("Races")]
        [JsonProperty(nameof(Races), Required = Required.Always)]
        public required ICollection<RaceModel> Races { get; set; } = new List<RaceModel>();
    }

    [ExcludeFromCodeCoverage]
    public class ElectionModel
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
        [MaxLength(32768)]
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

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Always)]
        public required DateTime DateCreated { get; set; }

        [Required]
        [Description("List of Races")]
        [DataType("ICollection<RaceModel>")]
        [JsonPropertyName("Races")]
        [JsonProperty(nameof(Races), Required = Required.Always)]
        public required ICollection<RaceModel> Races { get; set; } = new List<RaceModel>();
    }

    // Same as above model but without required properties
    [ExcludeFromCodeCoverage]
    public class ElectionModelResponse
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        [Key]
        [JsonIgnore]
        public required string ElectionId { get; set; } = Guid.NewGuid().ToString();

        [Description("Parent Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ParentElectionId")]
        [JsonProperty(nameof(ParentElectionId), Required = Required.Default)]
        public string? ParentElectionId { get; set; }

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

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Always)]
        [JsonIgnore]
        public required DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("List of Races")]
        [DataType("ICollection<RaceModel>")]
        [JsonPropertyName("Races")]
        [JsonProperty(nameof(Races), Required = Required.Always)]
        public required ICollection<RaceModel> Races { get; set; } = new List<RaceModel>();
    }

    [ExcludeFromCodeCoverage]
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
        public required List<string> RaceIds { get; set; }
    }
}
