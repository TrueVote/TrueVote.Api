using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Xunit;
using TrueVote.Api.Tests.Helpers;
using TrueVote.Api.Helpers;
using Newtonsoft.Json;
using System.Linq;

namespace TrueVote.Api.Tests.HelperTests
{
    public class NameModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        public required string Name { get; set; } = string.Empty;
    }

    public class TestModel1
    {
        [Description("Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [NumberOfChoicesValidator(nameof(NameModel))]
        public int? NumberOfChoices { get; set; }

        [Description("List of Names")]
        [DataType("List<NameModel>")]
        public List<NameModel> NameModel { get; set; } = new List<NameModel>();
    }

    public class TestModel2
    {
        [Description("Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [NumberOfChoicesValidator("")]
        public int? NumberOfChoices { get; set; }

        [Description("List of Names")]
        [DataType("List<NameModel>")]
        public List<NameModel> NameModel { get; set; } = new List<NameModel>();
    }

    public class TestModel3
    {
        [Description("Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [NumberOfChoicesValidator(nameof(Name))]
        public int? NumberOfChoices { get; set; }

        [Description("Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; } = string.Empty;
    }

    public class TestModel5
    {
        [Description("Number of Choices")]
        [DataType("integer")]
        [Range(0, int.MaxValue)]
        [NumberOfChoicesValidator(nameof(NameModel))]
        public int? NumberOfChoices { get; set; }

        [Description("List of Names")]
        [DataType("List<NameModel>")]
        public List<NameModel> NameModel { get; set; } = new List<NameModel>();
    }

    public class CustomValidatorTest
    {
        [Fact]
        public void TestsNumberOfChoicesSucceeds()
        {
            var testModel = new TestModel1 { NumberOfChoices = 1 };
            testModel.NameModel.Add(new NameModel { Name = "name1 "});
            testModel.NameModel.Add(new NameModel { Name = "name2 " });
            testModel.NameModel.Add(new NameModel { Name = "name3 " });

            var validationResults = ValidationHelper.Validate(testModel);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void TestsNumberOfChoicesFailsWhenTooHigh()
        {
            var testModel = new TestModel1 { NumberOfChoices = 5 };
            testModel.NameModel.Add(new NameModel { Name = "name1 " });
            testModel.NameModel.Add(new NameModel { Name = "name2 " });
            testModel.NameModel.Add(new NameModel { Name = "name3 " });

            var validationResults = ValidationHelper.Validate(testModel);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("NumberOfChoices cannot exceed the", validationResults[0].ErrorMessage);
            Assert.Equal("NumberOfChoices", validationResults[0].MemberNames.First());
        }

        [Fact]
        public void TestsNumberOfChoicesFailsWhenPropertyNotFound()
        {
            var testModel = new TestModel2 { NumberOfChoices = 1 };
            testModel.NameModel.Add(new NameModel { Name = "name1 " });
            testModel.NameModel.Add(new NameModel { Name = "name2 " });
            testModel.NameModel.Add(new NameModel { Name = "name3 " });

            var validationResults = ValidationHelper.Validate(testModel);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("Property not found", validationResults[0].ErrorMessage);
            Assert.Equal("NumberOfChoices", validationResults[0].MemberNames.First());
        }

        [Fact]
        public void TestsNumberOfChoicesFailsWhenPropertyToCheckIsInvalid()
        {
            var testModel = new TestModel3 { NumberOfChoices = 1 };

            var validationResults = ValidationHelper.Validate(testModel);
            Assert.NotEmpty(validationResults);
            Assert.NotNull(validationResults);
            Assert.Single(validationResults);
            Assert.Contains("is not a valid collection", validationResults[0].ErrorMessage);
            Assert.Equal("NumberOfChoices", validationResults[0].MemberNames.First());
        }

        [Fact]
        public void TestsNumberOfChoicesSucceedsWhenNumberOfChoicesIsUnset()
        {
            var testModel = new TestModel5();

            var validationResults = ValidationHelper.Validate(testModel);
            Assert.Empty(validationResults);
        }
    }
}
