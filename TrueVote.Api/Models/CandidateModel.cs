using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class CandidateObj
    {
        [JsonPropertyName("Candidate")]
        [JsonProperty("Candidate", Required = Required.Default)]
        public List<CandidateModel>? candidate;
    }

    [ExcludeFromCodeCoverage]
    public class CandidateModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Candidates")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Always)]
        public required List<CandidateModel> Candidates { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindCandidateModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;

        [Required]
        [Description("Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("PartyAffiliation")]
        [JsonProperty(nameof(PartyAffiliation), Required = Required.Always)]
        public required string PartyAffiliation { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class BaseCandidateModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty("Candidates", Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;

        [Required]
        [Description("Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("PartyAffiliation")]
        [JsonProperty(nameof(PartyAffiliation), Required = Required.Always)]
        public required string PartyAffiliation { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class CandidateModel
    {
        [Required]
        [Description("Candidate Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("CandidateId")]
        [JsonProperty(nameof(CandidateId), Required = Required.Always)]
        [Key]
        public required string CandidateId { get; set; }

        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;

        [Required]
        [Description("Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("PartyAffiliation")]
        [JsonProperty(nameof(PartyAffiliation), Required = Required.Always)]
        public required string PartyAffiliation { get; set; } = string.Empty;

        [Description("CandidateImageUrl")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonPropertyName("CandidateImageUrl")]
        [JsonProperty(nameof(CandidateImageUrl), Required = Required.Default)]
        public string CandidateImageUrl { get; set; } = string.Empty;

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Always)]
        public required DateTime DateCreated { get; set; }

        [Description("Selected")]
        [JsonPropertyName("Selected")]
        [JsonProperty(nameof(Selected), Required = Required.Default)]
        public bool Selected { get; set; } = false;

        [Description("SelectedMetadata")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonPropertyName("SelectedMetadata")]
        [JsonProperty(nameof(SelectedMetadata), Required = Required.Default)]
        public string SelectedMetadata { get; set; } = string.Empty;

    }
}
