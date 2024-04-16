using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Primitives;

namespace TrueVote.Api.Helpers
{
    public interface IJwtHandler
    {
        string GenerateToken(string userId, IEnumerable<string> roles);
        (ClaimsPrincipal, string) ValidateAndRenewToken(HttpRequest req);
    }

    [ExcludeFromCodeCoverage] // TODO Add coverage for this
    public class JwtHandler : IJwtHandler
    {
        private const string Issuer = "TrueVoteApi";
        private const string Audience = "https://api.truevote.org/api/";
        private const double ExpiresValidityPeriod = 0.5;
        private const int TokenExpirationDays = 30;
        private readonly SymmetricSecurityKey SymmetricSecurityKey;
        private readonly SigningCredentials SigningCredentials;
        private readonly TimeSpan ClockSkew;
        private readonly IConfiguration _configuration;

        public JwtHandler(IConfiguration configuration)
        {
            _configuration = configuration;
            var secret = _configuration["JWTSecret"];
            var secretByte = Convert.FromBase64String(secret);
            SymmetricSecurityKey = new SymmetricSecurityKey(secretByte);
            SigningCredentials = new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            ClockSkew = TimeSpan.FromMinutes(1);
        }

        // Generate a JWT token with a Expiration, UserID, and Roles claims
        public string GenerateToken(string userId, IEnumerable<string> roles)
        {
            if (userId == null)
            {
                throw new SecurityTokenException("Token validation failed. userId cannot be null");
            }

            var claims = new List<Claim>();

            // Add roles to the claims
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var currentTime = DateTimeOffset.UtcNow;
            var expirationTime = currentTime.AddDays(TokenExpirationDays);
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, expirationTime.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            // Other essential claims
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId));
            claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userId));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iss, Issuer));
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, Audience));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, currentTime.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, currentTime.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Other token descriptor properties...
                Subject = new ClaimsIdentity(claims),
                Expires = expirationTime.UtcDateTime,
                SigningCredentials = SigningCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Validate a JWT token in the request header
        private ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = SymmetricSecurityKey,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    ClockSkew = ClockSkew
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Token validation failed.", ex);
            }
        }

        public (ClaimsPrincipal, string) ValidateAndRenewToken(HttpRequest req)
        {
            try
            {
                // Extract token from the request header
                if (!req.Headers.TryGetValue("Authorization", out var authHeaderEnumerable) || StringValues.IsNullOrEmpty(authHeaderEnumerable))
                {
                    throw new SecurityTokenException("Token not found.");
                }

                var authHeader = authHeaderEnumerable.First() ?? throw new SecurityTokenException("AuthHeader is null.");
                var token = authHeader.Replace("Bearer ", "");

                // Validate or auto-renew the token
                var principal = ValidateToken(token);

                // THIS should work, but it doesn't. So instead need to fully qualify claim name.
                // var userId = principal.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
                var userId = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                var roles = principal.FindAll(ClaimTypes.Role)?.Select(c => c.Value);

                // Extract the expiration claim
                var expirationClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
                if (expirationClaim != null && long.TryParse(expirationClaim, out var expirationUnixTime))
                {
                    // Convert Unix time to DateTime
                    var expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(expirationUnixTime).UtcDateTime;

                    var validFor = expirationDateTime - DateTimeOffset.UtcNow;

                    // TODO - Confirm this is working!!
                    // Renew if expires within x% of validity period
                    var renewalPeriod = validFor.TotalSeconds * ExpiresValidityPeriod;

                    if (validFor < TimeSpan.FromSeconds(renewalPeriod))
                    {
                        // If the token is about to expire, generate a new token with the same UserID and Roles
                        return (principal, GenerateToken(userId, roles));
                    }
                }

                return (principal, token);
            }
            catch (SecurityTokenException e)
            {
                throw new SecurityTokenException("Token validation failed.", e);
            }
            catch (Exception e)
            {
                throw new Exception("An unexpected error occurred.", e);
            }
        }
    }
}
