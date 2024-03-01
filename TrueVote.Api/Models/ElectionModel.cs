using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class ElectionObj
    {
        [JsonPropertyName("Election")]
        public List<ElectionModelResponse> election;
    }

    [ExcludeFromCodeCoverage]
    public class ElectionModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Elections")]
        [JsonPropertyName("Elections")]
        public List<ElectionModel> Elections { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindElectionModel
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
    public class BaseElectionModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Description")]
        [MaxLength(32768)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Description("HeaderImageUrl")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonPropertyName("HeaderImageUrl")]
        public string HeaderImageUrl { get; set; } = string.Empty;

        [Required]
        [Description("StartDate")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("StartDate")]
        public DateTime? StartDate { get; set; }

        [Required]
        [Description("EndDate")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("EndDate")]
        public DateTime? EndDate { get; set; }

        [Required]
        [Description("List of Races")]
        [DataType("ICollection<RaceModel>")]
        [JsonPropertyName("Races")]
        public ICollection<RaceModel> Races { get; set; } = new List<RaceModel>();
    }

    [ExcludeFromCodeCoverage]
    public class ElectionModel
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("ElectionId")]
        [Key]
        public string ElectionId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Parent Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("ParentElectionId")]
        public string ParentElectionId { get; set; }

        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Description")]
        [MaxLength(32768)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Description("HeaderImageUrl")]
        [MaxLength(32768)]
        [DataType(DataType.Text)]
        [JsonPropertyName("HeaderImageUrl")]
        public string HeaderImageUrl { get; set; } = string.Empty;

        [Required]
        [Description("StartDate")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("StartDate")]
        public DateTime? StartDate { get; set; }

        [Required]
        [Description("EndDate")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("EndDate")]
        public DateTime? EndDate { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("List of Races")]
        [DataType("ICollection<RaceModel>")]
        [JsonPropertyName("Races")]
        public ICollection<RaceModel> Races { get; set; } = new List<RaceModel>();
    }

    // Same as above model but without required properties
    [ExcludeFromCodeCoverage]
    public class ElectionModelResponse
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("ElectionId")]
        [Key]
        public string ElectionId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Parent Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("ParentElectionId")]
        public string ParentElectionId { get; set; }

        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Description")]
        [MaxLength(32768)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Description("StartDate")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("StartDate")]
        public DateTime? StartDate { get; set; }

        [Required]
        [Description("EndDate")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("EndDate")]
        public DateTime? EndDate { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("List of Races")]
        [DataType("ICollection<RaceModel>")]
        [JsonPropertyName("Races")]
        public ICollection<RaceModel> Races { get; set; } = new List<RaceModel>();
    }

    [ExcludeFromCodeCoverage]
    public class AddRacesModel
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("ElectionId")]
        [Key]
        public string ElectionId { get; set; }

        [Required]
        [Description("Race Ids")]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("RaceIds")]
        public List<string> RaceIds { get; set; }
    }
}
