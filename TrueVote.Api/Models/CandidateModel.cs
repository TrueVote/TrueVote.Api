using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [MaxLength(2048)]
        [OpenApiProperty(Description = "List of Candidates")]
        [JsonProperty(PropertyName = "Candidates")]
        public List<CandidateModel> Candidates { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindCandidateModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "PartyAffiliation")]
        public string PartyAffiliation { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class BaseCandidateModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "PartyAffiliation")]
        public string PartyAffiliation { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class CandidateModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Candidate Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "CandidateId")]
        [Key]
        public string CandidateId { get; set; } = Guid.NewGuid().ToString();

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "PartyAffiliation")]
        public string PartyAffiliation { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "CandidateImageUrl")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "CandidateImageUrl", Required = Required.Default)]
        public string CandidateImageUrl { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Selected")]
        [JsonProperty(PropertyName = "Selected")]
        public bool Selected { get; set; } = false;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "SelectedMetadata")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "SelectedMetadata", Required = Required.Default)]
        public string SelectedMetadata { get; set; } = string.Empty;

    }
}
