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

namespace TrueVote.Api.Tests.HelperTests
{
    public class CandidateTestModel
    {
        [Description("List of Candidates")]
        [DataType("List<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Default)]
        public List<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();

        [Description("Max Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MaxNumberOfChoicesValidator(nameof(Candidates))]
        public int? MaxNumberOfChoices { get; set; }

        [Description("Min Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MinNumberOfChoicesValidator(nameof(Candidates))]
        public int? MinNumberOfChoices { get; set; }
    }

    public class CandidateTestModelBlankProperty
    {
        [Description("List of Candidates")]
        [DataType("List<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Default)]
        public List<CandidateModel> Candidates { get; set; } = new List<CandidateModel>();

        [Description("Max Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MaxNumberOfChoicesValidator("")]
        public int? MaxNumberOfChoices { get; set; }

        [Description("Min Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MinNumberOfChoicesValidator("")]
        public int? MinNumberOfChoices { get; set; }
    }

    public class CandidateTestModelMinInvalidProperty
    {
        [Description("List of Candidates")]
        [DataType("List<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Default)]
        public string Candidates { get; set; }

        [Description("Min Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MinNumberOfChoicesValidator(nameof(Candidates))]
        public int? MinNumberOfChoices { get; set; }
    }

    public class CandidateTestModelMaxInvalidProperty
    {
        [Description("List of Candidates")]
        [DataType("List<CandidateModel>")]
        [JsonPropertyName("Candidates")]
        [JsonProperty(nameof(Candidates), Required = Required.Default)]
        public string Candidates { get; set; }

        [Description("Max Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MaxNumberOfChoicesValidator(nameof(Candidates))]
        public int? MaxNumberOfChoices { get; set; }
    }

    public class CandidateTestModelMaxNotFoundProperty
    {
        [Description("Max Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MaxNumberOfChoicesValidator("foo")]
        public int? MaxNumberOfChoices { get; set; }
    }

    public class CandidateTestModelMinNotFoundProperty
    {
        [Description("Min Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [MaxNumberOfChoicesValidator("foo")]
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

            var validationResults = ValidationHelper.Validate(testModel);
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

            var validationResults = ValidationHelper.Validate(testModel);
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
    }
}
