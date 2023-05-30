using Microsoft.Azure.Cosmos.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Utf8Json;

namespace TrueVote.Api.Helpers
{
    public static class MerkleTree
    {
        private static readonly SHA256 s_sha256 = SHA256.Create();
        private static readonly ArrayPool<byte> s_bytePool = ArrayPool<byte>.Shared;

        // Calculates the Merkle root hash from a list of data
        public static byte[] CalculateMerkleRoot<T>(List<T> data)
        {
            if (data == null || data.Count == 0)
            {
                return null;
            }

            if (data.Count == 1)
            {
                return GetHash(data[0]);
            }

            // Convert each data item to its hash representation (leaf nodes)
            var leafNodes = data.Select(GetHash).ToList();

            // Build the Merkle tree by computing parent nodes until only the root remains
            while (leafNodes.Count > 1)
            {
                var parentNodes = new List<byte[]>();
                for (var i = 0; i < leafNodes.Count; i += 2)
                {
                    var left = leafNodes[i];
                    var right = (i + 1 < leafNodes.Count) ? leafNodes[i + 1] : left;

                    var parent = GetHash(left, right);
                    parentNodes.Add(parent);
                }
                leafNodes = parentNodes;
            }

            return leafNodes[0];
        }

        // Computes the hash of an object
        public static byte[] GetHash<T>(T value)
        {
            var serializer = Utf8Json.JsonSerializer.Serialize(value);
            return s_sha256.ComputeHash(serializer);
        }

        // Computes the hash of the concatenation of two byte arrays
        public static byte[] GetHash(byte[] left, byte[] right)
        {
            var buffer = s_bytePool.Rent(left.Length + right.Length);
            try
            {
                left.AsSpan().CopyTo(buffer);
                right.AsSpan().CopyTo(buffer.AsSpan(left.Length));

                return s_sha256.ComputeHash(buffer.AsSpan(0, left.Length + right.Length).ToArray());
            }
            finally
            {
                s_bytePool.Return(buffer);
            }
        }
    }
}
