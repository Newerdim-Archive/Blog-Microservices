using Authentication.API.Dtos;
using Authentication.API.Enums;
using Authentication.API.Helpers;
using Authentication.API.Models;
using Authentication.API.Publishers;
using Authentication.API.Responses;
using Authentication.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Authentication.API.Controllers
{
    [Route(AuthControllerRoutes.Controller)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IUserPublisher _userPublisher;
        private readonly IEmailPublisher _emailPublisher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ITokenService tokenService,
            IUserPublisher userPublisher,
            IEmailPublisher emailPublisher,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _userPublisher = userPublisher;
            _emailPublisher = emailPublisher;
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
                case RegisterResultMessage.EmailAlreadyExists:
                    return Unauthorized("User with this email already exists");

                case RegisterResultMessage.UsernameAlreadyExists:
                    return Unauthorized("User with this username already exists");
            }

            var emailConfiramtionToken = await _tokenService
                .CreateEmailConfirmationTokenAsync(registerResult.UserId);

            await _emailPublisher.PublishEmailConfirmationAsync(new PublishEmailConfirmationRequest
            {
                TargetEmail = model.Email,
                Token = emailConfiramtionToken,
                EmailConfirmationUrl = model.EmailConfirmationUrl
            });

            await _userPublisher.PublishNewUserAsync(new PublishNewUserRequest
            {
                UserId = registerResult.UserId,
                Username = model.Username,
                Email = model.Email
            });

            return Ok(new RegisterResponse
            {
                Message = "Register successful",
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
                    return Unauthorized("Password does not match");

                case LoginResultMessage.UserNotExist:
                    return Unauthorized("User does not exist");

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
                Message = "Logged in successfully",
                UserId = result.UserId,
                AccessToken = accessToken
            });
        }

        private void AddRefreshTokenToCookies(string token)
        {
            var cookieName = "refresh_token";
            
            var cookieOptions = new CookieOptions { HttpOnly = true };

            HttpContext.Response.Cookies.Append(cookieName, token, cookieOptions);
        }
    }
}