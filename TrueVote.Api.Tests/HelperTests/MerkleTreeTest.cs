using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using Xunit;

namespace TrueVote.Api.Tests.HelperTests
{
    public class MerkleTreeTest
    {
        [Fact]
        public void CreatesMerkleRootFromSimpleString()
        {
            var testList = new List<string>
            {
                "A test string"
            };

            var merkleRoot = MerkleTree.CalculateMerkleRoot(testList);

            Assert.NotNull(merkleRoot);
            Assert.Equal("56", merkleRoot[0].ToString());
        }

        [Fact]
        public void CreatesMerkleRootFromMultipleStrings()
        {
            var testList = new List<string>
            {
                "A test string - 1",
                "A test string - 2",
                "A test string - 3",
                "A test string - 4",
                "A test string - 5",
                "A test string - 6",
                "A test string - 7",
            };

            var merkleRoot = MerkleTree.CalculateMerkleRoot(testList);

            Assert.NotNull(merkleRoot);
            Assert.Equal("140", merkleRoot[0].ToString());
        }

        [Fact]
        public void CreatesMerkleRootFromBallotObject()
        {
            var merkleRoot = MerkleTree.CalculateMerkleRoot(MoqData.MockBallotData);

            Assert.NotNull(merkleRoot);
            Assert.Equal("147", merkleRoot[0].ToString());
        }

        [Fact]
        public void CreatesHashFromBallotObject()
        {
            var data1 = MoqData.MockBallotData;
            var data2 = MoqData.MockBallotData;
            Assert.Equal(data1, data2);

            var hash1 = MerkleTree.GetHash(data1);
            var hash2 = MerkleTree.GetHash(data1);

            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.Equal(hash1, hash2);
            Assert.Equal("126", hash1[0].ToString());
        }
    }
}
