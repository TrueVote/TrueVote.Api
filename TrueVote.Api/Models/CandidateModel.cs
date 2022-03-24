using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class CandidateObj
    {
        public CandidateModel candidate;
    }

    [ExcludeFromCodeCoverage]
    public class CandidateModelList
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [MaxLength(2048)]
        [OpenApiProperty(Description = "List of Candidates")]
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
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Party Affiliation")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public string PartyAffiliation { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class BaseCandidateModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Party Affiliation")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
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
        [Key]
        public string CandidateId { get; set; } = Guid.NewGuid().ToString();

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Id (FK)")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [ForeignKey("RaceId")]
        public virtual string RaceId { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Party Affiliation")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public string PartyAffiliation { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
