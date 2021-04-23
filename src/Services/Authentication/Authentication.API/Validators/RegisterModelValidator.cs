using Authentication.API.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Validators
{
    public class RegisterModelValidator : AbstractValidator<RegisterModel>
    {
        public RegisterModelValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(32)
                .Matches(@"^[A-Za-z0-9]+(?:[ _-][A-Za-z0-9]+)*$")
                .WithMessage("{PropertyName} can only contains lower and upper case letters, numbers, space, dash and underscore");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6)
                .Matches(@"[A-Z]")
                .WithMessage("{PropertyName} must contains uppercase letter")
                .Matches(@"[a-z]")
                .WithMessage("{PropertyName} must contains lowercase letter")
                .Matches(@"[0-9]")
                .WithMessage("{PropertyName} must contains number");
        }
    }
}
