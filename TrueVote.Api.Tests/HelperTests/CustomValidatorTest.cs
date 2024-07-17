using Moq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
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

    public class CustomValidatorTest : TestHelper
    {
        private readonly RecursiveValidator recursiveValidator;

        public CustomValidatorTest(ITestOutputHelper output) : base(output)
        {
            recursiveValidator = new RecursiveValidator();
        }

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
            validationContext.Items["IsBallot"] = true;
            validationContext.Items["DBContext"] = _trueVoteDbContext;
            validationContext.Items["Logger"] = _logHelper.Object;

            var validModel = recursiveValidator.TryValidateObjectRecursive(baseBallotObj, validationContext, validationResults);
            Assert.True(validModel);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void HandlesModelWithNullNestedModelProperties()
        {
            var validationResults = new List<ValidationResult>();
            var baseBallotObj = new SubmitBallotModel { Election = MoqData.MockBallotData[1].Election };
            baseBallotObj.Election.Races = null;
            var validationContext = new ValidationContext(baseBallotObj);
            validationContext.Items["IsBallot"] = true;
            validationContext.Items["DBContext"] = _trueVoteDbContext;
            validationContext.Items["Logger"] = _logHelper.Object;

            var validModel = recursiveValidator.TryValidateObjectRecursive(baseBallotObj, validationContext, validationResults);
            Assert.False(validModel);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Contains("Races field is required", validationResults[1].ErrorMessage);
            Assert.Contains("Races", validationResults[1].MemberNames);
            _logHelper.Verify(LogLevel.Information, Times.Exactly(1));
        }

        [Fact]
        public void ValidatesMinNumberOfChoicesInvalidProperty()
        {
            var validationResults = new List<ValidationResult>();
            var testModel = new CandidateTestModelMinInvalidProperty { MinNumberOfChoices = 3, Candidates = "foo" };
            var validationContext = new ValidationContext(testModel);
            var validModel = recursiveValidator.TryValidateObjectRecursive(testModel, validationContext, validationResults);
            Assert.False(validModel);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("Property 'Candidates' is not a valid List<CandidateModel> type", validationResults[0].ErrorMessage);
            Assert.Equal("MinNumberOfChoices", validationResults[0].MemberNames.First());

            var errorDictionary = recursiveValidator.GetValidationErrorsDictionary(validationResults);
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

            var errorDictionary = recursiveValidator.GetValidationErrorsDictionary(validationResults);

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
            var validModel = recursiveValidator.TryValidateObjectRecursive(null, validationContext, validationResults);
            Assert.True(validModel);
            Assert.Empty(validationResults);

            var errorDictionary = recursiveValidator.GetValidationErrorsDictionary(validationResults);
            Assert.Empty(errorDictionary);
            Assert.NotNull(errorDictionary);
        }

        [Fact]
        public void GetValidationErrorsDictionaryHandlesMultipleErrorsForSameProperty()
        {
            var validationResults = new List<ValidationResult>
            {
                new("Error 1", ["Property1"]),
                new("Error 2", ["Property1"]),
                new("Error 3", ["Property2"])
            };

            var errorDictionary = recursiveValidator.GetValidationErrorsDictionary(validationResults);

            Assert.Equal(2, errorDictionary.Count);
            Assert.Equal(new[] { "Error 1", "Error 2" }, errorDictionary["Property1"]);
            Assert.Equal(new[] { "Error 3" }, errorDictionary["Property2"]);
        }

        [Fact]
        public void TryValidateObjectRecursiveHandlesNestedObject()
        {
            var nestedModel = new CandidateTestModel { MaxNumberOfChoices = 1, MinNumberOfChoices = 1, Candidates = new List<CandidateModel>() };
            var testModel = new { NestedProperty = nestedModel };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(testModel);
            var validModel = recursiveValidator.TryValidateObjectRecursive(testModel, validationContext, validationResults);

            Assert.True(validModel);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void TryValidateObjectRecursiveHandlesCollectionOfObjects()
        {
            var testModel = new { Items = new List<CandidateTestModel> { new(), new() } };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(testModel);
            var validModel = recursiveValidator.TryValidateObjectRecursive(testModel, validationContext, validationResults);

            Assert.True(validModel);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void NumberOfChoicesValidatorHandlesInvalidPropertyType()
        {
            var testModel = new { Candidates = "Not a List<CandidateModel>", MinNumberOfChoices = 1 };
            var validationContext = new ValidationContext(testModel);
            validationContext.Items["IsBallot"] = true;

            var attribute = new MinNumberOfChoicesValidatorAttribute("Candidates", "Name");
            var result = attribute.GetValidationResult(testModel.MinNumberOfChoices, validationContext);

            Assert.NotNull(result);
            Assert.Contains("Property 'Candidates' is not a valid List<CandidateModel> type", result.ErrorMessage);
        }

        [Fact]
        public void NumberOfChoicesValidatorSkipsValidationWhenIsBallotNotSet()
        {
            var testModel = new CandidateTestModel { MaxNumberOfChoices = 1, MinNumberOfChoices = 1, Candidates = new List<CandidateModel>() };
            var validationContext = new ValidationContext(testModel);
            // Note: We're not setting IsBallot in the Items dictionary

            var attribute = new MaxNumberOfChoicesValidatorAttribute("Candidates", "Name");
            var result = attribute.GetValidationResult(testModel.MaxNumberOfChoices, validationContext);

            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void BallotIntegrityCheckerHandlesInvalidElectionPropertyType()
        {
            var testModel = new { Election = "Not an ElectionModel" };
            var validationContext = new ValidationContext(testModel);

            var attribute = new BallotIntegrityCheckerAttribute("Election");
            var result = attribute.GetValidationResult(testModel, validationContext);

            Assert.NotNull(result);
            Assert.Contains("Property 'Election' is not a valid ElectionModel type", result.ErrorMessage);
        }

        [Fact]
        public void BallotIntegrityCheckerHandlesMissingElectionPropertyType()
        {
            var testModel = new { Election = "Not an ElectionModel" };
            var validationContext = new ValidationContext(testModel);

            var attribute = new BallotIntegrityCheckerAttribute("Not an election attribute");
            var result = attribute.GetValidationResult(testModel, validationContext);

            Assert.NotNull(result);
            Assert.Contains("property not found", result.ErrorMessage);
        }

        [Fact]
        public void BallotIntegrityCheckerHandlesUnfoundElection()
        {
            var baseBallotObj = new SubmitBallotModel { Election = MoqData.MockBallotData[1].Election };
            baseBallotObj.Election.ElectionId = "willnotfind";
            var validationContext = new ValidationContext(baseBallotObj);
            validationContext.Items["IsBallot"] = true;
            validationContext.Items["DBContext"] = _trueVoteDbContext;
            validationContext.Items["Logger"] = _logHelper.Object;

            var attribute = new BallotIntegrityCheckerAttribute("Election");
            var result = attribute.GetValidationResult(baseBallotObj, validationContext);

            Assert.NotNull(result);
            Assert.Contains("Ballot for Election", result.ErrorMessage);
        }

        [Fact]
        public void BallotIntegrityCheckerHandlesMissingDBContext()
        {
            var baseBallotObj = new SubmitBallotModel { Election = MoqData.MockBallotData[1].Election };
            var validationContext = new ValidationContext(baseBallotObj);
            validationContext.Items["IsBallot"] = true;
            // validationContext.Items["DBContext"] = _trueVoteDbContext;
            validationContext.Items["Logger"] = _logHelper.Object;

            var attribute = new BallotIntegrityCheckerAttribute("Election");
            var result = attribute.GetValidationResult(baseBallotObj, validationContext);

            Assert.NotNull(result);
            Assert.Contains("Could not get DBContext", result.ErrorMessage);
        }

        [Fact]
        public void BallotIntegrityCheckerHandlesSubmissionBeforeElectionStart()
        {
            var baseBallotObj = new SubmitBallotModel { Election = MoqData.MockBallotData[4].Election };

            var validationContext = new ValidationContext(baseBallotObj);
            validationContext.Items["IsBallot"] = true;
            validationContext.Items["DBContext"] = _trueVoteDbContext;
            validationContext.Items["Logger"] = _logHelper.Object;

            var attribute = new BallotIntegrityCheckerAttribute("Election");
            var result = attribute.GetValidationResult(baseBallotObj, validationContext);

            Assert.NotNull(result);
            Assert.Contains("which is before the election start:", result.ErrorMessage);
        }

        [Fact]
        public void BallotIntegrityCheckerHandlesSubmissionAfterElectionEnd()
        {
            var baseBallotObj = new SubmitBallotModel { Election = MoqData.MockBallotData[3].Election };

            var validationContext = new ValidationContext(baseBallotObj);
            validationContext.Items["IsBallot"] = true;
            validationContext.Items["DBContext"] = _trueVoteDbContext;
            validationContext.Items["Logger"] = _logHelper.Object;

            var attribute = new BallotIntegrityCheckerAttribute("Election");
            var result = attribute.GetValidationResult(baseBallotObj, validationContext);

            Assert.NotNull(result);
            Assert.Contains("which is after the election end:", result.ErrorMessage);
        }

        [Fact]
        public void BallotIntegrityCheckerHandlesSelectionDifferences()
        {
            var baseBallotObj = new SubmitBallotModel { Election = MoqData.MockBallotData[0].Election };
            baseBallotObj.Election.Races[0].Candidates[0].Selected = true;
            baseBallotObj.Election.Races[0].Candidates[1].Selected = true;
            var validationContext = new ValidationContext(baseBallotObj);
            validationContext.Items["IsBallot"] = true;
            validationContext.Items["DBContext"] = _trueVoteDbContext;
            validationContext.Items["Logger"] = _logHelper.Object;

            var attribute = new BallotIntegrityCheckerAttribute("Election");
            var result = attribute.GetValidationResult(baseBallotObj, validationContext);

            Assert.Equal(ValidationResult.Success, result);
        }
    }
}
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
