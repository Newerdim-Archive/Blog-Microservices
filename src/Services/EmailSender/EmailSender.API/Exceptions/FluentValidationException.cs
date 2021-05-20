using FluentValidation.Results;
using System;

namespace EmailSender.API.Exceptions
{
    public class FluentValidationException : Exception
    {
        public FluentValidationException(Type testedClass, ValidationResult validationResult, string paramName)
            : base($"{testedClass} is invalid. Errors: {string.Concat(validationResult.Errors)}. Parameter name: {paramName}")
        {

        }
    }
}
