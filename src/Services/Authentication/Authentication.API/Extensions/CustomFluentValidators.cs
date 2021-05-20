using FluentValidation;
using System;

namespace Authentication.API.Extensions
{
    public static class CustomFluentValidators
    {
        /// <summary>
        /// Validate username length and regex
        /// <para>Note: Does not have empty check!</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleBuilder"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, string> Username<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .MinimumLength(3)
                .MaximumLength(32)
                .Matches(@"^[\w -]+$")
                .WithMessage("{PropertyName} can only contain upper and lower case letters, numbers, spaces, hyphens, and underscores ");
        }

        /// <summary>
        /// Validate password length and regex
        /// <para>Note: Does not have empty check!</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleBuilder"></param>
        /// <param name="specialCharacters">Check for special characters</param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, string> Password<T>(
            this IRuleBuilder<T, string> ruleBuilder, 
            bool specialCharacters = false)
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
                    .Matches(@"[\W_]")
                    .WithMessage("{PropertyName} must contain a special character");
            }

            return rules;
        }

        /// <summary>
        /// Validate URL
        /// <para>Note: Does not have empty check!</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleBuilder"></param>
        /// <param name="uriKind"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, string> Url<T>(this IRuleBuilder<T, string> ruleBuilder, UriKind uriKind = UriKind.Absolute)
        {
            return ruleBuilder
                .Must(x => Uri.IsWellFormedUriString(x, uriKind))
                .WithMessage("{PropertyName} must be valid URL");
        }
    }
}