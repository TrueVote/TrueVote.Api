using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Nostr.Client.Messages;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrueVote.Api.Models
{
    public class UserObj
    {
        [JsonPropertyName("User")]
        [JsonProperty("User", Required = Required.Default)]
        public List<UserModel>? user;
    }

    public class UserModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Users")]
        [JsonPropertyName("Users")]
        [JsonProperty(nameof(Users), Required = Required.Always)]
        public required List<UserModel> Users { get; set; }
    }

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
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; }

        [Required]
        [Description("DateUpdated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateUpdated")]
        [JsonProperty(nameof(DateUpdated), Required = Required.Default)]
        public required DateTime DateUpdated { get; set; }

        [Required]
        [Description("UserPreferences")]
        [DataType("UserPreferencesModel")]
        [JsonPropertyName("UserPreferences")]
        [JsonProperty(nameof(UserPreferences), Required = Required.Always)]
        public required UserPreferencesModel UserPreferences { get; set; } = new UserPreferencesModel();
    }

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

    [Owned]
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

    public class UserRoleModel
    {
        [Required]
        [Description("User Role Id")]
        [JsonPropertyName("UserRoleId")]
        [JsonProperty(nameof(UserRoleId), Required = Required.Always)]
        [Key]
        public required string UserRoleId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("User Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("UserId")]
        [ForeignKey("User")]
        [JsonProperty(nameof(UserId), Required = Required.Always)]
        public required string UserId { get; set; }

        [Required]
        [Description("Role Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RoleId")]
        [ForeignKey("Role")]
        [JsonProperty(nameof(RoleId), Required = Required.Always)]
        public required string RoleId { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }

    public class RoleModel
    {
        [Required]
        [Description("Role Id")]
        [JsonPropertyName("RoleId")]
        [JsonProperty(nameof(RoleId), Required = Required.Always)]
        [Key]
        public required string RoleId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Description("Role Name")]
        [MaxLength(256)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RoleName")]
        [JsonProperty(nameof(RoleName), Required = Required.Always)]
        public required string RoleName { get; set; }

        [Description("Role Description")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Description")]
        [JsonProperty(nameof(Description))]
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<UserRoleModel> UserRoles { get; set; } = new List<UserRoleModel>();
    }

    public readonly record struct RoleInfo
    {
        [Required]
        [Description("Role Name")]
        [DataType(DataType.Text)]
        [MaxLength(256)]
        [JsonPropertyName("name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public string Name { get; init; }

        [Required]
        [Description("Role ID")]
        [DataType(DataType.Text)]
        [MaxLength(256)]
        [JsonPropertyName("id")]
        [JsonProperty(nameof(Id), Required = Required.Always)]
        public string Id { get; init; }

        [Required]
        [Description("Role Description")]
        [DataType(DataType.Text)]
        [MaxLength(1024)]
        [JsonPropertyName("description")]
        [JsonProperty(nameof(Description), Required = Required.Always)]
        public string Description { get; init; }

        public RoleInfo(string Name, string Id, string Description)
        {
            (this.Name, this.Id, this.Description) = (Name, Id, Description);
        }
    }

    [Description("User Roles Static Definitions")]
    public static class UserRoles
    {
        // Role definitions
        [Description("Election Administrator Role")]
        public static readonly RoleInfo ElectionAdmin = new(
            Name: "ElectionAdmin",
            Id: "election-admin",
            Description: "Can manage elections"
        );

        [Description("Voter Role")]
        public static readonly RoleInfo Voter = new(
            Name: "Voter",
            Id: "voter",
            Description: "Can vote in elections"
        );

        [Description("System Administrator Role")]
        public static readonly RoleInfo SystemAdmin = new(
            Name: "SystemAdmin",
            Id: "system-admin",
            Description: "System administrator"
        );

        // Constants for attribute usage (attributes require const values)
        [Description("Election Administrator Role Constant")]
        public const string ElectionAdmin_Role = "ElectionAdmin";

        [Description("Voter Role Constant")]
        public const string Voter_Role = "Voter";

        [Description("System Administrator Role Constant")]
        public const string SystemAdmin_Role = "SystemAdmin";

        // Collection of all roles for iteration
        [Description("Collection of All Available Roles")]
        public static readonly IReadOnlyCollection<RoleInfo> AllRoles =
        [
            ElectionAdmin,
            Voter,
            SystemAdmin
        ];

        // Constants for attribute usage
        [Description("Election Administrator Name Constant")]
        public const string ElectionAdminName = "ElectionAdmin";

        [Description("Voter Name Constant")]
        public const string VoterName = "Voter";

        [Description("System Administrator Name Constant")]
        public const string SystemAdminName = "SystemAdmin";
    }
}
