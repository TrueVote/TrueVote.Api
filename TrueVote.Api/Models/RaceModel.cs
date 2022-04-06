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
    public enum RaceTypes
    {
        [EnumMember(Value = "Choose One")]
        ChooseOne = 0,
        [EnumMember(Value = "Choose Many")]
        ChooseMany = 1
    }

    [ExcludeFromCodeCoverage]
    public class RaceObj
    {
        public RaceModel race;
    }

    [ExcludeFromCodeCoverage]
    public class RaceModelList
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [MaxLength(2048)]
        [OpenApiProperty(Description = "List of Races")]
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
        public string Name { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class BaseRaceModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Type")]
        [JsonProperty(Required = Required.Always)]
        [EnumDataType(typeof(RaceTypes))]
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
        [Key]
        public string RaceId { get; set; } = Guid.NewGuid().ToString();

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Name")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public string Name { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Type")]
        [JsonProperty(Required = Required.Always)]
        [EnumDataType(typeof(RaceTypes))]
        public RaceTypes RaceType { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Type Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [NotMapped]
        public string RaceTypeName { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "List of Candidates")]
        [JsonProperty(Required = Required.AllowNull)]
        [DataType("ICollection<CandidateModel>")]
        public ICollection<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            RaceTypeName = RaceType.ToString();
        }
    }

    [ExcludeFromCodeCoverage]
    public class AddCandidatesModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Race Id")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public string RaceId { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Candidate Ids")]
        [JsonProperty(Required = Required.Always)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public List<string> CandidateIds { get; set; }
    }
}
