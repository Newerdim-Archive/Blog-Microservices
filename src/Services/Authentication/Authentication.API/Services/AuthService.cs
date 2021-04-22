using Authentication.API.Data;
using Authentication.API.Dtos;
using Authentication.API.Entities;
using Authentication.API.Enums;
using Authentication.API.Providers;
using Authentication.API.Validators;
using AutoMapper;
using System;
using System.Collections.Generic;
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
                throw new ArgumentException(string.Concat(result.Errors));
            }

            var isUserWithSameEmailInDb = _context.Users
                .Any(u => u.Email.ToLower() == request.Email.ToLower());

            if (isUserWithSameEmailInDb)
            {
                return new RegisterResult
                {
                    Result = RegisterResultMessage.EmailAlreadyExists,
                    UserId = 0
                };
            }

            var isUserWithSameUsernameInDb = _context.Users
                .Any(u => u.Username.ToLower() == request.Username.ToLower());

            if (isUserWithSameUsernameInDb)
            {
                return new RegisterResult
                {
                    Result = RegisterResultMessage.UsernameAlreadyExists,
                    UserId = 0
                };
            }

            var userToAdd = CreateNewUser(request);

            await _context.Users.AddAsync(userToAdd);
            await _context.SaveChangesAsync();

            return new RegisterResult
            {
                Result = RegisterResultMessage.Successful,
                UserId = userToAdd.Id
            };
        }

        private User CreateNewUser(RegisterRequest request)
        {
            var (passwordHash, passwordSalt) = HashWithSalt(request.Password);

            return new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                ConfirmedEmail = false,
                Created = _dateProvider.GetUtcNow(),
                LastChange = _dateProvider.GetUtcNow(),
            };
        }

        private static (byte[] hash, byte[] salt) HashWithSalt(string text)
        {
            using var hmac = new HMACSHA512();
            var passwordBytes = Encoding.UTF8.GetBytes(text);

            return (hmac.ComputeHash(passwordBytes), hmac.Key);
        }
    }
}
