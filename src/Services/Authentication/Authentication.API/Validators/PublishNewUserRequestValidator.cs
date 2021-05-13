using Authentication.API.Dtos;
using FluentValidation;

namespace Authentication.API.Validators
{
    public class PublishNewUserRequestValidator : AbstractValidator<PublishNewUserRequest>
    {
        public PublishNewUserRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.UserId).NotEqual(0);
            RuleFor(x => x.Username).NotEmpty();
        }
    }
}