using System;
using System.Threading.Tasks;

namespace Authentication.API.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Get user id from token
        /// </summary>
        /// <param name="token"></param>
        /// <returns>User id</returns>\
        /// <exception cref="ArgumentNullException">Throws when userId not exist in token</exception>
        /// <exception cref="ArgumentException">Throws when token is empty, null or invalid</exception>
        int GetUserIdFromToken(string token);

        /// <summary>
        /// Create access token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Token</returns>
        Task<string> CreateAccessTokenAsync(int userId);

        /// <summary>
        /// Create refresh token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Token</returns>
        Task<string> CreateRefreshTokenAsync(int userId);
    }
}