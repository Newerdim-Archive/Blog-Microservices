using Authentication.API.Dtos;
using Authentication.API.Enums;
using Authentication.API.Helpers;
using Authentication.API.Models;
using Authentication.API.Responses;
using Authentication.API.Services;
using EventBus.Events;
using MassTransit;
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
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IPublishEndpoint publishEndpoint, ILogger<AuthController> logger)
        {
            _authService = authService;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /Register
        ///     {
        ///        "username": "User1234"
        ///        "email": "User1234@email.com"
        ///        "password": "User1234$@"
        ///     }
        ///     
        /// </remarks>
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
            var request = new RegisterRequest
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password
            };

            var result = await _authService.RegisterAsync(request);

            switch (result.Message)
            {
                case RegisterResultMessage.EmailAlreadyExists:
                    return Unauthorized("User with this email already exists");

                case RegisterResultMessage.UsernameAlreadyExists:
                    return Unauthorized("User with this username already exists");
            }

            await PublishNewUserEvent(result.UserId, request.Username, request.Email);

            var response = new RegisterResponse
            {
                Message = "Register successful",
                UserId = result.UserId
            };

            return Ok(response);
        }

        private async Task PublishNewUserEvent(int userId, string username, string email)
        {
            var newUserEvent = new NewUserEvent
            {
                UserId = userId,
                Username = username,
                Email = email
            };

            await _publishEndpoint.Publish(newUserEvent);
        }
    }
}
