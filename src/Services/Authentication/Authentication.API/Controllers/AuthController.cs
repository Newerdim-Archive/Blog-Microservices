﻿using Authentication.API.Dtos;
using Authentication.API.Enums;
using Authentication.API.Helpers;
using Authentication.API.Models;
using Authentication.API.Responses;
using Authentication.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Controllers
{
    [Route(AuthControllerRoutes.Controller)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="model"></param>
        /// <returns>The id of newly created user with message</returns>
        /// <response code="200">Returns the id of newly created user with message</response>
        /// <response code="400">If model is invalid</response>
        /// <response code="401">If user with the same credentials exists</response>
        [HttpPost(AuthControllerRoutes.Register)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegisterResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var registerResult = await _authService.RegisterAsync(new RegisterRequest
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password
            });

            switch (registerResult.Message)
            {
                case RegisterResultMessage.Successful:
                    break;

                case RegisterResultMessage.EmailAlreadyExists:
                    return Unauthorized(ResponseMessage.EmailAlreadyExists);

                case RegisterResultMessage.UsernameAlreadyExists:
                    return Unauthorized(ResponseMessage.UsernameAlreadyExists);

                default:
                    throw new NotImplementedException($"Register does not implement" +
                        $" result message with name: " +
                        $"{nameof(registerResult.Message)}");
            }

            return Ok(new RegisterResponse
            {
                Message = ResponseMessage.RegisteredSuccessfully,
                UserId = registerResult.UserId
            });
        }

        /// <summary>
        /// Login existing user
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Access token and message</returns>
        /// <response code="200">Returns access token and message</response>
        /// <response code="400">If model is invalid</response>
        /// <response code="401">If user not exists or invalid credentials</response>
        [HttpPost(AuthControllerRoutes.Login)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var request = new LoginRequest
            {
                Username = model.Username,
                Password = model.Password
            };

            var result = await _authService.LoginAsync(request);

            switch (result.Message)
            {
                case LoginResultMessage.Successful:
                    break;

                case LoginResultMessage.PasswordNotMatch:
                    return Unauthorized(ResponseMessage.PasswordNotMatch);

                case LoginResultMessage.UserNotExist:
                    return Unauthorized(ResponseMessage.UserNotExist);

                default:
                    throw new NotImplementedException(
                        $"Login does not implement result message with name: " +
                        $"{nameof(result.Message)}");
            }

            var accessToken = await _tokenService.CreateAccessTokenAsync(result.UserId);

            var refreshToken = await _tokenService.CreateRefreshTokenAsync(result.UserId);

            AddRefreshTokenToCookies(refreshToken);

            return Ok(new LoginResponse
            {
                Message = ResponseMessage.LoggedInSuccessfully,
                UserId = result.UserId,
                AccessToken = accessToken
            });
        }

        /// <summary>
        /// Refresh access and refresh token
        /// </summary>
        /// <returns>Access token</returns>
        /// <response code="200">Returns access token with message</response>
        /// <response code="401">If token is invalid or not exist</response>
        [HttpPost(AuthControllerRoutes.RefreshTokens)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RefreshTokensResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<IActionResult> RefreshTokens()
        {
            var refreshTokenFromRequest = GetRefreshTokenFromRequest();

            if (refreshTokenFromRequest is null)
            {
                return Unauthorized(ResponseMessage.RefreshTokenIsNullOrEmpty);
            }

            var isValidRefreshTokenFromRequest = await _tokenService
                .IsValidRefreshTokenAsync(refreshTokenFromRequest);

            if (!isValidRefreshTokenFromRequest)
            {
                return Unauthorized(ResponseMessage.RefreshTokenIsInvalid);
            }

            var userId = _tokenService.GetUserIdFromToken(refreshTokenFromRequest);

            var refreshToken = await _tokenService.CreateRefreshTokenAsync(userId);

            var accessToken = await _tokenService.CreateAccessTokenAsync(userId);

            AddRefreshTokenToCookies(refreshToken);

            return Ok(new RefreshTokensResponse
            {
                Message = ResponseMessage.TokensRefreshedSuccessfully,
                AccessToken = accessToken
            });
        }

        #region Private Methods

        private void AddRefreshTokenToCookies(string token)
        {
            var cookieName = CookieName.RefreshToken;

            var cookieOptions = new CookieOptions { HttpOnly = true };

            HttpContext.Response.Cookies.Append(cookieName, token, cookieOptions);
        }

        /// <summary>
        /// Get refresh token from request
        /// </summary>
        /// <returns>Refresh token if exists and is not null or empty, otherwise null</returns>
        private string GetRefreshTokenFromRequest()
        {
            var tokenExists = Request.Cookies
                .TryGetValue(CookieName.RefreshToken, out var refreshToken);

            if (!tokenExists)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            return refreshToken;
        }

        #endregion Private Methods
    }
}