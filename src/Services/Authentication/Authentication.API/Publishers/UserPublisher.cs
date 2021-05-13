using Authentication.API.Dtos;
using Authentication.API.Validators;
using EventBus.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Authentication.API.Publishers
{
    public class UserPublisher : IUserPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public UserPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishNewUserAsync(PublishNewUserRequest request)
        {
            var validator = new PublishNewUserRequestValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                throw new ArgumentException($"{typeof(PublishNewUserRequest)} is invalid. Errors: {string.Concat(result.Errors)}");
            }

            var newUserMessage = new NewUserEvent
            {
                UserId = request.UserId,
                Username = request.Username,
                Email = request.Email
            };

            await _publishEndpoint.Publish(newUserMessage);
        }
    }
}