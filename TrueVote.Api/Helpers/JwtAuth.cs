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

    public class JwtHandler(ILogger log, IServiceBus serviceBus) : LoggerHelper(log, serviceBus), IJwtHandler
    {
        private const string Issuer = "TrueVoteApi";
        private const string Audience = "https://api.truevote.org/api/";

        // Generate a JWT token with a Expiration, UserID, and Roles claims
        public string GenerateToken(string userId, IEnumerable<string> roles)
        {
            var SecretKey = Environment.GetEnvironmentVariable("JWTSecret");
            var key = Convert.FromBase64String(SecretKey);
            // var key = Encoding.UTF8.GetBytes(SecretKey);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

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
                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = signingCredentials,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Validate a JWT token in the request header
        private static ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var SecretKey = Environment.GetEnvironmentVariable("JWTSecret");
                var key = Convert.FromBase64String(SecretKey);
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    ClockSkew = TimeSpan.Zero, // No clock skew for simplicity, adjust as needed
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
                    // Check if the token is about to expire (e.g., within the last 5 minutes)
                    if (expirationDateTime < DateTime.UtcNow.AddMinutes(5))
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
