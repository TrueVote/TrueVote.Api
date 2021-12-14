using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class FindUserModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "First Name")]
        [StringLength(50)]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Email Address")]
        [StringLength(200)]
        [MaxLength(200)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BaseUserModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "First Name")]
        [JsonProperty(Required = Newtonsoft.Json.Required.Always)]
        [StringLength(50)]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Email Address")]
        [JsonProperty(Required = Newtonsoft.Json.Required.Always)]
        [StringLength(2000)]
        [MaxLength(2000)]
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UserModel: BaseUserModel
    {
        public UserModel(BaseUserModel baseUser)
        {
            FirstName = baseUser?.FirstName;
            Email = baseUser?.Email;
        }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "GUID Id")]
        [StringLength(40)]
        [MaxLength(40)]
        [DataType(DataType.Text)]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreated")]
        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {

        }
    }
}
