using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api.Helpers;
using Nostr.Client.Messages;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class UserObj
    {
        [JsonPropertyName("User")]
        public List<UserModel> user;
    }

    [ExcludeFromCodeCoverage]
    public class UserModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Users")]
        [JsonPropertyName("Users")]
        public List<UserModel> Users { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class FindUserModel
    {
        [Required]
        [Description("First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Description("Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonPropertyName("Email")]
        public string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BaseUserModel
    {
        [Required]
        [Description("First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [Description("Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonPropertyName("Email")]
        public string Email { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UserModel
    {
        [Required]
        [Description("User Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("UserId")]
        [Key]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Nostr PubKey")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("NostrPubKey")]
        public string NostrPubKey { get; set; } = string.Empty;

        [Required]
        [Description("First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [Description("Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;
    }

    [ExcludeFromCodeCoverage]
    public class SignInEventModel
    {
        [Required]
        [Description("Kind")]
        [EnumDataType(typeof(NostrKind))]
        [JsonPropertyName("Kind")]
        public NostrKind Kind { get; set; }

        [Required]
        [Description("PubKey")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("PubKey")]
        public string PubKey { get; set; }

        [Required]
        [Description("CreatedAt")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonPropertyName("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Description("Signature")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Signature")]
        public string Signature { get; set; }

        [Required]
        [Description("Content")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Content")]
        public string Content { get; set; }
    }
}
