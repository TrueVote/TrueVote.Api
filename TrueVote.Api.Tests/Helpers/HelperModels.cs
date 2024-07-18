using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;

namespace TrueVote.Api.Tests.Helpers
{
    public class CandidateTestModel
    {
        [Description("List of Candidates")]
        [DataType("List<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Default)]
        public List<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();

        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Default)]
        public string Name { get; set; } = string.Empty;

        [Description("Max Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MaxNumberOfChoicesValidator(nameof(Candidates), nameof(Name))]
        public int? MaxNumberOfChoices { get; set; }

        [Description("Min Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MinNumberOfChoicesValidator(nameof(Candidates), nameof(Name))]
        public int? MinNumberOfChoices { get; set; }
    }

    public class CandidateTestModelBlankProperty
    {
        [Description("List of Candidates")]
        [DataType("List<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Default)]
        public List<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();

        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Default)]
        public string Name { get; set; } = string.Empty;

        [Description("Max Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MaxNumberOfChoicesValidator("", "")]
        public int? MaxNumberOfChoices { get; set; }

        [Description("Min Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MinNumberOfChoicesValidator("", "")]
        public int? MinNumberOfChoices { get; set; }
    }

    public class CandidateTestModelMinInvalidProperty
    {
        [Description("List of Candidates")]
        [DataType("List<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Default)]
        public string Candidates { get; set; }

        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [Description("Min Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MinNumberOfChoicesValidator(nameof(Candidates), nameof(Name))]
        public int? MinNumberOfChoices { get; set; }
    }

    public class CandidateTestModelMaxInvalidProperty
    {
        [Description("List of Candidates")]
        [DataType("List<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Default)]
        public string Candidates { get; set; }

        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [Description("Max Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MaxNumberOfChoicesValidator(nameof(Candidates), nameof(Name))]
        public int? MaxNumberOfChoices { get; set; }
    }

    public class CandidateTestModelMaxNotFoundProperty
    {
        [Description("Max Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MaxNumberOfChoicesValidator("foo", "bar")]
        public int? MaxNumberOfChoices { get; set; }
    }

    public class CandidateTestModelMinNotFoundProperty
    {
        [Description("Min Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MaxNumberOfChoicesValidator("foo", "bar")]
        public int? MinNumberOfChoices { get; set; }
    }
}
