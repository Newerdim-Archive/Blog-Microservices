using Authentication.API.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
