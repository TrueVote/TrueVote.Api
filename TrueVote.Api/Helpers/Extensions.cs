using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TrueVote.Api
{
    [ExcludeFromCodeCoverage]
    public static partial class Extensions
    {
        public static string ToTitleCase(this string @this)
        {
            return new CultureInfo("en-US").TextInfo.ToTitleCase(@this);
        }

        public static string ExtractUrl(this string @this)
        {
            // Define the regular expression pattern
            var pattern = @"https?://\S+$";

            // Create a Regex object with the pattern
            var regex = new Regex(pattern);

            // Match the pattern against the input string
            var match = regex.Match(@this);

            // Check if a match is found
            return match.Success ? match.Value : string.Empty;
        }
    }
}
