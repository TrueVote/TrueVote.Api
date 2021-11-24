using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace TrueVote.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class User
    {
        [OpenApiProperty(Description = "First Name")]
        [JsonProperty(Required = Required.Always)]
        [StringLength(10)]
        [MaxLength(10)]
        [DataType(DataType.Text)]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiProperty(Description = "Email Address")]
        [JsonProperty(Required = Required.Always)]
        [StringLength(10)]
        [MaxLength(10)]
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [OpenApiProperty(Description = "GUID Id")]
        [StringLength(40)]
        [MaxLength(40)]
        [DataType(DataType.Text)]
        public string UserId { get; set; } = System.Guid.NewGuid().ToString();


        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {

        }
    }
}
