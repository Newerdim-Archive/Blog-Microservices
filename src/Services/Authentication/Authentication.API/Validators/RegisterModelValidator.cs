using Authentication.API.Extensions;
using Authentication.API.Models;
using FluentValidation;

namespace Authentication.API.Validators
{
    public class RegisterModelValidator : AbstractValidator<RegisterModel>
    {
        public RegisterModelValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .Username();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .Password();
        }
    }
}