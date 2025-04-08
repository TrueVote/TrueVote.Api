using System;

namespace TrueVote.Api.Helpers
{
    public static class PrefixedGuid
    {
        public enum EntityType
        {
            Ballot = 'b',
            Election = 'e',
            User = 'u',
            Race = 'r',
            Candidate = 'c',
            Message = 'm',
            Role = 'o',  // 'r' is taken by Race
            Feedback = 'f',
            Timestamp = 't',
            Hash = 'h'
        }

        public static string NewPrefixedGuid(EntityType entityType)
        {
            return $"{(char)entityType}-{Guid.NewGuid()}";
        }

        public static bool IsValidPrefixedGuid(string prefixedGuid, EntityType expectedType)
        {
            if (string.IsNullOrEmpty(prefixedGuid) || prefixedGuid.Length < 3)
                return false;

            var prefix = prefixedGuid[0];
            if (prefixedGuid[1] != '-')
                return false;

            var guidPart = prefixedGuid[2..];

            return prefix == (char)expectedType && Guid.TryParse(guidPart, out _);
        }

        public static Guid ExtractGuid(string prefixedGuid)
        {
            if (string.IsNullOrEmpty(prefixedGuid) || prefixedGuid.Length < 3)
                throw new ArgumentException("Invalid prefixed GUID format");

            if (prefixedGuid[1] != '-')
                throw new ArgumentException("Invalid prefixed GUID format - missing dash");

            var guidPart = prefixedGuid[2..];
            if (!Guid.TryParse(guidPart, out var guid))
                throw new ArgumentException("Invalid GUID format");

            return guid;
        }

        public static EntityType GetEntityType(string prefixedGuid)
        {
            if (string.IsNullOrEmpty(prefixedGuid) || prefixedGuid.Length < 3)
                throw new ArgumentException("Invalid prefixed GUID format");

            if (prefixedGuid[1] != '-')
                throw new ArgumentException("Invalid prefixed GUID format - missing dash");

            var prefix = prefixedGuid[0];
            return (EntityType)prefix;
        }
    }
} 