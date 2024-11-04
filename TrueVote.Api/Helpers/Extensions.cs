using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Claims;
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

    [ExcludeFromCodeCoverage]
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            // Get the user ID from the JWT token
            // Dereference the ClaimTypes in an odd way because JwtRegisteredClaimNames doesn't work well.
            // Instead, getting this value from token creation code in JwtAuth.cs:
            // claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userId));
            var nameIdentifierClaims = principal.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).ToList();

            foreach (var claim in nameIdentifierClaims)
            {
                if (claim.Value != null && Guid.TryParse(claim.Value, out var userId))
                {
                    return userId;
                }
            }

            return Guid.Empty;
        }

        public static Guid GetUserIdOrThrow(this ClaimsPrincipal principal)
        {
            var userId = principal.GetUserId();
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }
    }

    [ExcludeFromCodeCoverage]
    public static class ModelBuilderExtensions
    {
        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
        {
            return propertyBuilder.HasConversion(
                v => v,
                v => v);
        }
    }
}
