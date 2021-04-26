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
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly AuthDataContext _context;
        private readonly IDateProvider _dateProvider;
        private readonly TokenSettings _tokenSettings;

        public TokenService(
            AuthDataContext context, 
            IDateProvider dateProvider,
            IOptions<TokenSettings> tokenOptions)
        {
            _context = context;
            _dateProvider = dateProvider;
            _tokenSettings = tokenOptions.Value;
        }

        public async Task<string> CreateEmailConfirmationTokenAsync(int userId)
        {
            if (userId == 0)
            {
                throw new ArgumentException("UserId cannot be 0");
            }

            var userInDb = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (userInDb is null)
            {
                throw new ArgumentException($"User with userId={userId} not exists");
            }

            var claims = new[]
            {
                new Claim(CustomClaimTypes.UserId, userId.ToString()),
                new Claim(CustomClaimTypes.Reason, TokenReasons.EmailConfirmation),
            };

            return CreateToken(claims, _tokenSettings.EmailConfirmationSecret, null);
        }

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

        public Task<bool> IsValidEmailConfirmationTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException($"'{nameof(token)}' cannot be null or whitespace.", nameof(token));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters(false, _tokenSettings.EmailConfirmationSecret);

            string tokenReason;

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                tokenReason = principal.Claims.First(x => x.Type == CustomClaimTypes.Reason).Value;
            }
            catch
            {
                return Task.FromResult(false);
            }

            if (tokenReason is not TokenReasons.EmailConfirmation)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
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

        public int GetUserIdFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException($"'{nameof(token)}' cannot be null or whitespace.", nameof(token));
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
            {
                return 0;
            }

            var claims = tokenHandler.ReadJwtToken(token).Claims;

            var userIdValue = claims.FirstOrDefault(c => c.Type == CustomClaimTypes.UserId)?.Value;

            var result = int.TryParse(userIdValue, out var userId);

            return result ? userId : 0;
        }
    }
}
