using Authentication.API.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
