using Authentication.API.Data;
using Authentication.API.Helpers;
using Authentication.API.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IDateProvider _dateProvider;
        private readonly TokenSettings _tokenSettings;

        public TokenService(
            IDateProvider dateProvider,
            IOptions<TokenSettings> tokenOptions)
        {
            _dateProvider = dateProvider;
            _tokenSettings = tokenOptions.Value;
        }

        public int GetUserIdFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException($"'{nameof(token)}' cannot be null or whitespace", nameof(token));
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
            {
                throw new ArgumentException($"'{nameof(token)}' is invalid", nameof(token));
            }

            var claims = tokenHandler.ReadJwtToken(token).Claims;

            var userIdClaim = claims.FirstOrDefault(c => c.Type == CustomClaimTypes.UserId);

            if (userIdClaim is null)
            {
                throw new ArgumentException($"'{nameof(token)}' does not have userId", nameof(token));
            }

            var isValidUserId = int.TryParse(userIdClaim.Value, out var userId);

            if (!isValidUserId)
            {
                throw new ArgumentException($"'{nameof(userId)}' is invalid", nameof(token));
            }

            return userId;
        }

        public Task<string> CreateAccessTokenAsync(int userId)
        {
            var claims = new Claim[]
            {
                new Claim(CustomClaimTypes.UserId, userId.ToString()),
            };

            var secret = _tokenSettings.AccessTokenSecret;

            var expires = _dateProvider.GetAfterUtcNow(0, 15);

            var token = CreateToken(claims, secret, expires);

            return Task.FromResult(token);
        }

        public Task<string> CreateRefreshTokenAsync(int userId)
        {
            var claims = new Claim[]
            {
                new Claim(CustomClaimTypes.UserId, userId.ToString()),
            };

            var secret = _tokenSettings.RefreshTokenSecret;

            var expires = _dateProvider.GetAfterUtcNow(15, 0);

            var token = CreateToken(claims, secret, expires);

            return Task.FromResult(token);
        }

        #region Private Methods

        /// <summary>
        /// Create token
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="secret"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        private static string CreateToken(Claim[] claims, string secret, DateTimeOffset? expires)
        {
            var key = Encoding.UTF8.GetBytes(secret);
            var symmetricKey = new SymmetricSecurityKey(key);

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires?.DateTime,
                SigningCredentials = new SigningCredentials(
                    symmetricKey,
                    SecurityAlgorithms.HmacSha512Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(securityToken);
        }

        private static TokenValidationParameters GetValidationParameters(bool validateLifetime, string secret)
        {
            var key = Encoding.UTF8.GetBytes(secret);

            return new TokenValidationParameters()
            {
                ValidateLifetime = validateLifetime,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        }

        #endregion
    }
}