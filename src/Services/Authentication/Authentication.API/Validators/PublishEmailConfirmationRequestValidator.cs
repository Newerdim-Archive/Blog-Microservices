using Authentication.API.Dtos;
using FluentValidation;

namespace Authentication.API.Validators
{
    public class PublishEmailConfirmationRequestValidator : AbstractValidator<PublishEmailConfirmationRequest>
    {
        public PublishEmailConfirmationRequestValidator()
        {
            RuleFor(x => x.TargetEmail).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.EmailConfirmationUrl).NotEmpty();
        }
    }
}