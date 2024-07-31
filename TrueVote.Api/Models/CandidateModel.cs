using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Models
{
    public class CandidateObj
    {
        [JsonPropertyName("Candidate")]
        [JsonProperty("Candidate", Required = Required.Default)]
        public List<CandidateModel>? candidate;
    }

    public class CandidateModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Candidates")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Always)]
        public required List<CandidateModel> Candidates { get; set; }
    }

    public class FindCandidateModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;

        [Description("Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("PartyAffiliation")]
        [JsonProperty(nameof(PartyAffiliation), Required = Required.Default)]
        public string PartyAffiliation { get; set; } = string.Empty;
    }

    public abstract class RootCandidateBaseModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;

        [Description("Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("PartyAffiliation")]
        [JsonProperty(nameof(PartyAffiliation), Required = Required.Default)]
        public string PartyAffiliation { get; set; } = string.Empty;

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
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; }

        [Required]
        [Description("Selected")]
        [JsonPropertyName("Selected")]
        [JsonProperty(nameof(Selected), Required = Required.Default)]
        public required bool Selected { get; set; } = false;

        [Description("SelectedMetadata")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonPropertyName("SelectedMetadata")]
        [JsonProperty(nameof(SelectedMetadata), Required = Required.Default)]
        public string SelectedMetadata { get; set; } = string.Empty;
    }

    public class BaseCandidateModel : RootCandidateBaseModel { }

    public class CandidateModel : RootCandidateBaseModel
    {
        [Required]
        [Description("Candidate Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("CandidateId")]
        [JsonProperty(nameof(CandidateId), Required = Required.Default)]
        [Key]
        public required string CandidateId { get; set; }
    }

    public static class CandidateModelExtensions
    {
        public static List<CandidateModel> DTOToCandidates(this List<BaseCandidateModel> baseCandidates)
        {
            return baseCandidates.Select(DTOToCandidate).ToList();
        }

        public static CandidateModel DTOToCandidate(this BaseCandidateModel baseCandidateModel)
        {
            return new CandidateModel
            {
                CandidateId = Guid.NewGuid().ToString(),
                Name = baseCandidateModel.Name,
                DateCreated = UtcNowProviderFactory.GetProvider().UtcNow,
                PartyAffiliation = baseCandidateModel.PartyAffiliation,
                CandidateImageUrl = baseCandidateModel.CandidateImageUrl,
                Selected = baseCandidateModel.Selected,
                SelectedMetadata = baseCandidateModel.SelectedMetadata
            };
        }
    }
}
