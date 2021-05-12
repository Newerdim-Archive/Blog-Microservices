using Authentication.API.Extensions;
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
                .Username();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .Password();

            RuleFor(x => x.EmailConfirmationUrl)
                .NotEmpty()
                .Url();
        }
    }
}
