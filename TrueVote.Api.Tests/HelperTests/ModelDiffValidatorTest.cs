using System;
using System.Collections;
using System.Collections.Generic;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
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

        [Fact]
        public void ModelDiff_HandlesDifferentLengthsWithComplexObjects()
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

            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("ComplexType,ComplexType", "ComplexType"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiff_HandlesMixedNullAndComplexObjects()
        {
            var modelA = new
            {
                ListProp = new List<ComplexType?>
                {
                    new ComplexType { Prop = "A" },
                    null
                }
            };

            var modelB = new
            {
                ListProp = new List<ComplexType?>
                {
                    new ComplexType { Prop = "A" },
                    new ComplexType { Prop = "B" }
                }
            };

            var diff = modelA.ModelDiff(modelB);

            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("ComplexType,", "ComplexType,ComplexType"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiff_HandlesListsOfSimpleTypesWithDifferentLengths()
        {
            var modelA = new
            {
                ListProp = new List<string> { "A", "B", "C" }
            };

            var modelB = new
            {
                ListProp = new List<string> { "A", "B" }
            };

            var diff = modelA.ModelDiff(modelB);

            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("A,B,C", "A,B"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiff_HandlesEmptyLists()
        {
            var modelA = new
            {
                ListProp = new List<string>()
            };

            var modelB = new
            {
                ListProp = new List<string> { "A" }
            };

            var diff = modelA.ModelDiff(modelB);

            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("", "A"), diff["ListProp"]);
        }

        [Fact]
        public void ModelDiff_HandlesListsWithAllNullItems()
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

            Assert.Empty(diff);
        }

        [Fact]
        public void ModelDiff_HandlesListsWithSomeNullItemsAndDifferentLengths()
        {
            var modelA = new
            {
                ListProp = new List<string?> { "A", null, "C" }
            };

            var modelB = new
            {
                ListProp = new List<string?> { "A", "B" }
            };

            var diff = modelA.ModelDiff(modelB);

            Assert.Single(diff);
            Assert.True(diff.ContainsKey("ListProp"));
            Assert.Equal(("A,,C", "A,B"), diff["ListProp"]);
        }

    }
}
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
