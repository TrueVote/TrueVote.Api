using Newtonsoft.Json;
using System.Collections.Generic;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;
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
            var dataJson = JsonConvert.SerializeObject(MoqData.MockBallotData);

            var merkleRoot = MerkleTree.CalculateMerkleRoot(dataJson);

            Assert.NotNull(merkleRoot);
            Assert.Equal("91", merkleRoot[0].ToString());
        }

        [Fact]
        public void CreatesHashFromBallotObject()
        {
            var data1 = MoqData.MockBallotData;
            var data2 = MoqData.MockBallotData;

            var data1Json = JsonConvert.SerializeObject(data1);
            var data2Json = JsonConvert.SerializeObject(data2);

            Assert.Equal(data1Json, data2Json);

            var hash1 = MerkleTree.GetHash(data1Json);
            var hash2 = MerkleTree.GetHash(data2Json);

            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.Equal(hash1, hash2);
            Assert.Equal("203", hash1[0].ToString());
            Assert.Equal("203", hash2[0].ToString());
        }

        [Fact]
        public void HandlesNullString()
        {
            string dataString = null;

            var merkleRoot = MerkleTree.CalculateMerkleRoot(dataString);

            Assert.Null(merkleRoot);
        }

        [Fact]
        public void HandlesNullObject()
        {
            List<BallotModel> dataObject = null;

            var merkleRoot = MerkleTree.CalculateMerkleRoot(dataObject);

            Assert.Null(merkleRoot);
        }

        [Fact]
        public void HandlesEmptyObject()
        {
            var dataObject = new List<BallotModel>();

            var merkleRoot = MerkleTree.CalculateMerkleRoot(dataObject);

            Assert.Null(merkleRoot);
        }
    }
}
