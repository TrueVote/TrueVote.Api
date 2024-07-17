using Moq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
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

        [Fact]
        public void ModelDiffHandlesDifferentPropertyTypes()
        {
            var modelA = new
            {
                StringProp = "A",
                IntProp = 1,
                DateTimeProp = new DateTime(2023, 1, 1, 12, 0, 0),
                NullableBoolProp = (bool?) true,
                ListProp = new List<string> { "A", "B" }
            };

            var modelB = new
            {
                StringProp = "B",
                IntProp = 2,
                DateTimeProp = new DateTime(2023, 1, 2, 12, 0, 0),
                NullableBoolProp = (bool?) false,
                ListProp = new List<string> { "B", "C" }
            };

            var diff = modelA.ModelDiff(modelB);

            // Print out all the differences
            foreach (var kvp in diff)
            {
                Console.WriteLine($"Difference in {kvp.Key}: {kvp.Value.OldValue} -> {kvp.Value.NewValue}");
            }

            Assert.Equal(5, diff.Count);

            // Individual assertions
            Assert.True(diff.ContainsKey("StringProp"));
            Assert.True(diff.ContainsKey("IntProp"));
            Assert.True(diff.ContainsKey("DateTimeProp"));
            Assert.True(diff.ContainsKey("NullableBoolProp"));
            Assert.True(diff.ContainsKey("ListProp"));

            Assert.Equal(("A", "B"), diff["StringProp"]);
            Assert.Equal((1, 2), diff["IntProp"]);
            Assert.Equal((new DateTime(2023, 1, 1, 12, 0, 0), new DateTime(2023, 1, 2, 12, 0, 0)), diff["DateTimeProp"]);
            Assert.Equal((true, false), diff["NullableBoolProp"]);
            Assert.Equal(("A,B", "B,C"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiffHandlesNullLists()
        {
            var modelA = new
            {
                ListProp = new List<string> { "B", "C" }
            };

            var modelB = new
            {
                ListProp = (List<string>) null
            };

            var modelC = new
            {
                ListProp = (List<string>) null
            };

            var diff1 = modelA.ModelDiff(modelB);
            Assert.Single(diff1);
            Assert.True(diff1.ContainsKey("ListProp"));

            var diff2 = modelB.ModelDiff(modelC);
            Assert.Empty(diff2);

            var diff3 = modelB.ModelDiff(modelA);
            Assert.Single(diff3);
            Assert.True(diff3.ContainsKey("ListProp"));
        }

        [Fact]
        public void ModelDiffHandlesNullModels()
        {
            var modelA = new SubmitBallotModel { Election = null };

            var modelB = (SubmitBallotModel) null;

            var modelC = (SubmitBallotModel) null;

            var diff1 = modelA.ModelDiff(modelB);
            Assert.Single(diff1);

            // Call using static invocation since we can't call an extension on a null object
            // var diff2 = modelB.ModelDiff(modelC);
            var diff2 = ModelDiffExtensions.ModelDiff(modelB, modelC);
            Assert.Empty(diff2);

            //var diff3 = modelB.ModelDiff(modelA);
            var diff3 = ModelDiffExtensions.ModelDiff(modelB, modelA);
            Assert.Single(diff3);
        }

        [Fact]
        public void ModelDiffHandlesNullDateTimes()
        {
            var modelA = new
            {
                DateTimeProp = (DateTime?) new DateTime(2023, 1, 1, 12, 0, 0),
            };

            var modelB = new
            {
                DateTimeProp = (DateTime?) null,
            };

            var modelC = new
            {
                DateTimeProp = (DateTime?) null,
            };

            var diff1 = modelA.ModelDiff(modelB);
            Assert.Single(diff1);
            Assert.True(diff1.ContainsKey("DateTimeProp"));

            var diff2 = modelB.ModelDiff(modelC);
            Assert.Empty(diff2);

            var diff3 = modelB.ModelDiff(modelA);
            Assert.Single(diff3);
            Assert.True(diff3.ContainsKey("DateTimeProp"));
        }

        [Fact]
        public void CompareEnumerables_BothNonNull_ReturnsCorrectDifference()
        {
            // Arrange
            var a = new List<int> { 1, 2, 3 };
            var b = new List<int> { 1, 2, 4 };

            // Act
            var result = ModelDiffExtensions.CompareEnumerables(a, b, "TestPrefix");

            // Assert
            Assert.Single(result);
            Assert.Equal(("1,2,3", "1,2,4"), result["TestPrefix"]);
        }

        [Fact]
        public void CompareEnumerables_FirstNull_ReturnsCorrectDifference()
        {
            // Arrange
            IEnumerable a = null;
            var b = new List<int> { 1, 2, 3 };

            // Act
            var result = ModelDiffExtensions.CompareEnumerables(a, b, "TestPrefix");

            // Assert
            Assert.Single(result);
            Assert.Equal(("", "1,2,3"), result["TestPrefix"]);
        }

        [Fact]
        public void CompareEnumerables_SecondNull_ReturnsCorrectDifference()
        {
            // Arrange
            var a = new List<int> { 1, 2, 3 };
            IEnumerable b = null;

            // Act
            var result = ModelDiffExtensions.CompareEnumerables(a, b, "TestPrefix");

            // Assert
            Assert.Single(result);
            Assert.Equal(("1,2,3", ""), result["TestPrefix"]);
        }

        [Fact]
        public void CompareEnumerables_BothNull_ReturnsEmptyDictionary()
        {
            // Arrange
            IEnumerable a = null;
            IEnumerable b = null;

            // Act
            var result = ModelDiffExtensions.CompareEnumerables(a, b, "TestPrefix");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void CompareEnumerables_DifferentTypes_HandlesCorrectly()
        {
            // Arrange
            var a = new List<int> { 1, 2, 3 };
            var b = new string[] { "1", "2", "3" };

            // Act
            var result = ModelDiffExtensions.CompareEnumerables(a, b, "TestPrefix");

            // Assert
            Assert.Single(result);
            Assert.Equal(("1,2,3", "1,2,3"), result["TestPrefix"]);
        }

        [Fact]
        public void AreDateTimesEqual_BothNull_ReturnsTrue()
        {
            // Arrange
            DateTime? a = null;
            DateTime? b = null;

            // Act
            var result = ModelDiffExtensions.AreDateTimesEqual(a, b);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AreDateTimesEqual_FirstNull_ReturnsFalse()
        {
            // Arrange
            DateTime? a = null;
            DateTime? b = new DateTime(2023, 7, 16);

            // Act
            var result = ModelDiffExtensions.AreDateTimesEqual(a, b);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AreDateTimesEqual_SecondNull_ReturnsFalse()
        {
            // Arrange
            DateTime? a = new DateTime(2023, 7, 16);
            DateTime? b = null;

            // Act
            var result = ModelDiffExtensions.AreDateTimesEqual(a, b);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AreDateTimesEqual_BothNonNull_Equal_ReturnsTrue()
        {
            // Arrange
            DateTime? a = new DateTime(2023, 7, 16, 10, 30, 0);
            DateTime? b = new DateTime(2023, 7, 16, 10, 30, 0);

            // Act
            var result = ModelDiffExtensions.AreDateTimesEqual(a, b);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AreDateTimesEqual_BothNonNull_NotEqual_ReturnsFalse()
        {
            // Arrange
            DateTime? a = new DateTime(2023, 7, 16, 10, 30, 0);
            DateTime? b = new DateTime(2023, 7, 16, 10, 30, 1);

            // Act
            var result = ModelDiffExtensions.AreDateTimesEqual(a, b);

            // Assert
            Assert.False(result);
        }

        private class TestClass
        {
            public int TestProperty { get; set; }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("SomePrefix.")]
        public void CompareComplexTypes_PrefixHandling_WorksCorrectly(string prefix)
        {
            // Arrange
            var a = new TestClass { TestProperty = 1 };
            var b = new TestClass { TestProperty = 2 };

            // Act
            var result = ModelDiffExtensions.CompareComplexTypes(a, b, prefix);

            // Assert
            var expectedKey = string.IsNullOrEmpty(prefix) ? "TestProperty" : $"{prefix}TestProperty";
            Assert.True(result.ContainsKey(expectedKey));
            Assert.Equal((1, 2), result[expectedKey]);
        }

        private class TestClassWithExceptionProperty
        {
            public int NormalProperty { get; set; }

            public int ExceptionProperty => throw new InvalidOperationException("This property always throws an exception");
        }

        [Fact]
        public void CompareComplexTypes_PropertyThrowsException_ContinuesComparison()
        {
            // Arrange
            var a = new TestClassWithExceptionProperty { NormalProperty = 1 };
            var b = new TestClassWithExceptionProperty { NormalProperty = 2 };

            // Act
            var result = ModelDiffExtensions.CompareComplexTypes(a, b, "");

            // Assert
            Assert.Single(result);  // Only NormalProperty should be in the result
            Assert.True(result.ContainsKey("NormalProperty"));
            Assert.Equal((1, 2), result["NormalProperty"]);
            Assert.False(result.ContainsKey("ExceptionProperty"));  // ExceptionProperty should be skipped
        }

        [Fact]
        public void ModelDiff_HandlesNullItemsInLists()
        {
            var modelA = new
            {
                ListProp = new List<string?> { null, "B" }
            };

            var modelB = new
            {
                ListProp = new List<string?> { null, "B" }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be no difference
            Assert.Empty(diff);
        }

        [Fact]
        public void ModelDiff_HandlesOneNullItemInList()
        {
            var modelA = new
            {
                ListProp = new List<string?> { "A", null }
            };

            var modelB = new
            {
                ListProp = new List<string?> { "A", "B" }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be one difference
            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("A,", "A,B"), diff["ListProp"]);
        }

        public class ComplexType
        {
            public string Prop { get; set; } = string.Empty;
        }

        [Fact]
        public void ModelDiff_HandlesComplexTypesInLists()
        {
            var modelA = new
            {
                ListProp = new List<ComplexType>
                {
                    new ComplexType { Prop = "A" }
                }
            };

            var modelB = new
            {
                ListProp = new List<ComplexType>
                {
                    new ComplexType { Prop = "B" }
                }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be one difference
            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp[0].Prop"));
            Assert.Equal(("A", "B"), diff["ListProp[0].Prop"]);
        }

        [Fact]
        public void ModelDiff_HandlesDifferentLengthLists()
        {
            var modelA = new
            {
                ListProp = new List<string> { "A", "B" }
            };

            var modelB = new
            {
                ListProp = new List<string> { "A" }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be one difference
            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("A,B", "A"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiff_HandlesOneNullItemInList_CoverAllCases()
        {
            var modelA = new
            {
                ListProp = new List<string?> { "A", null }
            };

            var modelB = new
            {
                ListProp = new List<string?> { "A", "B" }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be one difference
            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("A,", "A,B"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiff_HandlesComplexTypesInLists_CoverAllCases()
        {
            var modelA = new
            {
                ListProp = new List<ComplexType>
                {
                    new ComplexType { Prop = "A" }
                }
            };

            var modelB = new
            {
                ListProp = new List<ComplexType>
                {
                    new ComplexType { Prop = "B" }
                }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be one difference
            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp[0].Prop"));
            Assert.Equal(("A", "B"), diff["ListProp[0].Prop"]);
        }

        [Fact]
        public void ModelDiff_HandlesDifferentLengthListsWithNonNullItems()
        {
            var modelA = new
            {
                ListProp = new List<string> { "A", "B" }
            };

            var modelB = new
            {
                ListProp = new List<string> { "A" }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be one difference
            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("A,B", "A"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiff_HandlesMixedNullAndNonNullItemsInLists()
        {
            var modelA = new
            {
                ListProp = new List<string?> { "A", null }
            };

            var modelB = new
            {
                ListProp = new List<string?> { "A", "B" }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be one difference
            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("A,", "A,B"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiff_HandlesAllNullItemsInLists()
        {
            var modelA = new
            {
                ListProp = new List<string?> { null, null }
            };

            var modelB = new
            {
                ListProp = new List<string?> { null, null }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be no difference
            Assert.Empty(diff);
        }

        [Fact]
        public void ModelDiff_HandlesDifferentComplexObjectsInLists()
        {
            var modelA = new
            {
                ListProp = new List<ComplexType>
                {
                    new ComplexType { Prop = "A" },
                    new ComplexType { Prop = "B" }
                }
            };

            var modelB = new
            {
                ListProp = new List<ComplexType>
                {
                    new ComplexType { Prop = "A" }
                }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be one difference
            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("ComplexType,ComplexType", "ComplexType"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiff_HandlesIdenticalComplexObjectsWithDifferentCounts()
        {
            var modelA = new
            {
                ListProp = new List<ComplexType>
                {
                    new ComplexType { Prop = "A" }
                }
            };

            var modelB = new
            {
                ListProp = new List<ComplexType>
                {
                    new ComplexType { Prop = "A" },
                    new ComplexType { Prop = "B" }
                }
            };

            var diff = modelA.ModelDiff(modelB);

            // There should be one difference
            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("ComplexType", "ComplexType,ComplexType"), diff["ListProp"]);
        }
    }
}
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
