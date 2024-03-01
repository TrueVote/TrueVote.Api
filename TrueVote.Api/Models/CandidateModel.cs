using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class CandidateObj
    {
        [JsonProperty(PropertyName = "Candidate")]
        public List<CandidateModel> candidate;
    }

    [ExcludeFromCodeCoverage]
    public class CandidateModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Candidates")]
        [JsonProperty(PropertyName = "Candidates")]
        public List<CandidateModel> Candidates { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindCandidateModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "PartyAffiliation")]
        public string PartyAffiliation { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class BaseCandidateModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "PartyAffiliation")]
        public string PartyAffiliation { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class CandidateModel
    {
        [Required]
        [Description("Candidate Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "CandidateId")]
        [Key]
        public string CandidateId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Description("Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "PartyAffiliation")]
        public string PartyAffiliation { get; set; } = string.Empty;

        [Required]
        [Description("CandidateImageUrl")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "CandidateImageUrl", Required = Required.Default)]
        public string CandidateImageUrl { get; set; } = string.Empty;

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [Required]
        [Description("Selected")]
        [JsonProperty(PropertyName = "Selected")]
        public bool Selected { get; set; } = false;

        [Required]
        [Description("SelectedMetadata")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "SelectedMetadata", Required = Required.Default)]
        public string SelectedMetadata { get; set; } = string.Empty;

    }
}
