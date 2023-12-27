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
        [DataType(DataType.Custom)]
        [JsonProperty(PropertyName = "Kind")]
        public StringWrapper Kind { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "PubKey")]
        [DataType(DataType.Custom)]
        [JsonProperty(PropertyName = "PubKey")]
        public PubKeyWrapper PubKey { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "CreatedAt")]
        [DataType(DataType.Custom)]
        [JsonProperty(PropertyName = "CreatedAt")]
        public UInt64Wrapper CreatedAt { get; set; }

        [OpenApiSchemaVisibility(OpenApiVisibilityType.Important)]
        [OpenApiProperty(Description = "Signature")]
        [DataType(DataType.Custom)]
        [JsonConverter(typeof(ByteConverter))]
        [JsonProperty(PropertyName = "Signature")]
        public byte[] Signature { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class StringWrapper : IBitcoinSerializable
    {
        public string Value { get; set; }

        public void ReadWrite(BitcoinStream stream)
        {
            stream.ReadWrite(Encoding.UTF8.GetBytes(Value));
        }
    }

    [ExcludeFromCodeCoverage]
    public class UInt64Wrapper : IBitcoinSerializable
    {
        public ulong Value { get; set; }

        public void ReadWrite(BitcoinStream stream)
        {
            stream.ReadWrite(Value);
        }
    }

    [ExcludeFromCodeCoverage]
    public class PubKeyWrapper : IBitcoinSerializable
    {
        public byte[] Value { get; set; }

        public void ReadWrite(BitcoinStream stream)
        {
            stream.ReadWrite(Value);
        }
    }
}
