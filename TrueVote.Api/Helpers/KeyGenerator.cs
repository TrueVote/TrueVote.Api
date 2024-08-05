using System.Security.Cryptography;

#pragma warning disable SCS0005 // Weak random number generator.
namespace TrueVote.Api.Helpers
{
    public static class UniqueKeyGenerator
    {
        private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private static readonly Random random = new();

        public static string GenerateUniqueKey()
        {
            var length = random.Next(12, 17); // Random length between 12 and 16
            var randomBytes = new byte[length];

            RandomNumberGenerator.Fill(randomBytes);

            var chars = new char[length];

            for (var i = 0; i < length; i++)
            {
                var index = randomBytes[i] % AllowedChars.Length;
                chars[i] = AllowedChars[index];
            }

            return new string(chars);
        }
    }
}
#pragma warning restore SCS0005 // Weak random number generator.
