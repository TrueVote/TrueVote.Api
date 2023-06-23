using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class BallotModelList
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [MaxLength(2048)]
        [OpenApiProperty(Description = "List of Ballots")]
        [JsonProperty(PropertyName = "Ballots")]
        public List<BallotModel> Ballots { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindBallotModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "BallotId")]
        [Key]
        public string BallotId { get; set; } = Guid.NewGuid().ToString();
    }

    [ExcludeFromCodeCoverage]
    public class BallotModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "BallotId")]
        [Key]
        public string BallotId { get; set; } = Guid.NewGuid().ToString();

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "ElectionId")]
        public string ElectionId { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [MaxLength(2048)]
        [OpenApiProperty(Description = "Election for the Ballot")]
        [JsonProperty(PropertyName = "Election")]
        public ElectionModel Election { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SubmitBallotModel {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "ElectionId")]
        [Key]
        public string ElectionId { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Election")]
        [DataType("ElectionModel")]
        [JsonProperty(PropertyName = "Election", Required = Required.Always)]
        public ElectionModel Election { get; set; }

        // TODO Add Bindings of User / Ballot connection
        // Requires encryption for binding stored at client and server for match
        // public string UserId { get; set; }
        // public string UserIdBallotIdHashed { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SubmitBallotModelResponse {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Ballot Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "BallotId")]
        [Key]
        public string BallotId { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "ElectionId")]
        public string ElectionId { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Message")]
        [MaxLength(32768)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }
    }
}
