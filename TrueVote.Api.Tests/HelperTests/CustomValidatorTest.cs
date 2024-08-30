using System;
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
    public class CustomValidatorTest(ITestOutputHelper output) : TestHelper(output)
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
            Assert.Contains("Property 'Candidates' is not a valid List<CandidateModel>", validationResults[0].ErrorMessage);
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
            Assert.Contains("Property 'Candidates' is not a valid List<CandidateModel>", validationResults[0].ErrorMessage);
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
