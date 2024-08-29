using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.HelperTests
{
    public class RecursiveValidatorTest : TestHelper
    {
        private readonly RecursiveValidator recursiveValidator;

        public RecursiveValidatorTest(ITestOutputHelper output) : base(output)
        {
            recursiveValidator = new RecursiveValidator();
        }

        [Fact]
        public void ValidatesModelWithNestedModelProperties()
        {
            var validationResults = new List<ValidationResult>();
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };
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
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };
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
            Assert.Contains("Property 'Candidates' is not a valid List<CandidateModel>", validationResults[0].ErrorMessage);
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
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };
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
            Assert.Contains("Property 'Candidates' is not a valid List<CandidateModel>", result.ErrorMessage);
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
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };
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
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[1].Election };
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
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[4].Election };

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
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[3].Election };

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
            var baseBallotObj = new SubmitBallotModel { AccessCode = MoqData.MockUsedAccessCodeData[0].AccessCode, Election = MoqData.MockBallotData[0].Election };
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
        public void IsValidWhenRacePropertyValueIsNull_ShouldSetEmptyString()
        {
            // Arrange
            var attribute = new TestNumberOfChoicesValidatorAttribute("CandidatesProperty", "RaceProperty");
            var mockObject = new MockObjectWithNullProperty();
            var validationContext = new ValidationContext(mockObject);

            // Act
            attribute.TestIsValid(null, validationContext);

            // Assert
            Assert.Equal(string.Empty, attribute.GetRacePropertyValue());
        }

        // Test-specific subclass of NumberOfChoicesValidatorAttribute
        private class TestNumberOfChoicesValidatorAttribute : NumberOfChoicesValidatorAttribute
        {
            public TestNumberOfChoicesValidatorAttribute(string propertyName, string racePropertyName)
                : base(propertyName, racePropertyName) { }

            public ValidationResult TestIsValid(object value, ValidationContext validationContext)
            {
                return base.IsValid(value, validationContext);
            }

            protected override ValidationResult ValidateCount(object choicesValue, int count, ValidationContext validationContext)
            {
                // Implementation not needed for this test
                return ValidationResult.Success;
            }
        }

        // Mock object that returns null for the RaceProperty
        private class MockObjectWithNullProperty
        {
            public object RaceProperty => null;
            public List<CandidateModel> CandidatesProperty => new List<CandidateModel>();
        }
    }
}
