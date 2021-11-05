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
        [OpenApiPropertyDescription("First Name")]
        [JsonProperty(Required = Required.Always)]
        [StringLength(10)]
        [MaxLength(10)]
        [DataType(DataType.Text)]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiPropertyDescription("Email Address")]
        [JsonProperty(Required = Required.Always)]
        [StringLength(10)]
        [MaxLength(10)]
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {

        }

    }
}
