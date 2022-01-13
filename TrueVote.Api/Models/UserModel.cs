using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class UserObj
    {
        public UserModel user;
    }

    [ExcludeFromCodeCoverage]
    public class UserModelList
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [MaxLength(2048)]
        [OpenApiProperty(Description = "List of Users")]
        public List<UserModel> Users { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindUserModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        public string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BaseUserModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "First Name")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Email Address")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        public string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UserModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "First Name")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Email Address")]
        [JsonProperty(Required = Required.Always)]
        [MaxLength(2048)]
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        public string Email { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "GUID Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [Key]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {

        }
    }
}
