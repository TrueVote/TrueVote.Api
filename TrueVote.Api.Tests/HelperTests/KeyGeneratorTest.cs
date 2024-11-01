using System.Collections.Generic;
using TrueVote.Api.Helpers;
using TrueVote.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace TrueVote.Api.Tests.HelperTests
{
    public class KeyGeneratorTests : TestHelper
    {
        private readonly UniqueKeyGenerator uniqueKeyGenerator;

        public KeyGeneratorTests(ITestOutputHelper output) : base(output)
        {
            uniqueKeyGenerator = new UniqueKeyGenerator();
        }

        [Fact]
        public void GenerateUniqueKeyReturnsValidString()
        {
            var key = uniqueKeyGenerator.GenerateUniqueKey();

            Assert.NotNull(key);
            Assert.True(key.Length is >= 12 and <= 16);
            Assert.Matches("^[A-Za-z0-9]+$", key);
        }

        [Fact]
        public void GenerateUniqueKeyGeneratesUniqueKeys()
        {
            var generatedKeys = new HashSet<string>();
            const int iterations = 10000;

            for (var i = 0; i < iterations; i++)
            {
                var key = uniqueKeyGenerator.GenerateUniqueKey();
                Assert.True(generatedKeys.Add(key), $"Duplicate key generated: {key}");
            }

            Assert.Equal(iterations, generatedKeys.Count);
        }

        [Fact]
        public void GenerateUniqueKeyDistributionTest()
        {
            var charCounts = new Dictionary<char, int>();
            const int iterations = 100000;

            for (var i = 0; i < iterations; i++)
            {
                var key = uniqueKeyGenerator.GenerateUniqueKey();
                foreach (var c in key)
                {
                    if (!charCounts.ContainsKey(c))
                        charCounts[c] = 0;
                    charCounts[c]++;
                }
            }

            foreach (var kvp in charCounts)
            {
                var percentage = (double) kvp.Value / (iterations * 14) * 100; // 14 is average length
                Assert.True(percentage is > 1 and < 3,
                    $"Character '{kvp.Key}' has unusual distribution: {percentage:F2}%");
            }
        }
    }
}
