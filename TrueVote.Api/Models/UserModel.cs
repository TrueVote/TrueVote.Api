using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Nostr.Client.Messages;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class UserObj
    {
        [JsonPropertyName("User")]
        [JsonProperty("User", Required = Required.Default)]
        public List<UserModel>? user;
    }

    [ExcludeFromCodeCoverage]
    public class UserModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Users")]
        [JsonPropertyName("Users")]
        [JsonProperty(nameof(Users), Required = Required.Always)]
        public required List<UserModel> Users { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindUserModel
    {
        [Required]
        [Description("First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("FirstName")]
        [JsonProperty(nameof(FirstName), Required = Required.Always)]
        public required string FirstName { get; set; } = string.Empty;

        [Required]
        [Description("Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonPropertyName("Email")]
        [JsonProperty(nameof(Email), Required = Required.Always)]
        public required string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BaseUserModel
    {
        [Required]
        [Description("First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("FirstName")]
        [JsonProperty(nameof(FirstName), Required = Required.Always)]
        public required string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [Description("Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonPropertyName("Email")]
        [JsonProperty(nameof(Email), Required = Required.Always)]
        public required string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UserModel
    {
        [Required]
        [Description("User Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("UserId")]
        [JsonProperty(nameof(UserId), Required = Required.Always)]
        [Key]
        public required string UserId { get; set; }

        [Required]
        [Description("Nostr PubKey")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("NostrPubKey")]
        [JsonProperty(nameof(NostrPubKey), Required = Required.Always)]
        public required string NostrPubKey { get; set; }

        [Required]
        [Description("First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("FirstName")]
        [JsonProperty(nameof(FirstName), Required = Required.Always)]
        public required string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [Description("Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonPropertyName("Email")]
        [JsonProperty(nameof(Email), Required = Required.Always)]
        public required string Email { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Always)]
        public required DateTime DateCreated { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SignInEventModel
    {
        [Required]
        [Description("Kind")]
        [EnumDataType(typeof(NostrKind))]
        [JsonPropertyName("Kind")]
        [JsonProperty(nameof(Kind), Required = Required.Always)]
        public required NostrKind Kind { get; set; }

        [Required]
        [Description("PubKey")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("PubKey")]
        [JsonProperty(nameof(PubKey), Required = Required.Always)]
        public required string PubKey { get; set; }

        [Required]
        [Description("CreatedAt")]
        [DataType(DataType.Date)]
        [JsonPropertyName("CreatedAt")]
        [JsonProperty(nameof(CreatedAt), Required = Required.Always)]
        public required DateTime CreatedAt { get; set; }

        [Required]
        [Description("Signature")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Signature")]
        [JsonProperty(nameof(Signature), Required = Required.Always)]
        public required string Signature { get; set; }

        [Required]
        [Description("Content")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Content")]
        [JsonProperty(nameof(Content), Required = Required.Always)]
        public required string Content { get; set; }
    }
}
