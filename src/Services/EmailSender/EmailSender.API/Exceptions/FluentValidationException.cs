using FluentValidation.Results;
using System;

namespace EmailSender.API.Exceptions
{
    public class FluentValidationException : Exception
    {
        /// <summary>
        /// Exception for invalid fluent validation
        /// </summary>
        /// <param name="testedClass"></param>
        /// <param name="validationResult"></param>
        /// <param name="paramName"></param>
        /// <exception cref="NullReferenceException">Throws when <paramref name="validationResult"/> is null</exception>
        public FluentValidationException(Type testedClass, ValidationResult validationResult, string paramName)
            : base($"{testedClass} is invalid. Errors: {string.Concat(validationResult.Errors)}. Parameter name: {paramName}")
        {

        }
    }
}
