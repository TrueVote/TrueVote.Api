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
            var pattern = @"https?://\S+$";
            var regex = new Regex(pattern);
            var match = regex.Match(@this);

            return match.Success ? match.Value : string.Empty;
        }

        public static bool IsJwt(this string @this)
        {
            var pattern = @"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_=]+$";
            var regex = new Regex(pattern);
            var match = regex.Match(@this);

            return match.Success;
        }
    }
}
