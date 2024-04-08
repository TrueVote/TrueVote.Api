using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

/* 
 * This class shouldn't need to exist. The preferred method is to use the [Authorize] attribute on endpoints to
 * protect. However, that doesn't work with Azure Functions. There is no documentation or workaround that will enable
 * [Authorize] to work properly, hence the need for this class.
 * 
 * To protect a function, these lines need to be added to the top:
 *
    var (HttpResponse, renewedToken) = await _jwtHandler.ProcessTokenValidationAsync(req);
    if (HttpResponse != null)
        return HttpResponse;
 *
 * And something like this to the bottom:
 * 
    return await req.CreateOkResponseAsync(status, renewedToken);
 *
 */
namespace TrueVote.Api.Helpers
{
    public interface IJwtHandler
    {
        string GenerateToken(string userId, IEnumerable<string> roles);

        // Task<(HttpResponse Response, string RenewedToken)> ProcessTokenValidationAsync(HttpRequest req);
    }

    [ExcludeFromCodeCoverage] // TODO Add coverage for this
    public class JwtHandler : IJwtHandler
    {
        private const string Issuer = "TrueVoteApi";
        private const string Audience = "https://api.truevote.org/api/";
        private const double ExpiresValidityPeriod = 0.5;
        private const int TokenExpirationDays = 30;
        private readonly SymmetricSecurityKey symmetricSecurityKey;
        private readonly SigningCredentials signingCredentials;
        private readonly TimeSpan clockSkew;
        private readonly IConfiguration _configuration;

        public JwtHandler(IConfiguration configuration)
        {
            _configuration = configuration;
            var secret = _configuration["JWTSecret"];
            var secretByte = Convert.FromBase64String(secret);
            symmetricSecurityKey = new SymmetricSecurityKey(secretByte);
            signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            clockSkew = TimeSpan.FromMinutes(1);
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
            var expirationTime = currentTime.AddMinutes(5);
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
                SigningCredentials = signingCredentials
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
                    IssuerSigningKey = symmetricSecurityKey,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    ClockSkew = clockSkew
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

                var authHeader = authHeaderEnumerable[0] ?? throw new SecurityTokenException("AuthHeader is null.");
                var token = authHeader.Replace("Bearer ", string.Empty);

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

        /*
        public async Task<(HttpResponse Response, string RenewedToken)> ProcessTokenValidationAsync(HttpRequest req)
        {
            try
            {
                var (principal, token) = ValidateAndRenewToken(req);
                return (Response: null, RenewedToken: token);
            }
            catch (SecurityTokenException e)
            {
                LogError($"{e.Message} : {e.InnerException}");
                LogDebug("HTTP trigger - GetStatus:End");
                var unauthorizedResponse = await req.CreateUnauthorizedResponseAsync(new SecureString { Value = $"{e.Message} : {e.InnerException}" });
                return (Response: unauthorizedResponse, RenewedToken: null);
            }
            catch (Exception e)
            {
                LogError($"{e.Message} : {e.InnerException}");
                LogDebug("HTTP trigger - GetStatus:End");
                var badRequestResponse = await req.CreateBadRequestResponseAsync(new SecureString { Value = $"{e.Message} : {e.InnerException}" });
                return (Response: badRequestResponse, RenewedToken: null);
            }
        }
        */
    }
}
