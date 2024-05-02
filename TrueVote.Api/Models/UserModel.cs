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
        [Description("Full Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("FullName")]
        [JsonProperty(nameof(FullName), Required = Required.Always)]
        public required string FullName { get; set; } = string.Empty;

        [Description("Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonPropertyName("Email")]
        [JsonProperty(nameof(Email), Required = Required.Default)]
        public string Email { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class BaseUserModel
    {
        [Required]
        [Description("Full Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("FullName")]
        [JsonProperty(nameof(FullName), Required = Required.Always)]
        public required string FullName { get; set; } = string.Empty;

        private string? _email;

        [Required(AllowEmptyStrings = false)]
        [Description("Email Address")]
        [MaxLength(2048)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression(Constants.EMailRegex)]
        [JsonPropertyName("Email")]
        [JsonProperty(nameof(Email), Required = Required.Always)]
        public required string Email
        {
            get => string.IsNullOrWhiteSpace(_email) ? "unknown@truevote.org" : _email;
            set => _email = value;
        }

        [Required]
        [Description("Nostr Public Key")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("NostrPubKey")]
        [JsonProperty(nameof(NostrPubKey), Required = Required.Always)]
        public required string NostrPubKey { get; set; }
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
        [Description("Full Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("FullName")]
        [JsonProperty(nameof(FullName), Required = Required.Always)]
        public required string FullName { get; set; } = string.Empty;

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

        [Required]
        [Description("DateUpdated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateUpdated")]
        [JsonProperty(nameof(DateUpdated), Required = Required.DisallowNull)]
        public DateTime DateUpdated { get; set; }

        [Required]
        [Description("UserPreferences")]
        [DataType("UserPreferencesModel")]
        [JsonPropertyName("UserPreferences")]
        [JsonProperty(nameof(UserPreferences), Required = Required.Always)]
        public required UserPreferencesModel UserPreferences { get; set; } = new UserPreferencesModel();
    }

    [ExcludeFromCodeCoverage]
    public class SignInResponse
    {
        [Required]
        [Description("User")]
        [DataType("UserModel")]
        [JsonPropertyName("User")]
        [JsonProperty(nameof(User), Required = Required.Always)]
        public required UserModel User { get; set; }

        [Required]
        [Description("Token")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Token")]
        [JsonProperty(nameof(Token), Required = Required.Always)]
        public required string Token { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UserPreferencesModel
    {
        [Description("Notification: New Elections")]
        [JsonPropertyName("NotificationNewElections")]
        [JsonProperty(nameof(NotificationNewElections), Required = Required.Default)]
        public bool NotificationNewElections { get; set; }

        [Description("Notification: Election Start")]
        [JsonPropertyName("NotificationElectionStart")]
        [JsonProperty(nameof(NotificationElectionStart), Required = Required.Default)]
        public bool NotificationElectionStart { get; set; }

        [Description("Notification: Election End")]
        [JsonPropertyName("NotificationElectionEnd")]
        [JsonProperty(nameof(NotificationElectionEnd), Required = Required.Default)]
        public bool NotificationElectionEnd { get; set; }

        [Description("Notification: New TrueVote Features")]
        [JsonPropertyName("NotificationNewTrueVoteFeatures")]
        [JsonProperty(nameof(NotificationNewTrueVoteFeatures), Required = Required.Default)]
        public bool NotificationNewTrueVoteFeatures { get; set; }
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
