using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace TrueVote.Api.Models
{
    // TODO Need to describe this for SwaggerUI
    // GraphQL spec is reason for ALL_CAPS below (https://github.com/graphql-dotnet/graphql-dotnet/pull/2773)
    public enum RaceTypes
    {
        [EnumMember(Value = "CHOOSE_ONE")]
        ChooseOne = 0,
        [EnumMember(Value = "CHOOSE_MANY")]
        ChooseMany = 1
    }

    [ExcludeFromCodeCoverage]
    public class RaceObj
    {
        [JsonProperty(PropertyName = "Race")]
        public List<RaceModelResponse> race;
    }

    [ExcludeFromCodeCoverage]
    public class RaceModelList
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [MaxLength(2048)]
        [OpenApiProperty(Description = "List of Races")]
        [JsonProperty(PropertyName = "Races")]
        public List<RaceModel> Races { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindRaceModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class BaseRaceModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonProperty(PropertyName = "RaceType", Required = Required.Always)]
        public RaceTypes RaceType { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class RaceModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "RaceId")]
        [Key]
        public string RaceId { get; set; } = Guid.NewGuid().ToString();

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonProperty(PropertyName = "RaceType", Required = Required.Always)]
        public RaceTypes RaceType { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Type Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [NotMapped]
        [JsonProperty(PropertyName = "RaceTypeName")]
        public string RaceTypeName => RaceType.ToString();

        private DateTime _DateCreated;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated
        {
            get => _DateCreated = _DateCreated == DateTime.MinValue ? DateTime.UtcNow : _DateCreated;
            set => _DateCreated = value;
        }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "List of Candidates")]
        [DataType("ICollection<CandidateModel>")]
        [JsonProperty(PropertyName = "Candidates", Required = Required.AllowNull)]
        public ICollection<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();
    }

    // Same as above model but without required properties
    [ExcludeFromCodeCoverage]
    public class RaceModelResponse
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "RaceId")]
        [Key]
        public string RaceId { get; set; } = Guid.NewGuid().ToString();

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Type")]
        [EnumDataType(typeof(RaceTypes))]
        [JsonProperty(PropertyName = "RaceType")]
        public RaceTypes RaceType { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Type Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [NotMapped]
        [JsonProperty(PropertyName = "RaceTypeName")]
        public string RaceTypeName => RaceType.ToString();

        private DateTime _DateCreated;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated
        {
            get => _DateCreated = _DateCreated == DateTime.MinValue ? DateTime.UtcNow : _DateCreated;
            set => _DateCreated = value;
        }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "List of Candidates")]
        [DataType("ICollection<CandidateModel>")]
        [JsonProperty(PropertyName = "Candidates")]
        public ICollection<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();
    }

    [ExcludeFromCodeCoverage]
    public class AddCandidatesModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "RaceId", Required = Required.Always)]
        public string RaceId { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Candidate Ids")]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "CandidateIds", Required = Required.Always)]
        public List<string> CandidateIds { get; set; }
    }
}
