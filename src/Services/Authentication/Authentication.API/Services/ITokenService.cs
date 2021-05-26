using System;
using System.Threading.Tasks;

namespace Authentication.API.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Create token for email confirmation
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Token</returns>
        /// <exception cref="ArgumentException">Throws when userId is 0 or user with that id not exists</exception>
        Task<string> CreateEmailConfirmationTokenAsync(int userId);

        /// <summary>
        /// Check if token for email confirmation is valid
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if token is valid otherwise false</returns>
        /// <exception cref="ArgumentException">Throws when token is null or empty</exception>
        Task<bool> IsValidEmailConfirmationTokenAsync(string token);

        /// <summary>
        /// Get user id from token
        /// </summary>
        /// <param name="token"></param>
        /// <returns>User id if exists otherwise 0</returns>
        int GetUserIdFromToken(string token);

        /// <summary>
        /// Create access token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Token</returns>
        /// <exception cref="ArgumentException">Throws when userId is 0 or user with that id not exists</exception>
        Task<string> CreateAccessTokenAsync(int userId);

        /// <summary>
        /// Create refresh token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Token</returns>
        /// <exception cref="ArgumentException">Throws when userId is 0 or user with that id not exists</exception>
        Task<string> CreateRefreshTokenAsync(int userId);

        /// <summary>
        /// Check if refresh token is valid and has all required claims
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if token is valid, otherwise false</returns>
        /// <exception cref="ArgumentException">Throws when token is null or empty</exception>
        Task<bool> IsValidRefreshTokenAsync(string token);
    }
}