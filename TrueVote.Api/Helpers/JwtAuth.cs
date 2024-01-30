using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;
using TrueVote.Api.Models;
using Microsoft.Extensions.Logging;
using TrueVote.Api.Services;

namespace TrueVote.Api.Helpers
{
    public interface IJwtHandler
    {
        Task<HttpResponseData> ValidateOrAbortAsync(HttpRequestData req);
        HttpResponseData ValidateOrAbort(HttpRequestData req);
        string GenerateToken(string userId, IEnumerable<string> roles);
    }

    public class JwtHandler : LoggerHelper, IJwtHandler
    {
        private const string Issuer = "TrueVoteApi";
        private const string Audience = "https://api.truevote.org/api/";
        private const double ExpiresValidityPeriod = 0.2;
        private const int TokenExpirationDays = 30;
        private readonly SymmetricSecurityKey SymmetricSecurityKey;
        private readonly SigningCredentials SigningCredentials;
        private readonly TimeSpan ClockSkew;

        public JwtHandler(ILogger log, IServiceBus serviceBus): base(log, serviceBus)
        {
            var secret = Environment.GetEnvironmentVariable("JWTSecret");
            var secretByte = Convert.FromBase64String(secret);
            SymmetricSecurityKey = new SymmetricSecurityKey(secretByte);
            SigningCredentials = new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            ClockSkew = TimeSpan.FromMinutes(1);
        }

        // Generate a JWT token with a Expiration, UserID, and Roles claims
        public string GenerateToken(string userId, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
            };

            // Add roles to the claims
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = Issuer,
                Audience = Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(TokenExpirationDays),
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

        public HttpResponseData ValidateOrAbort(HttpRequestData req)
        {
            try
            {
                var (principal, renewedToken) = ValidateAndRenewToken(req);
            }
            catch (SecurityTokenException e)
            {
                LogError(e.Message);
                LogDebug("HTTP trigger - GetStatus:End");
                return req.CreateUnauthorizedResponse(new SecureString { Value = e.Message });
            }
            catch (Exception e)
            {
                LogError(e.Message);
                LogDebug("HTTP trigger - GetStatus:End");
                return req.CreateBadRequestResponse(new SecureString { Value = e.Message });
            }

            return null;
        }

        public async Task<HttpResponseData> ValidateOrAbortAsync(HttpRequestData req)
        {
            try
            {
                var (principal, renewedToken) = ValidateAndRenewToken(req);
            }
            catch (SecurityTokenException e)
            {
                LogError($"{e.Message} : {e.InnerException}");
                LogDebug("HTTP trigger - GetStatus:End");
                return await req.CreateUnauthorizedResponseAsync(new SecureString { Value = $"{e.Message} : {e.InnerException}" });
            }
            catch (Exception e)
            {
                LogError($"{e.Message} : {e.InnerException}");
                LogDebug("HTTP trigger - GetStatus:End");
                return await req.CreateBadRequestResponseAsync(new SecureString { Value = $"{e.Message} : {e.InnerException}" });
            }

            return null;
        }

        public (ClaimsPrincipal, string) ValidateAndRenewToken(HttpRequestData req)
        {
            try
            {
                // Extract token from the request header
                if (!req.Headers.TryGetValues("Authorization", out var authHeaderEnumerable) || authHeaderEnumerable == null || !authHeaderEnumerable.Any())
                {
                    throw new SecurityTokenException("Token not found.");
                }

                var authHeader = authHeaderEnumerable.First(); // Assuming you only expect one value
                var token = authHeader.Replace("Bearer ", "");

                // Validate or auto-renew the token
                var principal = ValidateToken(token);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roles = principal.FindAll(ClaimTypes.Role)?.Select(c => c.Value);

                // Extract the expiration claim
                var expirationClaim = principal.FindFirst(ClaimTypes.Expiration)?.Value;
                if (expirationClaim != null && DateTime.TryParse(expirationClaim, out var expirationDateTime))
                {
                    var validFor = expirationDateTime - DateTimeOffset.UtcNow;

                    // Renew if expires within 20% of validity period
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
