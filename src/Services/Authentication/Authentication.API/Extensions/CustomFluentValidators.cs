using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.API.Extensions
{
    public static class CustomFluentValidators
    {
        public static IRuleBuilderOptions<T, string> Username<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .MinimumLength(3)
                .MaximumLength(32)
                .Matches(@"^[A-Za-z0-9]+(?:[ _-][A-Za-z0-9]+)*$")
                .WithMessage("{PropertyName} can only contain lower and upper case letters, numbers, spaces, dashes and underscores");
        }

        public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder, bool specialCharacters = false)
        {
            var rules = ruleBuilder
                .MinimumLength(6)
                .Matches(@"[A-Z]")
                .WithMessage("{PropertyName} must contain a uppercase letter")
                .Matches(@"[a-z]")
                .WithMessage("{PropertyName} must contain a lowercase letter")
                .Matches(@"[0-9]")
                .WithMessage("{PropertyName} must contain a number");

            if (specialCharacters)
            {
                rules
                    .Matches(@"[!@#$%^&*()]-+=.,/\[]{}<>'"";:`")
                    .WithMessage("{PropertyName} must contain a special character");
            }

            return rules;
        }

        public static IRuleBuilderOptions<T, string> Url<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
                .WithMessage("{PropertyName} must be valid URL");
        }
                        
    }
}
