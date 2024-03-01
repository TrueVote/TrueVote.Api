using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api2.Helpers;
using Nostr.Client.Messages;
using System.ComponentModel;

namespace TrueVote.Api2.Models
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
        [Required]
        [MaxLength(2048)]
        [Description("List of Users")]
        [JsonProperty(PropertyName = "Users")]
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
        [JsonProperty(PropertyName = "FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Description("Email Address")]
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
        [Required]
        [Description("First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "FirstName", Required = Required.Always)]
        public string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [Description("Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonProperty(PropertyName = "Email", Required = Required.Always)]
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
        [JsonProperty(PropertyName = "UserId")]
        [Key]
        public string UserId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Nostr PubKey")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "NostrPubKey")]
        public string NostrPubKey { get; set; } = string.Empty;

        [Required]
        [Description("First Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [RegularExpression(Constants.GenericStringRegex)]
        [JsonProperty(PropertyName = "FirstName", Required = Required.Always)]
        public string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [Description("Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonProperty(PropertyName = "Email", Required = Required.Always)]
        public string Email { get; set; }

        [Required]
        [Description("DateCreated")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "DateCreated")]
        public DateTime DateCreated { get; set; } = UtcNowProviderFactory.GetProvider().UtcNow;
    }

    [ExcludeFromCodeCoverage]
    public class SignInEventModel
    {
        [Required]
        [Description("Kind")]
        [EnumDataType(typeof(NostrKind))]
        [JsonProperty(PropertyName = "Kind", Required = Required.Always)]
        public NostrKind Kind { get; set; }

        [Required]
        [Description("PubKey")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "PubKey", Required = Required.Always)]
        public string PubKey { get; set; }

        [Required]
        [Description("CreatedAt")]
        [MaxLength(2048)]
        [DataType(DataType.Date)]
        [JsonProperty(PropertyName = "CreatedAt", Required = Required.Always)]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Description("Signature")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "Signature", Required = Required.Always)]
        public string Signature { get; set; }

        [Required]
        [Description("Content")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonProperty(PropertyName = "Content", Required = Required.Always)]
        public string Content { get; set; }
    }
}
