using System;
using System.Collections.Generic;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable CS8632
namespace TrueVote.Api.Tests.HelperTests
{
    public class ModelDiffValidatorTest : TestHelper
    {
        private readonly RecursiveValidator recursiveValidator;

        public ModelDiffValidatorTest(ITestOutputHelper output) : base(output)
        {
            recursiveValidator = new RecursiveValidator();
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

            Assert.Equal(5, diff.Count);
            Assert.Equal(("A", "B"), diff["StringProp"]);
            Assert.Equal((1, 2), diff["IntProp"]);
            Assert.Equal((new DateTime(2023, 1, 1, 12, 0, 0), new DateTime(2023, 1, 2, 12, 0, 0)), diff["DateTimeProp"]);
            Assert.Equal((true, false), diff["NullableBoolProp"]);
            Assert.Equal(("A,B", "B,C"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiffHandlesNullAndEmptyLists()
        {
            var modelA = new { ListProp = new List<string> { "B", "C" } };
            var modelB = new { ListProp = (List<string>) null };
            var modelC = new { ListProp = new List<string>() };

            var diff1 = modelA.ModelDiff(modelB);
            Assert.Single(diff1);
            Assert.True(diff1.ContainsKey("ListProp"));

            var diff2 = modelB.ModelDiff(modelC);
            Assert.Single(diff2);
            Assert.True(diff2.ContainsKey("ListProp"));

            var diff3 = modelA.ModelDiff(modelC);
            Assert.Single(diff3);
            Assert.True(diff3.ContainsKey("ListProp"));
            Assert.Equal(("B,C", ""), diff3["ListProp"]);
        }

        [Fact]
        public void ModelDiffHandlesNullModels()
        {
            var modelA = new SubmitBallotModel { Election = null };
            var modelB = (SubmitBallotModel) null;

            var diff1 = modelA.ModelDiff(modelB);
            Assert.Single(diff1);

            var diff2 = ModelDiffExtensions.ModelDiff(modelB, modelB);
            Assert.Empty(diff2);

            var diff3 = ModelDiffExtensions.ModelDiff(modelB, modelA);
            Assert.Single(diff3);
        }

        [Fact]
        public void ModelDiffHandlesNullDateTimes()
        {
            var modelA = new { DateTimeProp = (DateTime?) new DateTime(2023, 1, 1, 12, 0, 0) };
            var modelB = new { DateTimeProp = (DateTime?) null };

            var diff1 = modelA.ModelDiff(modelB);
            Assert.Single(diff1);
            Assert.True(diff1.ContainsKey("DateTimeProp"));

            var diff2 = modelB.ModelDiff(modelB);
            Assert.Empty(diff2);
        }

        [Theory]
        [InlineData(null, new[] { 1, 2, 3 }, "", "1,2,3")]
        [InlineData(new[] { 1, 2, 3 }, null, "1,2,3", "")]
        [InlineData(null, null, "", "")]
        [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2, 4 }, "1,2,3", "1,2,4")]
        public void CompareEnumerablesHandlesVariousScenarios(int[] a, int[] b, string expectedOld, string expectedNew)
        {
            var result = ModelDiffExtensions.CompareEnumerables(a, b, "TestPrefix");

            if (string.IsNullOrEmpty(expectedOld) && string.IsNullOrEmpty(expectedNew))
            {
                Assert.Empty(result);
            }
            else
            {
                Assert.Single(result);
                Assert.Equal((expectedOld, expectedNew), result["TestPrefix"]);
            }
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData(null, "2023-07-16", false)]
        [InlineData("2023-07-16", null, false)]
        [InlineData("2023-07-16 10:30:00", "2023-07-16 10:30:00", true)]
        [InlineData("2023-07-16 10:30:00", "2023-07-16 10:30:01", false)]
        public void AreDateTimesEqualHandlesVariousScenarios(string a, string b, bool expected)
        {
            DateTime? dateA = a == null ? null : DateTime.Parse(a);
            DateTime? dateB = b == null ? null : DateTime.Parse(b);

            var result = ModelDiffExtensions.AreDateTimesEqual(dateA, dateB);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("SomePrefix.")]
        public void CompareComplexTypesPrefixHandlingWorksCorrectly(string prefix)
        {
            var a = new TestClass { TestProperty = 1 };
            var b = new TestClass { TestProperty = 2 };

            var result = ModelDiffExtensions.CompareComplexTypes(a, b, prefix);

            var expectedKey = string.IsNullOrEmpty(prefix) ? "TestProperty" : $"{prefix}TestProperty";
            Assert.True(result.ContainsKey(expectedKey));
            Assert.Equal((1, 2), result[expectedKey]);
        }

        [Fact]
        public void CompareComplexTypesPropertyThrowsExceptionContinuesComparison()
        {
            var a = new TestClassWithExceptionProperty { NormalProperty = 1 };
            var b = new TestClassWithExceptionProperty { NormalProperty = 2 };

            var result = ModelDiffExtensions.CompareComplexTypes(a, b, "");

            Assert.Single(result);
            Assert.True(result.ContainsKey("NormalProperty"));
            Assert.Equal((1, 2), result["NormalProperty"]);
            Assert.False(result.ContainsKey("ExceptionProperty"));
        }

        [Fact]
        public void ModelDiffHandlesComplexScenarios()
        {
            var modelA = new
            {
                ListProp = new List<object> { "A", new ComplexType { Prop = "B" }, 1, null },
                NestedProp = new { InnerList = new List<int> { 1, 2, 3 } }
            };

            var modelB = new
            {
                ListProp = new List<object> { "A", new ComplexType { Prop = "C" }, 2, "D" },
                NestedProp = new { InnerList = new List<int> { 1, 2, 4 } }
            };

            var diff = modelA.ModelDiff(modelB);

            Assert.Equal(3, diff.Count);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.True(diff.ContainsKey("ListProp[1].Prop"));
            Assert.True(diff.ContainsKey("NestedPropInnerList"));
            Assert.Equal(("A,ComplexType,1,", "A,ComplexType,2,D"), diff["ListProp"]);
            Assert.Equal(("B", "C"), diff["ListProp[1].Prop"]);
            Assert.Equal(("1,2,3", "1,2,4"), diff["NestedPropInnerList"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Test")]
        [InlineData("Test.")]
        [InlineData("Test...")]
        public void CreateKeyHandlesVariousPrefixes(string prefix)
        {
            var result = ModelDiffExtensions.CreateKey(prefix);
            Assert.Equal(prefix.TrimEnd('.'), result);
        }

        [Fact]
        public void CreateKeyHandlesNullPrefix()
        {
            Assert.Throws<NullReferenceException>(() => ModelDiffExtensions.CreateKey(null));
        }

        [Fact]
        public void ModelDiffHandlesListWithComplexAndSimpleTypes()
        {
            var complexType = new ComplexType { Prop = "Test" };
            var modelA = new
            {
                ListProp = new List<object> { 1, "Simple", null, complexType }
            };

            var modelB = new
            {
                ListProp = new List<object> { 1, "Simple", complexType, null }
            };

            var diff = modelA.ModelDiff(modelB);

            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("1,Simple,,ComplexType", "1,Simple,ComplexType,"), diff["ListProp"]);
        }

        private class TestClass
        {
            public int TestProperty { get; set; }
        }

        private class TestClassWithExceptionProperty
        {
            public int NormalProperty { get; set; }
            public int ExceptionProperty => throw new InvalidOperationException("This property always throws an exception");
        }

        public class ComplexType
        {
            public string Prop { get; set; } = string.Empty;
        }
    }
}
#pragma warning restore CS8632
