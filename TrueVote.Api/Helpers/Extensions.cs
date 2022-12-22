using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace TrueVote.Api
{
    [ExcludeFromCodeCoverage]
    public static partial class Extensions
    {
        public static string ToTitleCase(this string @this)
        {
            return new CultureInfo("en-US").TextInfo.ToTitleCase(@this);
        }
    }
}
