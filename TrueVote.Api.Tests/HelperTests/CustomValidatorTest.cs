using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Xunit;
using TrueVote.Api.Tests.Helpers;
using TrueVote.Api.Helpers;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using TrueVote.Api.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace TrueVote.Api.Tests.HelperTests
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

    public class CustomValidatorTest
    {
        [Fact]
        public void TestsMinAndMaxNumberOfChoicesSucceeds()
        {
            var testModel = new CandidateTestModel { MaxNumberOfChoices = 1, MinNumberOfChoices = 1, Candidates = MoqData.MockCandidateData };
            testModel.Candidates[0].Selected = true;
            Assert.Single(testModel.Candidates.Where(c => c.Selected == true));

            var validationResults = ValidationHelper.Validate(testModel);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void TestsMinNumberOfChoicesFails()
        {
            var testModel = new CandidateTestModel { MinNumberOfChoices = 3, Candidates = MoqData.MockCandidateData };
            testModel.Candidates[0].Selected = true;
            testModel.Candidates[1].Selected = true;
            Assert.Equal(2, testModel.Candidates.Where(c => c.Selected == true).Count());

            var validationResults = ValidationHelper.Validate(testModel, true);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("must be greater or equal to MinNumberOfChoices", validationResults[0].ErrorMessage);
            Assert.Equal("MinNumberOfChoices", validationResults[0].MemberNames.First());
        }

        [Fact]
        public void TestsMaxNumberOfChoicesFails()
        {
            var testModel = new CandidateTestModel { MaxNumberOfChoices = 1, Candidates = MoqData.MockCandidateData };
            testModel.Candidates[0].Selected = true;
            testModel.Candidates[1].Selected = true;
            Assert.Equal(2, testModel.Candidates.Where(c => c.Selected == true).Count());

            var validationResults = ValidationHelper.Validate(testModel, true);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("cannot exceed MaxNumberOfChoices", validationResults[0].ErrorMessage);
            Assert.Equal("MaxNumberOfChoices", validationResults[0].MemberNames.First());
        }

        [Fact]
        public void TestsMinNumberOfChoicesInvalidProperty()
        {
            var testModel = new CandidateTestModelMinInvalidProperty { MinNumberOfChoices = 3, Candidates = "foo" };

            var validationResults = ValidationHelper.Validate(testModel);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("Property 'Candidates' is not a valid List<CandidateModel> type", validationResults[0].ErrorMessage);
            Assert.Equal("MinNumberOfChoices", validationResults[0].MemberNames.First());
        }

        [Fact]
        public void TestsMaxNumberOfChoicesInvalidProperty()
        {
            var testModel = new CandidateTestModelMaxInvalidProperty { MaxNumberOfChoices = 3, Candidates = "foo" };

            var validationResults = ValidationHelper.Validate(testModel);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("Property 'Candidates' is not a valid List<CandidateModel> type", validationResults[0].ErrorMessage);
            Assert.Equal("MaxNumberOfChoices", validationResults[0].MemberNames.First());
        }

        [Fact]
        public void TestsNumberOfChoicesMaxNotFoundProperty()
        {
            var testModel = new CandidateTestModelMaxNotFoundProperty { MaxNumberOfChoices = 3 };

            var validationResults = ValidationHelper.Validate(testModel);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("Property not found", validationResults[0].ErrorMessage);
            Assert.Equal("MaxNumberOfChoices", validationResults[0].MemberNames.First());
        }

        [Fact]
        public void TestsNumberOfChoicesMinNotFoundProperty()
        {
            var testModel = new CandidateTestModelMinNotFoundProperty { MinNumberOfChoices = 3 };

            var validationResults = ValidationHelper.Validate(testModel);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("Property not found", validationResults[0].ErrorMessage);
            Assert.Equal("MinNumberOfChoices", validationResults[0].MemberNames.First());
        }

        [Fact]
        public void ValidatesModelWithNestedModelProperties()
        {
            var validationResults = new List<ValidationResult>();
            var baseBallotObj = new SubmitBallotModel { Election = MoqData.MockBallotData[1].Election };
            var validationContext = new ValidationContext(baseBallotObj);
            var validModel = RecursiveValidator.TryValidateObjectRecursive(baseBallotObj, validationContext, validationResults);
            Assert.True(validModel);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void ValidatesMinNumberOfChoicesInvalidProperty()
        {
            var validationResults = new List<ValidationResult>();
            var testModel = new CandidateTestModelMinInvalidProperty { MinNumberOfChoices = 3, Candidates = "foo" };
            var validationContext = new ValidationContext(testModel);
            var validModel = RecursiveValidator.TryValidateObjectRecursive(testModel, validationContext, validationResults);
            Assert.False(validModel);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("Property 'Candidates' is not a valid List<CandidateModel> type", validationResults[0].ErrorMessage);
            Assert.Equal("MinNumberOfChoices", validationResults[0].MemberNames.First());

            var errorDictionary = RecursiveValidator.GetValidationErrorsDictionary(validationResults);
            Assert.NotEmpty(errorDictionary);
            Assert.NotNull(errorDictionary);
            Assert.Single(errorDictionary);
        }

        [Fact]
        public void GetValidationErrorsDictionaryShouldHandleMissingErrorMessage()
        {
            var validationResults = new List<ValidationResult>
            {
                new("Error 1", ["Property1"]),
                new("Error 2", ["Property2"]),
                new(null, ["Property3"])
            };

            var errorDictionary = RecursiveValidator.GetValidationErrorsDictionary(validationResults);

            Assert.NotEmpty(errorDictionary);
            Assert.NotNull(errorDictionary);
            Assert.Equal(3, errorDictionary.Count);
            Assert.Equal(expected: ["Error 1"], errorDictionary["Property1"]);
            Assert.Equal(expected: ["Error 2"], errorDictionary["Property2"]);
            Assert.Equal(expected: [string.Empty], errorDictionary["Property3"]);
        }

        [Fact]
        public void ValidatorHandlesNullModel()
        {
            var validationResults = new List<ValidationResult>();
            var baseBallotObj = new SubmitBallotModel { Election = MoqData.MockBallotData[1].Election };
            var validationContext = new ValidationContext(baseBallotObj);
            var validModel = RecursiveValidator.TryValidateObjectRecursive(null, validationContext, validationResults);
            Assert.True(validModel);
            Assert.Empty(validationResults);

            var errorDictionary = RecursiveValidator.GetValidationErrorsDictionary(validationResults);
            Assert.Empty(errorDictionary);
            Assert.NotNull(errorDictionary);
        }
    }
}
