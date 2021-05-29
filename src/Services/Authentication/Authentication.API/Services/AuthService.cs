using Authentication.API.Data;
using Authentication.API.Dtos;
using Authentication.API.Entities;
using Authentication.API.Enums;
using Authentication.API.Providers;
using Authentication.API.Validators;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDataContext _context;
        private readonly IDateProvider _dateProvider;

        public AuthService(AuthDataContext context, IDateProvider dateProvider)
        {
            _context = context;
            _dateProvider = dateProvider;
        }

        public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
        {
            var validator = new RegisterRequestValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                throw new ArgumentException($"{typeof(RegisterRequest)} is invalid. Errors: {string.Concat(result.Errors)}");
            }

            var isUserWithSameEmailInDb = _context.Users
                .Any(u => u.Email.ToLower() == request.Email.ToLower());

            if (isUserWithSameEmailInDb)
            {
                return new RegisterResult
                {
                    Message = RegisterResultMessage.EmailAlreadyExists,
                    UserId = 0
                };
            }

            var isUserWithSameUsernameInDb = _context.Users
                .Any(u => u.Username.ToLower() == request.Username.ToLower());

            if (isUserWithSameUsernameInDb)
            {
                return new RegisterResult
                {
                    Message = RegisterResultMessage.UsernameAlreadyExists,
                    UserId = 0
                };
            }

            var userToAdd = CreateNewUser(request);

            await _context.Users.AddAsync(userToAdd);
            await _context.SaveChangesAsync();

            return new RegisterResult
            {
                Message = RegisterResultMessage.Successful,
                UserId = userToAdd.Id
            };
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var userInDb = await GetUserByUsernameCaseInsensitive(request.Username);

            if (userInDb is null)
            {
                return new LoginResult
                {
                    Message = LoginResultMessage.UserNotExist
                };
            }

            var isPasswordMatch = IsPasswordMatch(request, userInDb);

            if (!isPasswordMatch)
            {
                return new LoginResult
                {
                    Message = LoginResultMessage.PasswordNotMatch
                };
            }

            return new LoginResult
            {
                Message = LoginResultMessage.Successful,
                UserId = userInDb.Id
            };
        }

        #region Private Methods

        private User CreateNewUser(RegisterRequest request)
        {
            var (passwordHash, passwordSalt) = HashWithSalt(request.Password);

            return new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Created = _dateProvider.GetUtcNow(),
                LastChange = _dateProvider.GetUtcNow(),
            };
        }

        /// <summary>
        /// Hash text using salt
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static (byte[] hash, byte[] salt) HashWithSalt(string text)
        {
            using var hmac = new HMACSHA512();
            var passwordBytes = Encoding.UTF8.GetBytes(text);

            return (hmac.ComputeHash(passwordBytes), hmac.Key);
        }

        /// <summary>
        /// Get user by <paramref name="username"/> case-insensitive
        /// </summary>
        /// <param name="username"></param>
        /// <returns>User if exists, otherwise null</returns>
        private async Task<User> GetUserByUsernameCaseInsensitive(string username)
        {
            username = username.ToLower();

            return await _context.Users
                .FirstOrDefaultAsync(x => x.Username.ToLower() == username);
        }

        /// <summary>
        /// Check if <paramref name="request"/> password match
        /// <paramref name="user"/> password
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns>True if password match, otherwise false</returns>
        private static bool IsPasswordMatch(LoginRequest request, User user)
        {
            return VerifyHashWithSalt(
                request.Password,
                user.PasswordHash,
                user.PasswordSalt);
        }

        /// <summary>
        /// Verify if hashed <paramref name="text"/> with <paramref name="salt"/>
        /// is equal to <paramref name="hash"/> with <paramref name="salt"/>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="hash"></param>
        /// <param name="salt"></param>
        /// <returns>True if equals, otherwise false</returns>
        private static bool VerifyHashWithSalt(string text, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var textBytes = Encoding.UTF8.GetBytes(text);
            var computedHash = hmac.ComputeHash(textBytes);

            return hash.SequenceEqual(computedHash);
        }

        #endregion
    }
}