using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api.Helpers;
using System.Text;
using Nostr.Client.Messages;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class UserObj
    {
        [JsonProperty(PropertyName = "User")]
        public List<UserModel> user;
    }

    [ExcludeFromCodeCoverage]
    public class UserModelList
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [MaxLength(2048)]
        [OpenApiProperty(Description = "List of Users")]
        [JsonProperty(PropertyName = "Users")]
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
        [JsonProperty(PropertyName = "FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonProperty(PropertyName = "Email")]
        public string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BaseUserModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "FirstName", Required = Required.Always)]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Email Address")]
        [MaxLength(2048)]
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonProperty(PropertyName = "Email", Required = Required.Always)]
        public string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UserModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "User Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "UserId")]
        [Key]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Nostr PubKey")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "NostrPubKey")]
        public string NostrPubKey { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "FirstName", Required = Required.Always)]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Email Address")]
        [MaxLength(2048)]
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonProperty(PropertyName = "Email", Required = Required.Always)]
        public string Email { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;
    }

    [ExcludeFromCodeCoverage]
    public class SignInEventModel
    {
        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Kind")]
        [EnumDataType(typeof(NostrKind))]
        [JsonProperty(PropertyName = "Kind", Required = Required.Always)]
        public NostrKind Kind { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "PubKey")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "PubKey", Required = Required.Always)]
        public string PubKey { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "CreatedAt")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "CreatedAt", Required = Required.Always)]
        public string CreatedAt { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Signature")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "Signature", Required = Required.Always)]
        public string Signature { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Content")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "Content", Required = Required.Always)]
        public string Content { get; set; }
    }
}
